namespace Farss.Server

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection

open Giraffe
open Persistence

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddGiraffe() |> ignore
        services.AddSingleton<FeedRepository>(Persistence.create ()) |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        if env.IsDevelopment() then 
            app.UseDeveloperExceptionPage() |> ignore

        app.UseGiraffe (this.CreateWebApp())

    abstract member CreateWebApp: unit -> HttpHandler

    default this.CreateWebApp () = Farss.Giraffe.createWebApp ()
