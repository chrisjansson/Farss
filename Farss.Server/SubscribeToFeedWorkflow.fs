module SubscribeToFeedWorkflow
open Domain
open Persistence
open FeedReaderAdapter

type SubscribeToFeedCommand = 
    {
        Url: string
    }
    
let subscribeToFeed (feedReader: FeedReaderAdapter) (repository: FeedRepository) (command: SubscribeToFeedCommand) =
    let saveFeed _ =
        let feed: Feed = { Url = command.Url }
        repository.save feed

    feedReader.getFromUrl command.Url
    |> AsyncResult.mapResult saveFeed

