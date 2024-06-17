module ORMappingConfiguration

open System
open System.Security.Claims
open Entities
open System.Collections.Generic
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Metadata.Builders
open System.Linq

type TenantProvider(httpContextAccessor: IHttpContextAccessor) =
    member _.TenantId with get() = 
        if httpContextAccessor <> null && httpContextAccessor.HttpContext <> null then
            let user = httpContextAccessor.HttpContext.User
            let claim = user.FindFirst(ClaimTypes.NameIdentifier)
            Guid.Parse(claim.Value)
        else
            Guid.Empty

type ReaderContext(options, tenantProvider: TenantProvider) =
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
        let inline configureTenancy(mb: EntityTypeBuilder<'T>) =
            //mb.Property<Guid>("TenantId") |> ignore
            mb.HasOne<PersistedUser>().WithMany().HasForeignKey("TenantId") |> ignore

            mb.HasQueryFilter(fun a -> EF.Property<Guid>(a, "TenantId") = tenantProvider.TenantId)
            |> ignore
        
        mb
            .Entity<PersistedArticle>()
            .HasOne(fun x -> x.Subscription)
            .WithMany(fun x -> x.Articles :> IEnumerable<_>)
        |> ignore

        configureTenancy(mb.Entity<PersistedArticle>())

        mb
            .Entity<PersistedSubscriptionLogEntry>()
            .HasOne(fun x -> x.Subscription)
            .WithMany()
        |> ignore

        configureTenancy(mb.Entity<PersistedSubscriptionLogEntry>())

        mb
            .Entity<PersistedSubscription>()
            .HasOne<PersistedFile>()
            .WithMany()
            .HasForeignKey(nameof Unchecked.defaultof<PersistedSubscription>.IconId)
        |> ignore

        configureTenancy(mb.Entity<PersistedSubscription>())

        mb.Entity<PersistedUser>().Property(fun e -> e.Username).IsRequired() |> ignore

        mb.Entity<PersistedUser>().HasIndex(fun e -> e.Username :> obj).IsUnique()
        |> ignore
        
    override x.SaveChanges() =
        let tenantIdEntities =
            x.ChangeTracker
                .Entries()
                .Where(fun e -> e.Properties.Any(fun p -> p.Metadata.Name = "TenantId"))
        for e in tenantIdEntities do
            let p = e.Property("TenantId")
            if p.CurrentValue = Guid.Empty then do
                p.CurrentValue <- tenantProvider.TenantId
        
        base.SaveChanges()

    override x.SaveChangesAsync(ct) =
        let tenantIdEntities =
            x.ChangeTracker
                .Entries()
                .Where(fun e -> e.Properties.Any(fun p -> p.Metadata.Name = "TenantId"))
        for e in tenantIdEntities do
            let p = e.Property("TenantId")
            if p.CurrentValue = Guid.Empty then do
                p.CurrentValue <- tenantProvider.TenantId
        
        base.SaveChangesAsync(ct)
