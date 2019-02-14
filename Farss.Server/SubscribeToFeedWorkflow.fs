module SubscribeToFeedWorkflow
open System
open Domain
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
    }

let subscribeToFeed (feedReader: FeedReaderAdapter) (repository: SubscriptionRepository) (command: SubscribeToFeedCommand) =
    //todo: handle atom feeds betters
    //todo: rss2.0 only? Does that change anything?
    let saveFeed _ =
        let feed: Subscription = { Url = command.Url; Id = Guid.NewGuid() }
        repository.save feed

    feedReader.getFromUrl command.Url
    |> AsyncResult.mapResult saveFeed
    |> Async.map convertToWorkflowError