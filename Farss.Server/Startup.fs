namespace Farss.Server

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection

open Giraffe
open Persistence
open Marten
open Postgres

type Startup() =

    //todo: read configuration from somewhere
    member this.ConfigureServices(services: IServiceCollection) =
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

    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        if env.IsDevelopment() then 
            app.UseDeveloperExceptionPage() |> ignore

        app.UseGiraffe (this.CreateWebApp())

    abstract member CreateWebApp: unit -> HttpHandler

    default this.CreateWebApp () = Farss.Giraffe.createWebApp ()
