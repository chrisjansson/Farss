module FetchArticlesWorkflowTests

open Domain
open Expecto
open Persistence
open FakeFeedReaderAdapter
open System
open FeedReaderAdapter
open System.Threading.Tasks
open FetchArticlesWorkflow

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
        Content: string
        Timestamp: Domain.ArticleTimestamp
    }

[<Tests>]
let fetchArticlesForSubscriptionTests = 
    let assertYieldsItemError title feedItem =
        title,  fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let subscriptionId = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = subscriptionId })
                let feedItems = [ 
                    feedItem
                ]
                let feedResult = { emptyFeed with Items = feedItems }
                adapterStub.SetResult ("feed url", Ok feedResult)

                let workflow = FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                let! result = workflow subscriptionId |> Async.AwaitTask
                
                let isItemError result =
                    match result with
                    | Error (ItemError _) -> true
                    |_ -> false

                Expect.isTrue (isItemError result) "Contains item error"
                Expect.equal (articles.getAll().Length) 0 "Articles"
        }

    testList "Fetch articles for subscription workflow" [
        let tests = [
            "Fails when subscription is not found", fun subs (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let workflow = FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                
                let op () = workflow  (Guid.NewGuid()) |> Async.AwaitTask |> Async.Ignore
                
                do! Expect.throwsAsyncT op "Fails with exception"
            }
            "Does nothing when no items exists in feed", fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let subId = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = subId })
                adapterStub.SetResult ("feed url", Ok emptyFeed)

                let workflow = FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                let! result = workflow subId |> Async.AwaitTask
                
                Expect.equal result (Ok 0) "Number of fetched articles"
                Expect.isEmpty (articles.getAll()) "Articles"
            }
            "Returns failure when feed fails", fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let id = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = id })
                let expectedError = (FeedError.ParseError (exn("error")))
                adapterStub.SetResult ("feed url", Error expectedError)

                let workflow = FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                let! result = workflow id |> Async.AwaitTask
                
                Expect.equal result (Error (FeedError expectedError)) "Expected error"
                Expect.isEmpty (articles.getAll()) "Articles"
            }
            "Does not fetch any articles when one fails",  fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let subscriptionId = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = subscriptionId })
                let feedItems = [ 
                    { FeedReaderAdapter.Item.Title = "Item title"; Id = "Article Guid"; Content = "Item content"; Timestamp = (Some DateTimeOffset.Now) } 
                    { FeedReaderAdapter.Item.Title = "Item title"; Id = "Article Guid"; Content = "Item content"; Timestamp = None } 
                ]
                let feedResult = { emptyFeed with Items = feedItems }
                adapterStub.SetResult ("feed url", Ok feedResult)

                let workflow = FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                let! result = workflow subscriptionId |> Async.AwaitTask
                
                let isItemError result =
                    match result with
                    | Error (ItemError _) -> true
                    |_ -> false

                Expect.isTrue (isItemError result) "Contains item error"
                Expect.equal (articles.getAll().Length) 0 "Articles"
            }
            
            assertYieldsItemError "Timestamp is required" { Title = "Item title"; Id = "Article Guid"; Content = "Content"; Timestamp = None }
            
            assertYieldsItemError "Item id is required" { Title = "Item title"; Id = null; Content = "Content"; Timestamp = Some (DateTimeOffset.Now) }
            
            "Feed with one new article adds article",  fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let subscriptionId = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = subscriptionId })
                let items = [ 
                    { 
                        FeedReaderAdapter.Item.Title = "Item title"
                        Id = "Article Guid"
                        Content = "Item content"
                        Timestamp = Some (DateTimeOffset(2002, 2, 3, 4, 5, 7, TimeSpan.Zero)) } 
                ]
                let feedResult = { emptyFeed with Items = items }
                adapterStub.SetResult ("feed url", Ok feedResult)

                let workflow = FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                let! result = workflow subscriptionId |> Async.AwaitTask
                
                let project (article: Article): ExpectedArticle =
                    {
                        Title = article.Title
                        Content = article.Content
                        Timestamp = article.Timestamp

                    }

                Expect.equal result (Ok 1) "One fetched article"
                Expect.equal (articles.getAll() |> List.map project) [ { Title = "Item title"; Content = "Item content"; Timestamp = DateTimeOffset(2002, 2, 3, 4, 5, 7, TimeSpan.Zero) } ] "Articles"
            }
            "Fetch one article associates with subscription",  fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let subscriptionId = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = subscriptionId })
                let feedResult = { emptyFeed with Items = [ { FeedReaderAdapter.Item.Title = "Item title"; Id = "a guid"; Content = ""; Timestamp = Some (DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero)) } ] }
                adapterStub.SetResult ("feed url", Ok feedResult)

                let workflow = FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                do! workflow subscriptionId |> Async.AwaitTask |> Async.Ignore
                
                let project (article: Article): ExpectedArticle =
                    {
                        Title = article.Title
                        Content = article.Content
                        Timestamp = article.Timestamp
                    }

                Expect.equal (articles.getAllBySubscription subscriptionId |> List.map project) [ { Title = "Item title"; Content = ""; Timestamp = DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero) } ] "Articles"
                Expect.equal (articles.getAllBySubscription (Guid.NewGuid())) [] "No articles exist for other subscription"
            }
            "Feed with one existing article is idempotent",  fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let subscriptionId = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = subscriptionId })
                let feedResult = { emptyFeed with Items = [ { FeedReaderAdapter.Item.Title = "Item title"; Id = "a guid"; Content = ""; Timestamp = Some (DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero)) } ] }
                adapterStub.SetResult ("feed url", Ok feedResult)

                let workflow = FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                do! workflow subscriptionId |> Async.AwaitTask |> Async.Ignore
                let! result = workflow subscriptionId |> Async.AwaitTask
                
                let project (article: Article): ExpectedArticle =
                    {
                        Title = article.Title
                        Content = article.Content
                        Timestamp = article.Timestamp
                    }

                Expect.equal result (Ok 0) "No fetched articles"
                Expect.equal (articles.getAll() |> List.map project) [ { Title = "Item title"; Content = ""; Timestamp = DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero) } ] "Articles"
            }
            "Fetches are grouped by subscription",  fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let subscriptionId0 = Guid.NewGuid()
                let subscriptionId1 = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = subscriptionId0 })
                subs.save ({ Url = "feed url"; Id = subscriptionId1 })
                let feedResult = { emptyFeed with Items = [ { FeedReaderAdapter.Item.Title = "Item title"; Id = "a guid"; Content = ""; Timestamp = (Some (DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero))) } ] }
                adapterStub.SetResult ("feed url", Ok feedResult)

                let workflow = FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                do! workflow subscriptionId0 |> Async.AwaitTask |> Async.Ignore
                do! workflow subscriptionId0 |> Async.AwaitTask |> Async.Ignore
                do! workflow subscriptionId1 |> Async.AwaitTask |> Async.Ignore
                do! workflow subscriptionId1 |> Async.AwaitTask |> Async.Ignore
                
                let project (article: Article): ExpectedArticle =
                    {
                        Title = article.Title
                        Content = article.Content
                        Timestamp = article.Timestamp
                    }
                
                Expect.equal (articles.getAllBySubscription subscriptionId0 |> List.map project) [ { Title = "Item title"; Content = ""; Timestamp = DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero) } ] "Article fetched for first sub"
                Expect.equal (articles.getAllBySubscription subscriptionId1 |> List.map project) [ { Title = "Item title"; Content = ""; Timestamp = DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero) } ] "Artile fetched for second sub"
                Expect.equal (articles.getAll ()).Length 2 "One article per subscription"
            }
        ]
        yield! testFixtureAsync setup tests
    ]

