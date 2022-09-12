module SubscribeToFeedWorkflow

open System.Threading.Tasks
open Persistence
open FeedReaderAdapter
open Dto

let private convertToWorkflowError r: Result<_, WorkflowError> =
    match r with
    | Ok r -> Ok r
    | Error (BaseDiscoveryError.FetchError e) -> BadRequest (e.Message, Some e) |> Error

let previewSubscribeToFeed (feedReader: FeedReaderAdapter) (query: PreviewSubscribeToFeedQueryDto): Task<Result<Result<PreviewSubscribeToFeedResponseDto, FeedError> list, WorkflowError>> =
    let aggregateResults (results: (Result<DiscoveredFeed, _>) list): Result<PreviewSubscribeToFeedResponseDto, FeedError> list =
        [
            for r in results do
                match r with
                | Ok feed ->
                    yield Ok {
                        Title = feed.Title
                        Url = feed.Url
                        Type =
                            match feed.FeedType with
                            | FeedReaderAdapter.FeedType.Atom -> FeedType.Atom
                            | FeedReaderAdapter.FeedType.Rss -> FeedType.Rss
                    }
                | _ -> ()
        ]
        
    feedReader.discoverFeeds query.Url
    |> TaskResult.map aggregateResults
    |> Task.map (fun e -> convertToWorkflowError e)

type SubscribeToFeedError =
    | FeedError of FeedReaderAdapter.FeedError
    | SubscriptionError of string list

let private convertToWorkflowError2 r: Result<_, WorkflowError> =
    match r with
    | Ok r -> Ok r
    | Error (FeedError (FeedReaderAdapter.FetchError e)) -> BadRequest (e.Message, Some e) |> Error
    | Error (FeedError (FeedReaderAdapter.ParseError e)) -> BadRequest (e.Message, Some e) |> Error
    | Error (SubscriptionError errors) -> BadRequest (sprintf "%A" errors, None) |> Error

let subscribeToFeed (feedReader: FeedReaderAdapter) (repository: SubscriptionRepository) (command: SubscribeToFeedDto) =
    let getFromUrl url =
        feedReader.getFromUrl url
        |> Async.map (Result.mapError SubscribeToFeedError.FeedError)
        |> Async.StartAsTask

    let createSubscription (command: SubscribeToFeedDto) _ = result {
        let! title = 
            Domain.SubscriptionTitle.create command.Title 
            |> Result.mapError (fun e -> SubscriptionError [e])

        return Domain.Subscription.create command.Url title
    }

    let saveSubscription = repository.save
    
    command.Url
    |> getFromUrl
    |> TaskResult.bind (createSubscription command)
    |> TaskResult.tee saveSubscription
    |> Task.map convertToWorkflowError2
    
let updateFeedIcon (feedReader: FeedReaderAdapter) (repository: SubscriptionRepository) (fileRepository: FileRepository) (subscriptionId: Domain.SubscriptionId) =
    let getFeedForSubscription (subscription: Domain.Subscription) =
        feedReader.getFromUrl subscription.Url
        |> Async.StartAsTask
        |> TaskResult.map (fun f -> subscription, f)
    
    let updateIcon (subscription: Domain.Subscription, feed: Feed) =
        let deleteIcon (subscription: Domain.Subscription) =
            subscription.Icon
            |> DtoValidation.Option.tap (fun f -> fileRepository.delete f)
            |> ignore
            
            { subscription with Icon = None }
           
        let saveFile file = fileRepository.save file 

        
        let createFileAndSaveFile (s, icon) =
            let inner (fn, data): Domain.File =
                {
                    Id = System.Guid.NewGuid()
                    FileName = fn
                    FileOwner = Domain.FileOwner.Feed
                    Data = data
                }
            
            let icon = 
                icon
                |> Option.map inner
                |> DtoValidation.Option.tap saveFile

            s, icon
        
        let changeIcon (subscription: Domain.Subscription, file: Domain.File option) =
            { subscription with Icon = file |> Option.map (fun file -> file.Id) }
         
        subscription
        |> deleteIcon
        |> fun s -> s,feed.Icon
        |> createFileAndSaveFile
        |> changeIcon
        
    subscriptionId
    |> repository.get
    |> (fun s -> getFeedForSubscription s)
    |> TaskResult.map updateIcon
    |> TaskResult.tee repository.save