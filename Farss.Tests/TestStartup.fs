module TestStartup

open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open FeedReaderAdapter

type FakeFeedReader =
    {
        Adapter: FeedReaderAdapter
        Add: string * string -> unit
    }

let createFakeFeedReader (): FakeFeedReader =  
    let mutable map: Map<string, string> = Map.empty

    let add (url: string, content: string): unit =
        map <- Map.add url content map

    let adapter: FeedReaderAdapter = 
        {
            getFromUrl = fun url -> 
                let content = Map.find url map
                CodeHollow.FeedReader.FeedReader.ReadFromString(content)
        }

    {
        Add = add
        Adapter = adapter
    }

type TestWebApplicationFactory() =
    inherit WebApplicationFactory<Farss.Server.Startup>()

    member val FakeFeedReader: FakeFeedReader = createFakeFeedReader ()

    override this.ConfigureWebHost(builder) =
        builder.ConfigureTestServices(
            fun services -> 
                services.AddSingleton(this.FakeFeedReader.Adapter) |> ignore
            )
        |> ignore

