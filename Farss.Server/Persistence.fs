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
        filterExistingArticles: SubscriptionId -> string list -> string list
        getAllBySubscription: SubscriptionId -> Article list
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

    let create (documentSession: IDocumentSession): ArticleRepository =
        let getAll () = 
            documentSession.Query<Article>()
            |> Query.toList
            |> List.ofSeq
        let getAllBySubscription (subscriptionId: SubscriptionId) =
            documentSession.Query<Article>()
            |> Query.where (Query.Expr.Quote(fun a -> a.Subscription = subscriptionId))
            |> Query.toList
            |> List.ofSeq

        let save (article: Article) =
            documentSession.Store(article)
            documentSession.SaveChanges()

        let filterExistingArticles (subscriptionId: SubscriptionId) (guids: string list) =
            if List.length guids = 0 then
                []
            else if List.contains null guids then
                failwith "Feed GUID is null"
            else
                let guidsA = Array.ofList guids
                let query = """SELECT unnest(?) as guid
                except
                SELECT data ->> 'Guid' from mt_doc_domain_article WHERE data ->> 'Subscription' = ?"""
                let query = documentSession.Query<string>(query, guidsA, subscriptionId.ToString())
                List.ofSeq query
        {
            getAll = getAll
            getAllBySubscription = getAllBySubscription
            save = save
            filterExistingArticles = filterExistingArticles
        }

    let createInMemory () =
        let mutable articles = []
        let getAll () = articles
        let getAllBySubscription subscriptionId = 
            articles
            |> List.filter (fun a -> a.Subscription = subscriptionId)
        
        let save (article: Article) = articles <- article :: articles
        
        let filterExistingArticles subscriptionId guids =
            let existingGuids = articles |> List.filter (fun (a: Article) -> a.Subscription = subscriptionId) |> List.map (fun (a:Article) -> a.Guid)
            List.except existingGuids guids
        {
            getAll = getAll
            getAllBySubscription = getAllBySubscription
            save = save
            filterExistingArticles = filterExistingArticles
        }