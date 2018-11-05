module SubscribeToFeedWorkflow
open Domain
open Persistence
open FeedReaderAdapter

type SubscribeToFeedCommand = 
    {
        Url: string
    }
    
let subscribeToFeed (feedReader: FeedReaderAdapter) (repository: FeedRepository) (command: SubscribeToFeedCommand) =
    feedReader.getFromUrl command.Url
    // let feed: Feed = { Url = command.Url }
    // repository.save feed

