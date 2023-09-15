namespace Farss.Server

open System
open Giraffe.HttpStatusCodeHandlers
open Giraffe
open Giraffe.EndpointRouting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.Extensions.DependencyInjection

open Microsoft.Extensions.Configuration
open CompositionRoot
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Thoth.Json.Giraffe
open Thoth.Json.Net

type Startup(configuration: IConfiguration) =    
    let errorHandler (ex : Exception) (logger : ILogger) =
        logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
        clearResponse
        >=> ServerErrors.INTERNAL_ERROR ex.Message
            
    member this.ConfigureServices(services: IServiceCollection) =
        let connectionString = Postgres.loadConnectionString configuration
        let cr = createCompositionRoot connectionString
        services.Add(cr) |> ignore
        
        services.AddGiraffe() |> ignore
        services.AddTransient<Json.ISerializer>(fun _ -> ThothSerializer(caseStrategy = CaseStrategy.CamelCase)) |> ignore
        
        services.Configure<KestrelServerOptions>(
            fun (opt: KestrelServerOptions) ->
                opt.AllowSynchronousIO <- true
        ) |> ignore
        
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        let isDevelopment = env.EnvironmentName = Environments.Development
        
        if  isDevelopment then 
            app.UseDeveloperExceptionPage() |> ignore
        else
            app.UseGiraffeErrorHandler(errorHandler) |> ignore
        app
            .UseResponseCaching()
            .UseResponseCompression()
            .UseStaticFiles()
            .UseRouting()
            .UseEndpoints(fun e -> e.MapGiraffeEndpoints(Farss.Giraffe.endpoints))
            |> ignore