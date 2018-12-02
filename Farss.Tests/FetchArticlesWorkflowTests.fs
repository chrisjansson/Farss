﻿module FetchArticlesWorkflowTests

open Domain
open Expecto
open Persistence
open FakeFeedReaderAdapter
open System
open FeedReaderAdapter
open System.Threading.Tasks

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
let fetchArticlesForSubscriptionTests = 
    testList "Fetch articles for subscription workflow" [
        let tests = [
            "Fails when subscription is not found", fun subs (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let workflow = FetchEntriesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                
                let op () = workflow  (Guid.NewGuid()) |> Async.AwaitTask |> Async.Ignore
                
                do! Expect.throwsAsyncT op "Fails with exception"
            }
            "Does nothing when no items exists in feed", fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let subId = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = subId })
                adapterStub.SetResult ("feed url", Ok emptyFeed)

                let workflow = FetchEntriesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                let! result = workflow subId |> Async.AwaitTask
                
                Expect.equal result (Ok 0) "Number of fetched articles"
                Expect.isEmpty (articles.getAll()) "Articles"
            }
            "Returns failure when feed fails", fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let id = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = id })
                let expectedError = FeedError.ParseError (exn("error"))
                adapterStub.SetResult ("feed url", Error expectedError)

                let workflow = FetchEntriesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                let! result = workflow id |> Async.AwaitTask
                
                Expect.equal result (Error expectedError) "Expected error"
                Expect.isEmpty (articles.getAll()) "Articles"
            }
            "Feed with one new article adds article",  fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let subscriptionId = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = subscriptionId })
                let feedResult = { emptyFeed with Items = [ { FeedReaderAdapter.Item.Title = "Item title"; Id = "" } ] }
                adapterStub.SetResult ("feed url", Ok feedResult)

                let workflow = FetchEntriesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                let! result = workflow subscriptionId |> Async.AwaitTask
                
                let project (article: Article): ExpectedArticle =
                    {
                        Title = article.Title
                    }

                Expect.equal result (Ok 1) "One fetched article"
                Expect.equal (articles.getAll() |> List.map project) [ { Title = "Item title" } ] "Articles"
            }
            "Feed with one existing article is idempotent",  fun (subs: SubscriptionRepository) (articles: ArticleRepository) (adapterStub: FeedReaderAdapterStub) -> async {
                let subscriptionId = Guid.NewGuid()
                subs.save ({ Url = "feed url"; Id = subscriptionId })
                let feedResult = { emptyFeed with Items = [ { FeedReaderAdapter.Item.Title = "Item title"; Id = "a guid" } ] }
                adapterStub.SetResult ("feed url", Ok feedResult)

                let workflow = FetchEntriesWorkflow.fetchArticlesForSubscriptionImpl subs articles adapterStub.Adapter
                do! workflow subscriptionId |> Async.AwaitTask |> Async.Ignore
                let! result = workflow subscriptionId |> Async.AwaitTask
                
                let project (article: Article): ExpectedArticle =
                    {
                        Title = article.Title
                    }

                Expect.equal result (Ok 0) "No fetched articles"
                Expect.equal (articles.getAll() |> List.map project) [ { Title = "Item title" } ] "Articles"
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
                let expectedFetchError = (FetchError (exn "fetch error exception"))
                let expectedException = exn "throwing fetch"

                s.save ({ Domain.Subscription.Id = s0; Url = "0" })
                s.save ({ Domain.Subscription.Id = s1; Url = "0" })
                s.save ({ Domain.Subscription.Id = s2; Url = "0" })

                let stubFetch: FetchEntriesWorkflow.FetchArticlesForSubscription =
                    fun subId -> 
                        let r = 
                            match subId with
                            | x when x = s0 -> Ok 4711
                            | x when x = s1 -> Error expectedFetchError
                            | _ -> raise expectedException
                        Task.FromResult(r)

                let workflow = FetchEntriesWorkflow.fetchEntries s stubFetch

                let! result = workflow () |> Async.AwaitTask
                
                let expected = [ 
                        (s0, Ok 4711)
                        (s1, Error (FetchEntriesWorkflow.InnerError expectedFetchError))
                        (s2, Error (FetchEntriesWorkflow.OperationError expectedException))
                    ]

                Expect.equal result expected "Expected fetch results"
            }
        ]
        yield! testFixtureAsync setup tests
    ]