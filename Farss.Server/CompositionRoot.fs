﻿module CompositionRoot

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


type Canary() =
    interface IDisposable with 
        member this.Dispose() =
            ()

let createCompositionRoot (connectionString: PostgresConnectionString): IServiceCollection =
    let services = ServiceCollection()
    services.AddSingleton(connectionString) |> ignore

    services.AddGiraffe() |> ignore
        
    services.AddSingleton<IDocumentStore>(fun s -> 
        let connectionString = s.GetRequiredService<PostgresConnectionString>()
        let cs = Postgres.createConnectionString connectionString
        DocumentStore.For(fun a -> 
        a.Connection(cs)
        a.DdlRules.TableCreation <- CreationStyle.DropThenCreate) :> IDocumentStore) |> ignore

    services.AddScoped<IDocumentSession>(fun s ->
        let store = s.GetRequiredService<IDocumentStore>()
        store.LightweightSession()) |> ignore

    services.AddScoped<FeedRepository>(fun s -> 
        let session = s.GetRequiredService<IDocumentSession>()
        Persistence.FeedRepositoryImpl.create session) |> ignore

    services.AddSingleton<FeedReaderAdapter.FeedReaderAdapter>(FeedReaderAdapter.createAdapter ()) |> ignore

    services.AddSingleton<Canary>(fun _ -> new Canary()) |> ignore

    services :> IServiceCollection
