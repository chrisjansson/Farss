module Persistence
open Domain

type SubscriptionRepository =
    {
        getAll: unit -> Subscription list
        save: Subscription -> unit
        delete: SubscriptionId -> unit
    }

type ArticleRepository =
    {
        getAll: unit -> Article list
        save: Article -> unit
    }

let create () =
    let mutable feeds = []
    let getAll () = feeds
    let save (feed: Subscription) = feeds <- feed :: feeds
    let delete (id: SubscriptionId) = 
        let feeds' = List.filter (fun (f: Subscription) -> f.Id <> id) feeds
        feeds <- feeds'

    {
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
        {
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
        let save (article: Article) =
            documentSession.Store(article)
            documentSession.SaveChanges()
        {
            getAll = getAll
            save = save
        }

    let createInMemory () =
        let mutable articles = []
        let getAll () = articles
        let save (article: Article) = articles <- article :: articles

        {
            getAll = getAll
            save = save
        }