[<Tests>]
let fetchArticlesForAllSubscriptionsTests = 
    testList "Fetch articles for all subscriptions workflow" [
        let tests = [
            "Fetches articles in isolation from eachother", fun (s: SubscriptionRepository) _ _ -> async {
                let s0 = Guid.NewGuid()
                let s1 = Guid.NewGuid()
                let s2 = Guid.NewGuid()
                let expectedFetchError = (FeedError (FetchError (exn "fetch error exception")))
                let expectedException = exn "throwing fetch"

                s.save ({ Subscription.Id = s0; Url = "0" })
                s.save ({ Subscription.Id = s1; Url = "0" })
                s.save ({ Subscription.Id = s2; Url = "0" })

                let stubFetch: FetchArticlesWorkflow.FetchArticlesForSubscription =
                    fun subId -> 
                        let r = 
                            match subId with
                            | x when x = s0 -> Ok 4711
                            | x when x = s1 -> Error expectedFetchError
                            | _ -> raise expectedException
                        Task.FromResult(r)

                let workflow = FetchArticlesWorkflow.fetchEntries s stubFetch

                let! result = workflow () |> Async.AwaitTask
                
                let expected = [ 
                        (s0, Ok 4711)
                        (s1, Error (InnerError expectedFetchError))
                        (s2, Error (OperationError expectedException))
                    ]

                Expect.equal result expected "Expected fetch results"
            }
        ]
        yield! testFixtureAsync setup tests
    ]