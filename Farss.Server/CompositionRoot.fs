module CompositionRoot

open Giraffe.Serialization
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Hosting
open Postgres
open Microsoft.Extensions.DependencyInjection
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
        
    services.AddDbContext<ReaderContext>((fun sp options ->
        let cs = Postgres.createConnectionString connectionString
        options.UseNpgsql(cs) |> ignore
        ()
        )) |> ignore

    services.AddScoped<Persistence.SubscriptionRepository>(fun s -> 
        let context = s.GetRequiredService<ReaderContext>()
        SubscriptionRepositoryImpl.create context) |> ignore

    services.AddScoped<Persistence.ArticleRepository>(fun s -> 
        let context = s.GetRequiredService<ReaderContext>()
        ArticleRepositoryImpl.create context) |> ignore

    services.AddSingleton<FeedReaderAdapter.FeedReaderAdapter>(FeedReaderAdapter.createAdapter FeedReaderAdapter.downloadBytesAsync) |> ignore

    services.AddSingleton<IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer()) |> ignore

    services.AddSingleton<IHostedService, FetchArticlesHostedService.FetchArticlesHostedService>() |> ignore

    services :> IServiceCollection
