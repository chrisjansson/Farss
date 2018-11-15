module CompositionRoot

open Postgres
open Microsoft.Extensions.Configuration
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
        
    services.AddSingleton<IDocumentStore>(fun s -> 
        let connectionString = s.GetRequiredService<PostgresConnectionString>()
        let cs = Postgres.createConnectionString connectionString
        DocumentStore.For(cs) :> IDocumentStore) |> ignore

    services.AddScoped<IDocumentSession>(fun s ->
        let store = s.GetRequiredService<IDocumentStore>()
        store.LightweightSession()) |> ignore

    services.AddScoped<FeedRepository>(fun s -> 
        let session = s.GetRequiredService<IDocumentSession>()
        Persistence.FeedRepositoryImpl.create session) |> ignore

    services.AddSingleton<FeedReaderAdapter.FeedReaderAdapter>(FeedReaderAdapter.createAdapter ()) |> ignore

    services :> IServiceCollection
