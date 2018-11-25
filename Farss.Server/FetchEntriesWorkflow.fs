module FetchEntriesWorkflow

open Domain
open Persistence
open FeedReaderAdapter
open FSharp.Control.Tasks.V2
open System


let fetchEntries 
    (subscriptionRepository: SubscriptionRepository) 
    (articleRepository: ArticleRepository) 
    (adapter: FeedReaderAdapter) 
    _ = task {
        let subscriptions = subscriptionRepository.getAll()
    
        for s in subscriptions do
            let! getFeedResult = adapter.getFromUrl(s.Url)
            match getFeedResult with
            | Ok feed ->
                for item in feed.Items do
                    let article = { Title = item.Title; Id = Guid.NewGuid() }
                    articleRepository.save(article)
            | Error e -> ()
    }
