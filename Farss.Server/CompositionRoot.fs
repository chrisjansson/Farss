﻿module CompositionRoot

open Farss.Server.BackgroundTaskQueue
open FetchArticlesHostedService
open JobExecutorHostedService
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Hosting
open ORMappingConfiguration
open Postgres
open Microsoft.Extensions.DependencyInjection
open Persistence
open Microsoft.AspNetCore.Builder

type IServiceCollection with

    member x.Add(source: IServiceCollection) =
        for service in source do
            x.Add(service)

        x

let createCompositionRoot (connectionString: PostgresConnectionString) : IServiceCollection =
    let services = ServiceCollection()

    services.AddRouting().AddResponseCaching().AddResponseCompression() |> ignore


    services.AddSingleton(connectionString) |> ignore
    
    services.AddHttpContextAccessor() |> ignore
    services.AddMemoryCache() |> ignore

    services.AddDbContext<ReaderContext>(fun sp options ->
        let cs = createConnectionString connectionString
        options.UseNpgsql(cs) |> ignore
        ())
    |> ignore

    services.AddScoped<SubscriptionRepository>(fun s ->
        let context = s.GetRequiredService<ReaderContext>()
        SubscriptionRepositoryImpl.create context)
    |> ignore
    
    services.AddScoped<BackendSubscriptionRepository>(fun s ->
        let context = s.GetRequiredService<ReaderContext>()
        BackendSubscriptionRepositoryImpl.create context)
    |> ignore

    services.AddScoped<ArticleRepository>(fun s ->
        let context = s.GetRequiredService<ReaderContext>()
        ArticleRepositoryImpl.create context)
    |> ignore
    
    services.AddScoped<BackendArticleRepository>(fun s ->
        let context = s.GetRequiredService<ReaderContext>()
        BackendArticleRepositoryImpl.create context)
    |> ignore
    
    services.AddScoped<HttpCacheRepository>(fun s ->
        let context = s.GetRequiredService<ReaderContext>()
        HttpCacheRepositoryImpl.create context)
    |> ignore

    services.AddScoped<FileRepository>(fun s ->
        let context = s.GetRequiredService<ReaderContext>()
        FileRepositoryImpl.create context)
    |> ignore
    
    services.AddTransient<TenantProvider>() |> ignore

    services.AddTransient<FeedReaderAdapter.FeedReaderAdapter>(fun sp ->
        let repository = sp.GetRequiredService<HttpCacheRepository>()
        FeedReaderAdapter.createAdapter FeedReaderAdapter.downloadBytesAsync (FeedReaderAdapter.downloadAsync repository)
    )
    |> ignore

    services.AddSingleton<IHostedService, FetchArticlesHostedService>() |> ignore
    services.AddSingleton<IHostedService, QueueFetchArticles>() |> ignore
    services.AddSingleton<IHostedService, JobExecutorHostedService>() |> ignore
    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>() |> ignore

    services :> IServiceCollection
