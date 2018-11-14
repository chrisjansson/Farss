module Persistence
open Domain

type FeedRepository =
    {
        getAll: unit -> Feed list
        save: Feed -> unit
        delete: FeedId -> unit
    }

let create () =
    let mutable feeds = []
    let getAll () = feeds
    let save (feed: Feed) = feeds <- feed :: feeds
    let delete (id: FeedId) = 
        let feeds' = List.filter (fun (f: Feed) -> f.Id <> id) feeds
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



module FeedRepositoryImpl =
    open Marten
    
    let create (documentSession: IDocumentSession) =
        let getAll () = 
            documentSession.Query<Feed>()
            |> Query.toList
            |> List.ofSeq
        let save (subscription: Feed) =
            documentSession.Store(subscription)
            documentSession.SaveChanges()
        let delete (subscriptionId: FeedId) =
            documentSession.Delete(subscriptionId)
        {
            getAll = getAll
            save = save
            delete = delete
        }