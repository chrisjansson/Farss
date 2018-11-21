module TestStartup

open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open FeedReaderAdapter
open DatabaseTesting
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration

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
                let result =
                    match Map.tryFind url map with
                    | Some v -> CodeHollow.FeedReader.FeedReader.ReadFromString(v) |> Ok
                    | None -> System.Exception() |> FetchError  |> Error
                async.Return result
        }

    {
        Add = add
        Adapter = adapter
    }

type TestWebApplicationFactory(databaseFixture: DatabaseTestFixture) =
    inherit WebApplicationFactory<Farss.Server.Startup>()

    member val FakeFeedReader: FakeFeedReader = createFakeFeedReader ()

    override this.ConfigureWebHost(builder) =
        builder
            .ConfigureAppConfiguration(fun c ->
                c.AddInMemoryCollection(Postgres.toKeyValuePairs databaseFixture.ConnectionString) |> ignore
            )
            .ConfigureTestServices(
                fun services -> 
                    services.AddSingleton(this.FakeFeedReader.Adapter) |> ignore
                )
            |> ignore

    member this.InScope<'T, 'U>(f: 'T -> 'U) =
        use scope = this.Server.Host.Services.CreateScope()
        let service = scope.ServiceProvider.GetService<'T>()
        f service

    member this.InScopeAsync<'T>(f) = async {
            use scope = this.Server.Host.Services.CreateScope()
            let service = scope.ServiceProvider.GetService<'T>()
            return! f service
        }
        