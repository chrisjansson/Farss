module CompositionRoot

open Farss.Server.BackgroundTaskQueue
open FetchArticlesHostedService
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Hosting
open Postgres
open Microsoft.Extensions.DependencyInjection
open Persistence
open Microsoft.AspNetCore.Builder

type IServiceCollection with    
    member x.Add(source: IServiceCollection) =
        for service in source do    
            x.Add(service)
        x

let createCompositionRoot (connectionString: PostgresConnectionString): IServiceCollection =
    let services = ServiceCollection()

    services
        .AddRouting()
        .AddResponseCaching()
        .AddResponseCompression()
        |> ignore


    services.AddSingleton(connectionString) |> ignore
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

    services.AddScoped<Persistence.FileRepository>(fun s -> 
        let context = s.GetRequiredService<ReaderContext>()
        FileRepositoryImpl.create context) |> ignore
    
    services.AddSingleton<FeedReaderAdapter.FeedReaderAdapter>(FeedReaderAdapter.createAdapter FeedReaderAdapter.downloadBytesAsync FeedReaderAdapter.downloadAsync) |> ignore

    services.AddSingleton<IHostedService, FetchArticlesHostedService>() |> ignore
    services.AddSingleton<IHostedService, QueueFetchArticles>() |> ignore
    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>() |> ignore

    services :> IServiceCollection
