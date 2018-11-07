module SubscribeToFeedWorkflow
open Domain
open Persistence
open FeedReaderAdapter

type WorkflowError =
    | BadRequest of string * System.Exception

type SubscribeToFeedCommand = 
    {
        Url: string
    }


let subscribeToFeed (feedReader: FeedReaderAdapter) (repository: FeedRepository) (command: SubscribeToFeedCommand) =
    //todo: handle atom feeds betters
    //todo: rss2.0 only? Does that change anything?
    let saveFeed _ =
        let feed: Feed = { Url = command.Url }
        repository.save feed

    let convertToWorkflowError r: Result<Unit, WorkflowError> =
        match r with
        | Ok r -> Ok r
        | Error (FetchError e) -> BadRequest (e.Message, e) |> Error
        | Error (ParseError e) -> BadRequest (e.Message, e) |> Error

    feedReader.getFromUrl command.Url
    |> AsyncResult.mapResult saveFeed
    |> Async.map convertToWorkflowError


