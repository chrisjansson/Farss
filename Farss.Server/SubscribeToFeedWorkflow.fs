module SubscribeToFeedWorkflow

open Persistence
open FeedReaderAdapter
open Dto

let private convertToWorkflowError r: Result<_, WorkflowError> =
    match r with
    | Ok r -> Ok r
    | Error (FetchError e) -> BadRequest (e.Message, Some e) |> Error
    | Error (ParseError e) -> BadRequest (e.Message, Some e) |> Error

let previewSubscribeToFeed (feedReader: FeedReaderAdapter) (query: PreviewSubscribeToFeedQueryDto) =   
    let toResponse (feed: Feed): PreviewSubscribeToFeedResponseDto =
        {
            Title = feed.Title
        }
    
    feedReader.getFromUrl query.Url
    |> Async.StartAsTask
    |> TaskResult.map toResponse
    |> Task.map convertToWorkflowError

type SubscribeToFeedError =
    | FeedError of FeedError
    | SubscriptionError of string list

let private convertToWorkflowError2 r: Result<_, WorkflowError> =
    match r with
    | Ok r -> Ok r
    | Error (FeedError (FetchError e)) -> BadRequest (e.Message, Some e) |> Error
    | Error (FeedError (ParseError e)) -> BadRequest (e.Message, Some e) |> Error
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