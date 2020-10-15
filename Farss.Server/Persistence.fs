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

[<AllowNullLiteral>]
type PersistedArticle() =
    member val Id: Guid = Unchecked.defaultof<_> with get, set
    member val Title: string = Unchecked.defaultof<_> with get, set
    member val Guid: string = Unchecked.defaultof<_> with get, set
    member val SubscriptionId: Guid = Unchecked.defaultof<_> with get, set
    member val Subscription: PersistedSubscription = Unchecked.defaultof<_> with get, set
    member val Content: string = Unchecked.defaultof<_> with get, set
    member val IsRead: bool = Unchecked.defaultof<_> with get, set
    member val Timestamp: DateTimeOffset = Unchecked.defaultof<_> with get, set
    member val Link: string = Unchecked.defaultof<_> with get, set

open Microsoft.EntityFrameworkCore

type ReaderContext(options) =
    inherit DbContext(options)
    
    member val Subscriptions: DbSet<PersistedSubscription> = null with get, set
    member val Articles: DbSet<PersistedArticle> = null with get, set

    override x.OnModelCreating(mb) =
        mb.Entity<PersistedArticle>()
            .HasOne(fun x -> x.Subscription)
            .WithMany()
            |> ignore

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
    
    let getOrAddNew<'T when 'T : (new: unit -> 'T) and 'T : not struct and 'T : null> (id: Guid) (set: DbSet<_>) =
        set.Find id
        |> Option.ofObj
        |> Option.defaultWith (fun () ->
            let instance = new 'T()
            set.Add(instance) |> ignore
            instance)
        
    
    
    let create (context: ReaderContext) =
        let getAll () = 
            context.Subscriptions
            |> Query.toList
            |> Seq.map mapToSubscription
            |> List.ofSeq
        let save (subscription: Subscription) =
            getOrAddNew subscription.Id context.Subscriptions
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

    let private mapToArticle (s: PersistedArticle): Article =
        {
            Id = s.Id
            Title = s.Title
            Guid = s.Guid
            Subscription = s.SubscriptionId
            Content = s.Content
            IsRead = s.IsRead
            Timestamp = s.Timestamp
            Link = s.Link
        }
    
    let private mapFromArticle (s: Article) (t: PersistedArticle) =
        t.Id <- s.Id
        t.Title <- s.Title
        t.Guid <- s.Guid
        t.SubscriptionId <- s.Subscription
        t.Content <- s.Content
        t.IsRead <- s.IsRead
        t.Timestamp <- s.Timestamp
        t.Link <- s.Link
    
    let create (context: ReaderContext): ArticleRepository =
        let getAll () =
            context.Articles
            |> Query.toList
            |> Seq.map mapToArticle
            |> List.ofSeq
            
        let getAllBySubscription (subscriptionId: SubscriptionId) =
            context.Articles
                .Where(fun x -> x.SubscriptionId = subscriptionId)
            |> Query.toList
            |> Seq.map mapToArticle
            |> List.ofSeq

        let save (article: Article) =
            SubscriptionRepositoryImpl.getOrAddNew article.Id context.Articles
            |> mapFromArticle article
            context.SaveChanges() |> ignore

        let filterExistingArticles (subscriptionId: SubscriptionId) (guids: string list) =
            if List.length guids = 0 then
                []
            else if List.contains null guids then
                failwith "Feed GUID is null"
            else
                let guidsA = Array.ofList guids
                context.Articles
                    .Where(fun x -> x.SubscriptionId = subscriptionId && guidsA.Contains(x.Guid))
                    .Select(fun x -> x.Guid)
                    .ToList()
                |> List.ofSeq
                
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