module TestStartup

open Farss.Server
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.TestHost
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

type FakeFeedReader =
    {
        Adapter: FeedReaderAdapter.FeedReaderAdapter
        Add: string * string -> unit
    }

let createFakeFeedReader (): FakeFeedReader =  
    let mutable map: Map<string, string> = Map.empty

    let add (url: string, content: string): unit =
        map <- Map.add url content map

    let adapter = 
        {
            FeedReaderAdapter.FeedReaderAdapter.getFromUrl = fun url -> 
                let content = Map.find url map
                CodeHollow.FeedReader.FeedReader.ReadFromString(content)
        }

    {
        Add = add
        Adapter = adapter
    }

type TestStartup =
    inherit Startup
    override this.CreateWebApp() = Farss.Giraffe.createWebApp ()

type TestWebApplicationFactory() =
    inherit WebApplicationFactory<Farss.Server.Startup>()

    member val FakeFeedReader: FakeFeedReader = createFakeFeedReader ()

    override this.ConfigureWebHost(builder) =
        builder.ConfigureTestServices(
            fun services -> 
                services.AddSingleton(this.FakeFeedReader.Adapter) |> ignore
            )
        |> ignore

