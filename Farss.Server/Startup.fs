namespace Farss.Server

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.Extensions.DependencyInjection

open Giraffe
open Microsoft.Extensions.Configuration
open CompositionRoot
open Microsoft.Extensions.Hosting

type Startup(configuration: IConfiguration) =
    member this.ConfigureServices(services: IServiceCollection) =
        let connectionString = Postgres.loadConnectionString configuration
        let cr = createCompositionRoot connectionString
        services.Add(cr) |> ignore
        
        services.Configure<KestrelServerOptions>(
                                                    fun (opt: KestrelServerOptions) ->
                                                        opt.AllowSynchronousIO <- true
                                                ) |> ignore
        
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.EnvironmentName = Environments.Development then 
            app.UseDeveloperExceptionPage() |> ignore

        app.UseGiraffe (this.CreateWebApp())

    abstract member CreateWebApp: unit -> HttpHandler

    default this.CreateWebApp () = Farss.Giraffe.createWebApp ()

