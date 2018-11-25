module FetchArticlesWorkflowTests

open Domain
open Expecto
open Persistence
open FakeFeedReaderAdapter
open System
open FeedReaderAdapter

let setup test = async {
    
        let subscriptionRepository = Persistence.create () 
        let articleRepository = Persistence.ArticleRepositoryImpl.createInMemory()
        let adapter = FakeFeedReaderAdapter.createStub ()

        do! test subscriptionRepository articleRepository adapter
    
    }

[<Tests>]
let tests = 
    testList "Fetch articles workflow" [
        let tests = [
            "Does nothing when no subscriptions exists", fun subs (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let workflow = FetchEntriesWorkflow.fetchEntries subs articles adapterStub.Adapter
                let! result = workflow () |> Async.AwaitTask
                
                Expect.isOk result "Workflow result"
                Expect.isEmpty (articles.getAll()) "Articles"
            }
            "Does nothing when no items exists in feed", fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                subs.save ({ Url = "feed url"; Id = Guid.NewGuid() })
                let emptyFeed = { FeedReaderAdapter.Feed.Title = ""; Description = ""; Items = [] }
                adapterStub.SetResult ("feed url", Ok emptyFeed)

                let workflow = FetchEntriesWorkflow.fetchEntries subs articles adapterStub.Adapter
                let! result = workflow () |> Async.AwaitTask
                
                Expect.isOk result "Workflow result"
                Expect.isEmpty (articles.getAll()) "Articles"
            }
            "Returns failure when feed fails", fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                subs.save ({ Url = "feed url"; Id = Guid.NewGuid() })
                let expectedError = FeedError.ParseError (exn("error"))
                adapterStub.SetResult ("feed url", Error expectedError)

                let workflow = FetchEntriesWorkflow.fetchEntries subs articles adapterStub.Adapter
                let! result = workflow () |> Async.AwaitTask
                
                Expect.equal result (Ok [ expectedError ]) "Expected error"
                Expect.isEmpty (articles.getAll()) "Articles"
            }
        ]
        yield! testFixtureAsync setup tests

        //no feeds, no articles does nothing
        //feed with no articles does nothing
        //feed with one new article adds one
        //feed with existing article does nothing
        //multiple feeds multiple articles, read and not read
        //failing reads?
    ]