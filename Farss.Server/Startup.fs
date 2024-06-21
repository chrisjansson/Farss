namespace Farss.Server

open System
open System.IO
open Farss.Server.TrustedProxyHeaderAuthenticationHandler
open Farss.Server.UserCache
open Giraffe.HttpStatusCodeHandlers
open Giraffe
open Giraffe.EndpointRouting
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.Extensions.DependencyInjection

open Microsoft.Extensions.Configuration
open CompositionRoot
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Thoth.Json.Giraffe
open Thoth.Json.Net

type ReplacingFileInfo(baseInfo: IFileInfo) =
    
    let replaceFile (stream: Stream) =
        use stream = stream
        use ms = new MemoryStream()
        stream.CopyTo(ms)
        ms.Position <- 0
        
        use sr = new StreamReader(ms)
        let fileContents = sr.ReadToEnd()
        let fileContents = fileContents.Replace("%APP_BASE_URL%", "hejkakabakaweee")
        
        let newMs = new MemoryStream()
        let sw = new StreamWriter(newMs)
        sw.Write(fileContents)
        sw.Flush()
        newMs.Position <- 0
        newMs 
        
    let length =
        if baseInfo.Exists then
            use ms = replaceFile (baseInfo.CreateReadStream())
            ms.Length
        else
            0
        
    interface IFileInfo with
        member x.IsDirectory with get() = baseInfo.IsDirectory
        member this.CreateReadStream() = replaceFile (baseInfo.CreateReadStream())
        member this.Exists = baseInfo.Exists
        member this.LastModified = baseInfo.LastModified
        member this.Length = length
        member this.Name = baseInfo.Name
        member this.PhysicalPath = null

type FileProvider(fileProvider: IFileProvider) =
    interface IFileProvider with
        member x.GetFileInfo(subPath) =
            let fileInfo = fileProvider.GetFileInfo(subPath)
            if fileInfo.Name = "index.html" then
                ReplacingFileInfo(fileInfo)
            else
                fileInfo
        member this.GetDirectoryContents(subpath) = fileProvider.GetDirectoryContents(subpath)
        member this.Watch(filter) = fileProvider.Watch(filter)

type Startup(configuration: IConfiguration) =    
    let errorHandler (ex : Exception) (logger : ILogger) =
        logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
        clearResponse
        >=> ServerErrors.INTERNAL_ERROR ex.Message
    
    let authenticationScheme = "DefaultScheme"
            
    member this.ConfigureServices(services: IServiceCollection) =
        let connectionString = Postgres.loadConnectionString configuration
        let cr = createCompositionRoot connectionString
        services.Add(cr) |> ignore
        
        services.AddGiraffe() |> ignore
        services.AddTransient<Json.ISerializer>(fun _ -> ThothSerializer(caseStrategy = CaseStrategy.CamelCase)) |> ignore
        services.AddTransient<UserCache>() |> ignore
        
        services.AddAuthentication(authenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, TrustedProxyHeaderAuthenticationHandler>(authenticationScheme,fun x -> ())
            |> ignore
        
        services.AddAuthorization()
            |> ignore
            
        services.Configure<KestrelServerOptions>(
            fun (opt: KestrelServerOptions) ->
                opt.AllowSynchronousIO <- true
        ) |> ignore
        
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        let isDevelopment = env.EnvironmentName = Environments.Development
        let hostingSubdir =
            configuration.GetValue<string>("urlBase")
            |> Option.ofObj
        
        if  isDevelopment then 
            app.UseDeveloperExceptionPage() |> ignore
        else
            app.UseGiraffeErrorHandler(errorHandler) |> ignore
        app
            .UseResponseCaching()
            .UseResponseCompression()
            .UseStaticFiles()
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(fun e ->
                e.MapGiraffeEndpoints(Farss.Giraffe.endpoints hostingSubdir authenticationScheme)
                e.MapFallbackToFile("index.html", StaticFileOptions(FileProvider = FileProvider(env.WebRootFileProvider))) |> ignore
            )
            |> ignore
