﻿module FetchArticlesWorkflowTests

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

                Expect.isOk result "Fetch result"
                //todo: ok of number of articles updated
                Expect.equal (articles.getAll() |> List.map project) [ { Title = "Item title" } ] "Articles"
            }
        ]
        yield! testFixtureAsync setup tests
    ]

[<Tests>]
let fetchArticlesForAllSubscriptionsTests = 
    testList "Fetch articles for all subscriptions workflow" [
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

        //multiple feeds multiple articles, read and not read
        //failing reads?
    ]