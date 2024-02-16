module ORMappingConfiguration

open Entities
open System.Collections.Generic
open Microsoft.EntityFrameworkCore

type ReaderContext(options) =
    inherit DbContext(options)

    [<DefaultValue>]
    val mutable subscriptions: DbSet<PersistedSubscription>

    member x.Subscriptions
        with get () = x.subscriptions
        and set v = x.subscriptions <- v

    [<DefaultValue>]
    val mutable articles: DbSet<PersistedArticle>

    member x.Articles
        with get () = x.articles
        and set v = x.articles <- v

    [<DefaultValue>]
    val mutable files: DbSet<PersistedFile>

    member x.Files
        with get () = x.files
        and set v = x.files <- v

    [<DefaultValue>]
    val mutable httpCacheEntry: DbSet<PersistedHttpCacheEntry>

    member x.HttpCacheEntries
        with get () = x.httpCacheEntry
        and set v = x.httpCacheEntry <- v


    [<DefaultValue>]
    val mutable persistedSubscriptionLogEntries: DbSet<PersistedSubscriptionLogEntry>

    member x.SubscriptionLogEntries
        with get () = x.persistedSubscriptionLogEntries
        and set v = x.persistedSubscriptionLogEntries <- v

    override x.OnModelCreating(mb) =

        mb
            .Entity<PersistedArticle>()
            .HasOne(fun x -> x.Subscription)
            .WithMany(fun x -> x.Articles :> IEnumerable<_>)
        |> ignore

        mb
            .Entity<PersistedSubscriptionLogEntry>()
            .HasOne(fun x -> x.Subscription)
            .WithMany()
        |> ignore
