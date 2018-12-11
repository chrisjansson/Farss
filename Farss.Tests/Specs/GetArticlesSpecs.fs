module GetArticlesSpecs

open Expecto
open Spec
open Persistence
open Domain
open System
open TestStartup

open FeedBuilder
open SubscribeToFeedSpecs

type SpecArticle = 
    {
        Title: string
    }

module Build =
    open Domain
    let Article: Article = { 
        Article.Id = Guid.NewGuid()
        Title = "A title"
        Guid = Guid.NewGuid().ToString()
        Subscription = Guid.NewGuid()
        Content = "Content"
    }

let subscription (subscriptionUrl: string): AsyncTestStep<unit, Subscription> =
    Spec.Step.map (fun (_, f: TestWebApplicationFactory) -> 
            let subscription = { Subscription.Id = Guid.NewGuid(); Url = subscriptionUrl }
        
            f.InScope(fun (sr: SubscriptionRepository) -> sr.save(subscription), f) |> ignore

            subscription
        )

let with_articles (articles: SpecArticle list): AsyncTestStep<Subscription, unit> =
    Spec.Step.map (fun (_, f: TestWebApplicationFactory) -> 
            let toArticle (specArticle: SpecArticle) = { Build.Article with Title = specArticle.Title }

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
    ]