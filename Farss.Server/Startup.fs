namespace Farss.Server

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection

open Giraffe
open Microsoft.Extensions.Configuration
open CompositionRoot

type Startup(configuration: IConfiguration) =
    member this.ConfigureServices(services: IServiceCollection) =
        let connectionString = Postgres.loadConnectionString configuration
        let cr = CompositionRoot.createCompositionRoot connectionString
        services.Add(cr) |> ignore

    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        if env.IsDevelopment() then 
            app.UseDeveloperExceptionPage() |> ignore

        app.UseGiraffe (this.CreateWebApp())

    abstract member CreateWebApp: unit -> HttpHandler

    default this.CreateWebApp () = Farss.Giraffe.createWebApp ()

