namespace Farss.Server

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.Extensions.DependencyInjection

open Microsoft.Extensions.Configuration
open CompositionRoot
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Falco


type Startup(configuration: IConfiguration) =
    
    /// The default exception handler, attempts to logs exception (if exists) and returns HTTP 500
    let defaultExceptionHandler 
        (ex : Exception)
        (log : ILogger) : HttpHandler =
        let logMessage = sprintf "Server error: %s\n\n%s" ex.Message ex.StackTrace
        log.Log(LogLevel.Error, logMessage)        
        
        Response.withStatusCode 500
        >> Response.ofEmpty
            
    /// Returns HTTP 404
    let defaultNotFoundHandler : HttpHandler =    
        Response.withStatusCode 404
        >> Response.ofEmpty
    
    member this.ConfigureServices(services: IServiceCollection) =
        let connectionString = Postgres.loadConnectionString configuration
        let cr = createCompositionRoot connectionString
        services.Add(cr) |> ignore
        
        services.AddSpaStaticFiles(
            fun c ->
                c.RootPath <- "wwwroot"
                ()
                                  )
        
        services.Configure<KestrelServerOptions>(
            fun (opt: KestrelServerOptions) ->
                opt.AllowSynchronousIO <- true
        ) |> ignore
        
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        let isDevelopment = env.EnvironmentName = Environments.Development
        
        if  isDevelopment then 
            app.UseDeveloperExceptionPage() |> ignore
        else
            app.UseExceptionMiddleware(defaultExceptionHandler) |> ignore
        app
            .UseResponseCaching()
            .UseResponseCompression()
            .UseStaticFiles()
            .UseRouting()
            .UseHttpEndPoints(Farss.Giraffe.createWebAppFalco ())
            |> ignore
            
        app.UseSpa(
            fun spa ->
                if isDevelopment then
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:8080")
                else
                    ()
                )
            |> ignore
            

        
//        app.UseExceptionMiddleware(defaultExceptionHandler)
//            .UseResponseCaching()
//            .UseResponseCompression()
//            .UseStaticFiles()
//            .UseRouting()
//            .UseHttpEndPoints(endpoints)
//            .UseNotFoundHandler(defaultNotFoundHandler)
