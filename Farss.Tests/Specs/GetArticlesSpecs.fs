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
    }

let subscription (subscriptionUrl: string): AsyncTestStep<unit, Subscription> =
    Spec.Step.map (fun (_, f: TestWebApplicationFactory) -> 
            let subscription = { Subscription.Id = Guid.NewGuid(); Url = subscriptionUrl }
        
            f.InScope(fun (sr: SubscriptionRepository) -> sr.save(subscription), f) |> ignore

            subscription
        )

let with_articles (articles: SpecArticle list): AsyncTestStep<Subscription, unit> =
    Spec.Step.map (fun (_, f: TestWebApplicationFactory) -> 
            let toArticle (specArticle: SpecArticle) = { Build.article () with Title = specArticle.Title }

            let articles = articles |> List.map toArticle
        
            f.InScope(fun (ar: ArticleRepository) -> List.iter ar.save articles, f) |> ignore

            ()
        )

let the_reader_looks_at_all_articles: AsyncTestStep<unit, SpecArticle list> =
    Spec.Step.mapAsync (fun (_, f: TestWebApplicationFactory) -> async {
        use c = f.CreateClient()

        let! result = HttpClient.getAsJsonAsync<Dto.ArticleDto list> "/articles" c
        
        let toActualArticle (article: Dto.ArticleDto): SpecArticle =
            {
                Title = article.Title
            }

        return 
            result 
            |> List.map toActualArticle
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
        let command = { Dto.SetArticleReadStatusDto.ArticleId = Nullable(dArticle.Id) }

        let! response = HttpClient.postAsync "/article/setreadstatus" command client
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
    }

[<Tests>]
let tests = 
    specs "Reading articles" [
        spec "Read all articles" <| fun _ ->
            Given >>> subscription "sub" >>> with_articles [ article "First article"; article "Second article" ] >>>
            And >>> subscription "sub 2" >>> with_articles [ article "Third article" ] >>>
            When >>> the_reader_looks_at_all_articles >>>
            Then >>> the_reader_is_shown [ 
                article "First article"
                article "Second article"          
                article "Third article"
            ]
        spec "Mark article as unread" <| fun _ ->
            Given >>> subscription "sub" >>> with_articles [ article "First article"; article "Second article" ] >>>
            When >>> the_reader_marks "Second article" >>> as_read >>> 
            Then >>> article2 "First article" >>> is_unread >>>
            And >>> article2 "Second article" >>> is_read
        //todo: toggle unread
        //todo: query show there are queries
    ]