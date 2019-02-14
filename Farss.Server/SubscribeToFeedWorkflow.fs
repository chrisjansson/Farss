module SubscribeToFeedWorkflow

open Persistence
open FeedReaderAdapter

type PreviewSubscribeToFeedQuery =
    {
        Url: string
    }

type PreviewSubscribeToFeedResponse =
    {
        Title: string
    }

let private convertToWorkflowError r: Result<_, WorkflowError> =
    match r with
    | Ok r -> Ok r
    | Error (FetchError e) -> BadRequest (e.Message, e) |> Error
    | Error (ParseError e) -> BadRequest (e.Message, e) |> Error


let previewSubscribeToFeed (feedReader: FeedReaderAdapter) (query: PreviewSubscribeToFeedQuery) =   
    let toResponse (feed: Feed) =
        {
            Title = feed.Title
        }
    
    feedReader.getFromUrl query.Url
    |> AsyncResult.mapResult toResponse
    |> Async.map convertToWorkflowError

type SubscribeToFeedCommand = 
    {
        Url: string
        Title: string
    }

 type SubscribeToFeedError =
    | FeedError of FeedError
    | SubscriptionError of string list

let private convertToWorkflowError2 r: Result<_, WorkflowError> =
    match r with
    | Ok r -> Ok r
    | Error (FeedError (FetchError e)) -> BadRequest (e.Message, e) |> Error
    | Error (FeedError (ParseError e)) -> BadRequest (e.Message, e) |> Error
    | Error (SubscriptionError errors) -> BadRequest (sprintf "%A" errors, exn "wut") |> Error

let subscribeToFeed (feedReader: FeedReaderAdapter) (repository: SubscriptionRepository) (command: SubscribeToFeedCommand) =
    let getFromUrl url =
        feedReader.getFromUrl url
        |> Async.map (Result.mapError SubscribeToFeedError.FeedError)

    let createSubscription command _ = result {
        let! title = 
            Domain.SubscriptionTitle.create command.Title 
            |> Result.mapError (fun e -> SubscriptionError [e])

        return Domain.Subscription.create command.Url title
    }

    let saveSubscription = repository.save

    let tee onOk rA = async {
        let! r = rA
        match r with
        | Ok v -> 
            onOk v
        | Error _ -> 
            ()
        return r
    }
    
    command.Url
    |> getFromUrl
    |> AsyncResult.bind (createSubscription command)
    |> tee saveSubscription
    |> Async.map convertToWorkflowError2