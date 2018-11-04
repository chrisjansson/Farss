module SubscribeToFeedWorkflow
open Domain
open Persistence

type SubscribeToFeedCommand = 
    {
        Url: string
    }
    
let subscribeToFeed (repository: FeedRepository) (command: SubscribeToFeedCommand) =
    let feed: Feed = { Url = command.Url }
    repository.save feed

