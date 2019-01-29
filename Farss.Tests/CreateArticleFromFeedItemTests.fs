module CreateArticleFromFeedItemTests

open Expecto
open System
open FeedReaderAdapter

[<Tests>]
let tests = 
    testList "Create article from feed item" [
        let subscriptionId = Guid.NewGuid()
        let feedItemThatConvertsToArticle: FeedReaderAdapter.Item =
            {
                Title = "title"
                Id = "guid"
                Content = "content"
                Timestamp = Some (DateTimeOffset(2001, 2, 3, 4, 5, 6, TimeSpan.Zero))
                Link = "a link"
            }

        yield testCase "Item id is required" <| fun _ ->
            let item = { feedItemThatConvertsToArticle with Id = null }

            let result = FeedItem.toArticle subscriptionId item
            
            Expect.isError result "Item Id is required"

        yield testCase "Timestamp is required" <| fun _ ->
            let item = { feedItemThatConvertsToArticle with Timestamp = None }

            let result = FeedItem.toArticle subscriptionId item
            
            Expect.isError result "Item timestamp is required"
            
        yield testCase "Creates article when all requirements are met" <| fun _ ->
            let result = FeedItem.toArticle subscriptionId feedItemThatConvertsToArticle
            
            Expect.isOk result "Creates article from item successfully"
            
            let article: Domain.Article = result |> fun (Ok(article)) -> article

            Expect.equal article.Guid feedItemThatConvertsToArticle.Id "Guid"
            Expect.equal article.Content feedItemThatConvertsToArticle.Content "Content"
            Expect.equal article.Timestamp feedItemThatConvertsToArticle.Timestamp.Value "Timestamp"
            Expect.equal article.Title feedItemThatConvertsToArticle.Title "Title"
    ]