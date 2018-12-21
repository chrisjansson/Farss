module CompositionRoot

open Postgres
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Marten
open Giraffe
open Persistence
open System

type IServiceCollection with    
    member x.Add(source: IServiceCollection) =
        for service in source do    
            x.Add(service)
        x

let createCompositionRoot (connectionString: PostgresConnectionString): IServiceCollection =
    let services = ServiceCollection()
    services.AddSingleton(connectionString) |> ignore

    services.AddGiraffe() |> ignore
        
    services.AddSingleton<IDocumentStore>(fun s -> 
        let connectionString = s.GetRequiredService<PostgresConnectionString>()
        let cs = Postgres.createConnectionString connectionString
        let store = DocumentStore.For(fun a -> 
            a.Connection(cs)
            a.RegisterDocumentType<Domain.Subscription>()
            a.RegisterDocumentType<Domain.Article>()
            a.PLV8Enabled <- false
            a.DdlRules.TableCreation <- CreationStyle.DropThenCreate)
        store.Schema.ApplyAllConfiguredChangesToDatabase() |> ignore
        store :> IDocumentStore) |> ignore


    services.AddScoped<IDocumentSession>(fun s ->
        let store = s.GetRequiredService<IDocumentStore>()
        store.LightweightSession()) |> ignore

    services.AddScoped<SubscriptionRepository>(fun s -> 
        let session = s.GetRequiredService<IDocumentSession>()
        Persistence.SubscriptionRepositoryImpl.create session) |> ignore

    services.AddScoped<ArticleRepository>(fun s -> 
        let session = s.GetRequiredService<IDocumentSession>()
        Persistence.ArticleRepositoryImpl.create session) |> ignore

    services.AddSingleton<FeedReaderAdapter.FeedReaderAdapter>(FeedReaderAdapter.createAdapter FeedReaderAdapter.downloadBytesAsync) |> ignore

    services.AddSingleton<Giraffe.Serialization.Json.IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer()) |> ignore

    services :> IServiceCollection
