module ORMappingConfiguration

open System
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
        
    [<DefaultValue>]
    val mutable persistedUsers: DbSet<PersistedUser>

    member x.Users
        with get () = x.persistedUsers
        and set v = x.persistedUsers <- v

    override x.OnModelCreating(mb) =

        let tenantId = Guid.Parse "D6B09244-40DA-4454-A214-E4F13798E245"
        
        mb
            .Entity<PersistedArticle>()
            .HasOne(fun x -> x.Subscription)
            .WithMany(fun x -> x.Articles :> IEnumerable<_>)
        |> ignore

        mb
            .Entity<PersistedArticle>()
            .Property<Guid>("TenantId")
        |> ignore
        
        mb
            .Entity<PersistedArticle>()
            .HasQueryFilter(fun a -> EF.Property<Guid>(a, "TenantId") = tenantId)
        |> ignore
        
        mb
            .Entity<PersistedSubscriptionLogEntry>()
            .HasOne(fun x -> x.Subscription)
            .WithMany()
        |> ignore
                
        mb
            .Entity<PersistedSubscriptionLogEntry>()
            .HasQueryFilter(fun e -> EF.Property<Guid>(e, "TenantId") = tenantId)
        |> ignore
        
        mb
            .Entity<PersistedSubscriptionLogEntry>()
            .Property<Guid>("TenantId")
        |> ignore
        
        mb
            .Entity<PersistedSubscription>()
            .HasOne<PersistedFile>()
            .WithMany()
            .HasForeignKey(nameof Unchecked.defaultof<PersistedSubscription>.IconId)
        |> ignore
        
        mb
            .Entity<PersistedSubscription>()
            .Property<Guid>("TenantId")
        |> ignore
        
        mb
            .Entity<PersistedSubscription>()
            .HasQueryFilter(fun e -> EF.Property<Guid>(e, "TenantId") = tenantId)
        |> ignore
        
        mb
            .Entity<PersistedUser>()
            .Property(fun e -> e.Username)
            .IsRequired()
        |> ignore
        
        mb
            .Entity<PersistedUser>()
            .HasIndex(fun e -> e.Username :> obj)
            .IsUnique()
        |> ignore
