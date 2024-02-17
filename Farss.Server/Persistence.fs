module Persistence

open System
open Domain
open Microsoft.EntityFrameworkCore

type SubscriptionRepository = {
    get: SubscriptionId -> Subscription
    getAll: unit -> Subscription list
    save: Subscription -> unit
    delete: SubscriptionId -> unit
    storeLog: SubscriptionId * Result<string, string> * DateTimeOffset -> unit
}

type ArticleRepository = {
    getAll: unit -> Article list
    getTop: (Guid option * int) -> Article list
    save: Article -> unit
    filterExistingArticles: SubscriptionId -> string list -> string list
    getAllBySubscription: SubscriptionId -> Article list
}

type FileRepository = {
    get: Guid -> File
    save: File -> unit
    delete: Guid -> unit
}

type HttpCacheRepository = {
    getCacheHeaders: string -> CacheHeaders option
    save: string -> string -> string option -> DateTimeOffset option -> unit
    getContent: Guid -> string
}

open System.Linq.Expressions
open System.Linq

module Query =

    let where (predicate: Expression<Func<_, bool>>) (query: IQueryable<_>) = query.Where(predicate)

    let orderByDescending (selector: Expression<Func<_, _>>) (query: IQueryable<_>) = query.OrderByDescending(selector)

    let take (count: int) (query: IQueryable<_>) = query.Take(count)

    let single (query: IQueryable<_>) = query.Single()

    let toList (query: IQueryable<_>) = query.ToList()

    let singleP (predicate: Expression<Func<_, bool>>) (query: IQueryable<_>) = query.Single(predicate)

    type Expr =
        static member Quote(e: Expression<System.Func<_, _>>) = e

type Q =
    static member orderByDescending(selector: Expression<Func<_, _>>, query: IQueryable<_>) = query.OrderByDescending(selector)


let (~+) (expr: Expression<Func<_, _>>) = Query.Expr.Quote expr

open Entities
open ORMappingConfiguration

