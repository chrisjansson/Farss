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

let emptyFeed = { FeedReaderAdapter.Feed.Title = ""; Description = ""; Items = [] }

type ExpectedArticle = 
    {
        Title: string
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
            "Feed with one new article adds article",  fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                subs.save ({ Url = "feed url"; Id = Guid.NewGuid() })
                let feedResult = { emptyFeed with Items = [ { FeedReaderAdapter.Item.Title = "Item title"; Id = "" } ] }
                adapterStub.SetResult ("feed url", Ok feedResult)

                let workflow = FetchEntriesWorkflow.fetchEntries subs articles adapterStub.Adapter
                let! result = workflow () |> Async.AwaitTask
                
                let project (article: Article): ExpectedArticle =
                    {
                        Title = article.Title
                    }

                Expect.isOk result "Fetch result"
                Expect.equal (articles.getAll() |> List.map project) [ { Title = "Item title" } ] "Articles"
            }
            "Feed with one existing article does not add article",  fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                subs.save ({ Url = "feed url"; Id = Guid.NewGuid() })
                let feedResult = { emptyFeed with Items = [ { FeedReaderAdapter.Item.Title = "Item title"; Id = "a guid" } ] }
                adapterStub.SetResult ("feed url", Ok feedResult)

                let workflow = FetchEntriesWorkflow.fetchEntries subs articles adapterStub.Adapter
                do! workflow () |> Async.AwaitTask |> Async.Ignore
                let! result = workflow () |> Async.AwaitTask
                
                let project (article: Article): ExpectedArticle =
                    {
                        Title = article.Title
                    }

                Expect.isOk result "Fetch result"
                Expect.equal (articles.getAll() |> List.map project) [ { Title = "Item title" } ] "Articles"
            }
        ]
        yield! testFixtureAsync setup tests

        //feed with existing article does nothing
        //multiple feeds multiple articles, read and not read
        //failing reads?
    ]