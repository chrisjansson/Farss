module Persistence
open Domain

type SubscriptionRepository =
    {
        get: SubscriptionId -> Subscription
        getAll: unit -> Subscription list
        save: Subscription -> unit
        delete: SubscriptionId -> unit
    }

type ArticleRepository =
    {
        getAll: unit -> Article list
        save: Article -> unit
        filterExistingArticles: string list -> string list
    }

let create () =
    let mutable feeds = []
    let getAll () = feeds
    let save (feed: Subscription) = feeds <- feed :: feeds
    let delete (id: SubscriptionId) = 
        let feeds' = List.filter (fun (f: Subscription) -> f.Id <> id) feeds
        feeds <- feeds'
    let get (id: SubscriptionId) = List.find (fun (f: Subscription) -> f.Id = id) feeds
    
    {
        get = get
        getAll = getAll
        save = save
        delete = delete
    }

type WorkflowError =
    | BadRequest of string * System.Exception
    | InvalidParameter of string list

module Query = 
    open System.Linq.Expressions
    open System.Linq
    open System

    let where (predicate: Expression<Func<_, bool>>) (query: IQueryable<_>) =
        query.Where(predicate)

    let single (query: IQueryable<_>) =
        query.Single()

    let toList (query: IQueryable<_>) =
        query.ToList()

    let singleP (predicate: Expression<Func<_, bool>>) (query: IQueryable<_>) =
        query.Single(predicate)

    type Expr = 
        static member Quote(e:Expression<System.Func<_, _>>) = e

module SubscriptionRepositoryImpl =
    open Marten
    
    let create (documentSession: IDocumentSession) =
        let getAll () = 
            documentSession.Query<Subscription>()
            |> Query.toList
            |> List.ofSeq
        let save (subscription: Subscription) =
            documentSession.Store(subscription)
            documentSession.SaveChanges()
        let delete (subscriptionId: SubscriptionId) =
            documentSession.Delete<Subscription>(subscriptionId)
            documentSession.SaveChanges()
        let get (subscriptionId: SubscriptionId) =  
            documentSession.Query<Subscription>()
                |> Query.singleP (Query.Expr.Quote(fun s -> s.Id = subscriptionId))
        {
            get = get
            getAll = getAll
            save = save
            delete = delete
        }


module ArticleRepositoryImpl =
    open Marten
    open System.Linq
    
    [<CLIMutable>]
    type FilterProjection = 
        {
            ExistingGuid: string
            Exists: bool
        }

    let create (documentSession: IDocumentSession): ArticleRepository =
        let getAll () = 
            documentSession.Query<Article>()
            |> Query.toList
            |> List.ofSeq
        let save (article: Article) =
            documentSession.Store(article)
            documentSession.SaveChanges()

        let filterExistingArticles guids =
            let guidsA = Array.ofList guids
            let query = 
                (documentSession.Query<Article>() :> IQueryable<Article>)
                    .Select(Query.Expr.Quote<Article, FilterProjection>(fun a -> { ExistingGuid = a.Guid; Exists = guidsA.Contains(a.Guid) }))
                    .Where(Query.Expr.Quote(fun proj -> proj.Exists))
                    .Select(fun p -> p.ExistingGuid)
            let existingArticles = Query.toList query |> List.ofSeq
            List.except existingArticles guids
        {
            getAll = getAll
            save = save
            filterExistingArticles = filterExistingArticles
        }

    let createInMemory () =
        let mutable articles = []
        let getAll () = articles
        let save (article: Article) = articles <- article :: articles
        
        let filterExistingArticles guids =
            let existingGuids = List.map (fun a -> a.Guid) articles
            List.except existingGuids guids

        {
            getAll = getAll
            save = save
            filterExistingArticles = filterExistingArticles
        }