module SubscriptionRepositoryImpl =
    let private mapToSubscription (s: PersistedSubscription) : Subscription = {
        Id = s.Id
        Url = s.Url
        Title = s.Title
        Icon = Option.ofNullable s.IconId
    }

    let private mapFromSubscription (s: Subscription) (t: PersistedSubscription) =
        t.Id <- s.Id
        t.Title <- s.Title
        t.Url <- s.Url
        t.IconId <- Option.toNullable s.Icon

    let getOrAddNew<'T when 'T: (new: unit -> 'T) and 'T: not struct and 'T: null> (id: Guid) (set: DbSet<_>) =
        set.Find id
        |> Option.ofObj
        |> Option.defaultWith (fun () ->
            let instance = new 'T()
            set.Add(instance) |> ignore
            instance)

    let create (context: ReaderContext) =
        let getAll () =
            context.Subscriptions |> Query.toList |> Seq.map mapToSubscription |> List.ofSeq

        let save (subscription: Subscription) =
            getOrAddNew subscription.Id context.Subscriptions
            |> mapFromSubscription subscription

            context.SaveChanges() |> ignore

        let delete (subscriptionId: SubscriptionId) =
            let s = context.Subscriptions.Find(subscriptionId)
            context.Subscriptions.Remove(s) |> ignore
            context.SaveChanges() |> ignore

        let get (subscriptionId: SubscriptionId) =
            context.Subscriptions.Single(fun s -> s.Id = subscriptionId)
            |> mapToSubscription

        let storeLog (subscriptionId: SubscriptionId, result: Result<string, string>, timestamp: DateTimeOffset) =

            let tail =
                context.SubscriptionLogEntries
                    .Where(fun x -> x.SubscriptionId = subscriptionId)
                    .OrderByDescending(fun x -> x.Timestamp)
                    .Skip(99)
                    .Select(fun x -> x.Id)
                    .ToList()

            let entities = [|
                for id in tail do
                    PersistedSubscriptionLogEntry(Id = id)
            |]

            context.SubscriptionLogEntries.RemoveRange(entities)

            let message =
                match result with
                | Ok r -> r
                | Error e -> e

            context.SubscriptionLogEntries.Add(
                PersistedSubscriptionLogEntry(SubscriptionId = subscriptionId, Success = Result.isOk result, Message = message, Timestamp = timestamp)
            )
            |> ignore

            context.SaveChanges() |> ignore

        {
            get = get
            getAll = getAll
            save = save
            delete = delete
            storeLog = storeLog
        }


module ArticleRepositoryImpl =
    let private mapToArticle (s: PersistedArticle) : Article = {
        Id = s.Id
        Title = s.Title
        Guid = s.Guid
        Subscription = s.SubscriptionId
        Content = s.Content
        Source = s.Source
        IsRead = s.IsRead
        Timestamp = s.Timestamp
        Link = s.Link
        Summary = Option.ofObj s.Summary
    }

    let private mapFromArticle (s: Article) (t: PersistedArticle) =
        t.Id <- s.Id
        t.Title <- s.Title
        t.Guid <- s.Guid
        t.SubscriptionId <- s.Subscription
        t.Content <- s.Content
        t.Source <- s.Source
        t.IsRead <- s.IsRead
        t.Timestamp <- s.Timestamp
        t.Link <- s.Link
        t.Summary <- Option.toObj s.Summary

    let create (context: ReaderContext) : ArticleRepository =
        let getAll () =
            context.Articles |> Query.toList |> Seq.map mapToArticle |> List.ofSeq

        let getTop (feedId: Guid option, count: int) =
            context.Articles
            |> Query.orderByDescending (Query.Expr.Quote(fun (x: PersistedArticle) -> x.Timestamp))
            |> (fun q ->
                if feedId.IsSome then
                    Query.where (Query.Expr.Quote(fun (x: PersistedArticle) -> x.SubscriptionId = feedId.Value)) q
                else
                    q)
            |> Query.take count
            |> Query.toList
            |> Seq.map mapToArticle
            |> List.ofSeq

        let getAllBySubscription (subscriptionId: SubscriptionId) =
            context.Articles.Where(fun x -> x.SubscriptionId = subscriptionId)
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

                let existing =
                    context.Articles
                        .Where(fun x -> x.SubscriptionId = subscriptionId && guidsA.Contains(x.Guid))
                        .Select(fun x -> x.Guid)
                        .ToList()
                    |> Set.ofSeq

                guids |> Set.ofList |> (fun x -> Set.difference x existing) |> Set.toList

        {
            getAll = getAll
            getTop = getTop
            getAllBySubscription = getAllBySubscription
            save = save
            filterExistingArticles = filterExistingArticles
        }

module FileRepositoryImpl =
    let create (context: ReaderContext) : FileRepository =
        let get (id: Guid) : File =
            let file = context.Files.Find(id)

            {
                Id = file.Id
                FileName = file.FileName
                FileOwner = file.FileOwner
                Hash = file.Hash
                Data = file.Data
            }

        let mapFromFile (s: File) (t: PersistedFile) =
            t.Id <- s.Id
            t.FileName <- s.FileName
            t.Data <- s.Data
            t.FileOwner <- s.FileOwner
            t.Hash <- s.Hash


        let save (file: File) =
            SubscriptionRepositoryImpl.getOrAddNew file.Id context.Files |> mapFromFile file
            context.SaveChanges() |> ignore

        let delete (id: Guid) =
            let file = context.Files.Find(id)
            context.Files.Remove(file) |> ignore
            context.SaveChanges() |> ignore

        {
            get = get
            save = save
            delete = delete
        }

module HttpCacheRepositoryImpl =
    let create (context: ReaderContext) : HttpCacheRepository =
        let getCacheHeaders (url: string) : CacheHeaders option =
            let cacheEntry =
                context.HttpCacheEntries
                    .Where(fun x -> x.Url = url)
                    .Select(fun x -> {|
                        AETag = x.ETag
                        BLastModifiedDate = x.LastModifiedDate
                        CId = x.Id
                        DLastGet = x.LastGet
                    |})
                    .SingleOrDefault()
                |> (fun r -> if r = Unchecked.defaultof<_> then None else Some r)

            match cacheEntry with
            | Some ce ->
                let etag = ce.AETag |> Option.ofObj
                let lastModifiedDate = ce.BLastModifiedDate |> Option.ofNullable

                Some {
                    Id = ce.CId
                    ETag = etag
                    LastModified = lastModifiedDate
                    LastGet = ce.DLastGet
                }
            | None -> None

        let getContent (id: Guid) =
            context.HttpCacheEntries
                .Where(fun x -> x.Id = id)
                .Select(fun x -> x.Content)
                .Single()

        let save (url: string) (response: string) (etag: string option) (lastModified: DateTimeOffset option) =
            let entry =
                let e = context.HttpCacheEntries.Where(fun x -> x.Url = url).SingleOrDefault()

                if e = null then
                    let e = PersistedHttpCacheEntry()
                    e.Id <- Guid.NewGuid()
                    e.Url <- url
                    context.HttpCacheEntries.Add(e) |> ignore
                    e
                else
                    e

            entry.Content <- response
            entry.ETag <- Option.toObj etag
            entry.LastModifiedDate <- Option.toNullable lastModified
            entry.LastGet <- DateTimeOffset.UtcNow
            context.SaveChanges() |> ignore

        {
            getCacheHeaders = getCacheHeaders
            getContent = getContent
            save = save
        }
