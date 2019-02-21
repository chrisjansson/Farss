module GetArticlesSpecs

open Expecto
open Spec
open Persistence
open Domain
open System
open TestStartup

open SubscribeToFeedSpecs

type SpecArticle = 
    {
        Title: string
        IsRead: bool
        PublishedAt: DateTimeOffset
    }

let subscription (subscriptionUrl: string): AsyncTestStep<unit, Subscription> =
    Spec.Step.map (fun (_, f: TestWebApplicationFactory) -> 
            let subscription = { Subscription.Id = Guid.NewGuid(); Url = subscriptionUrl; Title = "title" }
        
            f.InScope(fun (sr: SubscriptionRepository) -> sr.save(subscription), f) |> ignore

            subscription
        )

let with_articles (articles: SpecArticle list): AsyncTestStep<Subscription, unit> =
    Spec.Step.map (fun (s, f: TestWebApplicationFactory) -> 
            let toArticle (specArticle: SpecArticle) = 
                { Build.article () with Title = specArticle.Title; Timestamp = specArticle.PublishedAt; IsRead = specArticle.IsRead; Subscription = s.Id }

            let articles = articles |> List.map toArticle
        
            f.InScope(fun (ar: ArticleRepository) -> List.iter ar.save articles, f) |> ignore

            ()
        )

let the_reader_looks_at_all_articles: AsyncTestStep<unit, SpecArticle list> =
    Spec.Step.mapAsync (fun (_, f: TestWebApplicationFactory) -> async {
        use c = f.CreateClient()

        let! result = HttpClient.getAsJsonAsync<Dto.ArticleDto list> ApiUrls.GetArticles c
        
        let toActualArticle (article: Dto.ArticleDto): SpecArticle =
            {
                Title = article.Title
                IsRead = article.IsRead
                PublishedAt = article.PublishedAt
            }

        return 
            result 
            |> List.map toActualArticle
            |> List.sortBy (fun a -> a.Title)
    })

let the_reader_is_shown (expectedArticles: SpecArticle list): AsyncTestStep<SpecArticle list, unit> =
    Spec.Step.map (fun (actualArticles, _: TestWebApplicationFactory) -> 
        Expect.equal actualArticles expectedArticles "Shown articles"
    )

let the_reader_marks (article: string): AsyncTestStep<unit, string> =
    Spec.Step.map (fun (_, _: TestWebApplicationFactory) -> 
        article
    )

let getArticleByTitle articleTitle (f: TestWebApplicationFactory) =
    f.InScope(fun (ar: ArticleRepository) -> ar.getAll() |> List.find (fun a -> a.Title = articleTitle))


let as_read: AsyncTestStep<string, unit> =
    Spec.Step.mapAsync (fun (article, f: TestWebApplicationFactory) -> async {
        let dArticle = getArticleByTitle article f
        
        let client = f.CreateClient()
        let command = { Dto.SetArticleReadStatusDto.ArticleId = Some (dArticle.Id); Dto.SetArticleReadStatusDto.SetIsReadTo = Some (true) }

        let! response = HttpClient.postAsync ApiUrls.SetArticleReadStatus command client
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        //TODO: Handle this in HttpClient instead
        System.IO.File.WriteAllText("C:\\temp\output.html", content)
        response.EnsureSuccessStatusCode() |> ignore
    })

let article2 (article: string): AsyncTestStep<unit, string> =
    Spec.Step.map (fun (_, _: TestWebApplicationFactory) -> 
        article
    )

let is_unread: AsyncTestStep<string, unit> =
    Spec.Step.map (fun (article, f: TestWebApplicationFactory) -> 
        let dArticle = getArticleByTitle article f
        Expect.isFalse dArticle.IsRead "Article should be unread"
    )

let is_read: AsyncTestStep<string, unit> =
    Spec.Step.map (fun (article, f: TestWebApplicationFactory) -> 
        let dArticle = getArticleByTitle article f
        Expect.isTrue dArticle.IsRead "Article should be read"
    )

let article (title: string): SpecArticle = 
    {
        Title = title
        IsRead = false
        PublishedAt = DateTimeOffset.MinValue
    }

let as_read2 (article: SpecArticle) =
    { article with IsRead = true }
    
let as_unread2 (article: SpecArticle) =
    { article with IsRead = false }
    
module DateTimeOffset =
    let parseTest (s: string) =
        DateTimeOffset.Parse(s)

let published_at (publishingDate: string) (article: SpecArticle) =
    { article with PublishedAt = DateTimeOffset.parseTest publishingDate }

[<Tests>]
let tests = 
    specs "Reading articles" [
        spec "Read all articles" <| fun _ ->
            Given >> subscription "sub" >> with_articles [ article "First article"; article "Second article" ] >>
            And >> subscription "sub 2" >> with_articles [ article "Third article" ] >>
            When >> the_reader_looks_at_all_articles >>
            Then >> the_reader_is_shown [ 
                article "First article"
                article "Second article"          
                article "Third article"
            ]

        spec "Mark article as unread" <| fun _ ->
            Given >> subscription "sub" >> with_articles [ article "First article"; article "Second article" ] >>
            When >> the_reader_marks "Second article" >> as_read >> 
            Then >> article2 "First article" >> is_unread >>
            And >> article2 "Second article" >> is_read

        spec "API client read all articles unfiltered" <| fun _ ->
            Given >> subscription "sub" >> with_articles [ 
                article "First article" |> published_at "2000-01-02"
                article "Second article" |> published_at "2000-02-03"
            ] >>
            Given >> subscription "sub 2" >> with_articles [ 
                article "Third article" |> published_at "2000-03-02"
            ] >>
            When >> the_reader_marks "Second article" >> as_read >> 
            When >> the_reader_looks_at_all_articles >>
            Then >> the_reader_is_shown [ 
                article "First article" |> as_unread2 |> published_at "2000-01-02"
                article "Second article" |> as_read2 |> published_at "2000-02-03"
                article "Third article" |> as_unread2 |> published_at "2000-03-02"
            ]       
    ]