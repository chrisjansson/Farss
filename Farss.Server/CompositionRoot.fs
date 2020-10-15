module CompositionRoot

open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Hosting
open Postgres
open Microsoft.Extensions.DependencyInjection
open Marten
open Giraffe
open Persistence

type IServiceCollection with    
    member x.Add(source: IServiceCollection) =
        for service in source do    
            x.Add(service)
        x

let createCompositionRoot (connectionString: PostgresConnectionString): IServiceCollection =
    let services = ServiceCollection()
    services.AddSingleton(connectionString) |> ignore

    services.AddGiraffe() |> ignore
        
    services.AddScoped<ReaderContext>(fun sp ->
        let cs = sp.GetRequiredService<PostgresConnectionString>()
        let cs = Postgres.createConnectionString connectionString
        let databaseOptions =
            DbContextOptionsBuilder<ReaderContext>()
                .UseNpgsql(cs)
                .Options

        new ReaderContext(databaseOptions)
        ) |> ignore
        
    services.AddSingleton<IDocumentStore>(fun s -> 
        let connectionString = s.GetRequiredService<PostgresConnectionString>()
        let cs = Postgres.createConnectionString connectionString
        let store = DocumentStore.For(fun a -> 
            a.Connection(cs)
            a.RegisterDocumentType<Domain.Subscription>()
            a.RegisterDocumentType<Domain.Article>()
            a.PLV8Enabled <- false
            a.DdlRules.TableCreation <- CreationStyle.DropThenCreate
            a.Schema.For<Domain.Article>().ForeignKey<Domain.Subscription>(fun a -> a.Subscription :> obj) |> ignore
            )
        store.Schema.ApplyAllConfiguredChangesToDatabase() |> ignore
        store :> IDocumentStore) |> ignore

    services.AddScoped<IDocumentSession>(fun s ->
        let store = s.GetRequiredService<IDocumentStore>()
        store.LightweightSession()) |> ignore

    services.AddScoped<SubscriptionRepository>(fun s -> 
        let context = s.GetRequiredService<ReaderContext>()
        SubscriptionRepositoryImpl.create context) |> ignore

    services.AddScoped<ArticleRepository>(fun s -> 
        let context = s.GetRequiredService<ReaderContext>()
        ArticleRepositoryImpl.create context) |> ignore

    services.AddSingleton<FeedReaderAdapter.FeedReaderAdapter>(FeedReaderAdapter.createAdapter FeedReaderAdapter.downloadBytesAsync) |> ignore

    services.AddSingleton<Giraffe.Serialization.Json.IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer()) |> ignore

    services.AddSingleton<IHostedService, FetchArticlesHostedService.FetchArticlesHostedService>() |> ignore

    services :> IServiceCollection
