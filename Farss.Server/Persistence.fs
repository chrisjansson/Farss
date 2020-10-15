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

//TODO: Move to test assembly
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

open System.Linq.Expressions
open System.Linq
open System

module Query = 


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

[<AllowNullLiteral>]
type PersistedSubscription() =
    member val Id: Guid = Unchecked.defaultof<_> with get, set
    member val Url: string = Unchecked.defaultof<_> with get, set
    member val Title: string = Unchecked.defaultof<_> with get, set

open Microsoft.EntityFrameworkCore

type ReaderContext(options) =
    inherit DbContext(options)
    
    member val Subscriptions: DbSet<PersistedSubscription> = null with get, set

module SubscriptionRepositoryImpl =
    let E = Query.Expr.Quote
    
    let private mapToSubscription (s: PersistedSubscription): Subscription =
        {
            Id = s.Id
            Url = s.Url
            Title = s.Title
        }
    
    let private mapFromSubscription (s: Subscription) (t: PersistedSubscription) =
        t.Id <- s.Id
        t.Title <- s.Title
        t.Url <- t.Url
    
    
    let create (context: ReaderContext) =
        let getAll () = 
            context.Subscriptions
            |> Query.toList
            |> Seq.map mapToSubscription
            |> List.ofSeq
        let save (subscription: Subscription) =
            context.Subscriptions.Find(subscription.Id)
            |> Option.ofObj
            |> Option.defaultWith (fun () ->
                let i = PersistedSubscription()
                context.Subscriptions.Add(i) |> ignore
                i)
            |> mapFromSubscription subscription
            context.SaveChanges() |> ignore
        let delete (subscriptionId: SubscriptionId) =
            let s = context.Subscriptions.Find(subscriptionId)
            context.Subscriptions.Remove(s) |> ignore
            context.SaveChanges() |> ignore
        let get (subscriptionId: SubscriptionId) =
            context.Subscriptions
                .Single(fun s -> s.Id = subscriptionId)
                |> mapToSubscription
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
            documentSession.Store<Article>(article)
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

    //TODO: move to test assembly
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