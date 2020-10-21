module TestStartup

open System
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open FeedReaderAdapter
open DatabaseTesting
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration

type InMemoryFeedReader =
    {
        Adapter: FeedReaderAdapter
        Add: string * string -> unit
    }

let createInMemoryFeedReader (): InMemoryFeedReader =  
    let mutable map: Map<string, string> = Map.empty

    let add (url: string, content: string): unit =
        map <- Map.add url content map

    let getFromUrlAsync = fun url -> 
        match Map.tryFind url map with
        | Some v -> System.Text.Encoding.UTF8.GetBytes(v) |> async.Return
        | None -> raise (new System.Exception())

    let adapter: FeedReaderAdapter = FeedReaderAdapter.createAdapter getFromUrlAsync

    {
        Add = add
        Adapter = adapter
    }

type ServiceProvider(serviceProvider: IServiceProvider) =
    member x.GetService<'T>() =
        serviceProvider.GetRequiredService(typeof<'T>) :?> 'T

type TestWebApplicationFactory(databaseFixture: DatabaseTestFixture) =
    inherit WebApplicationFactory<Farss.Server.Startup>()

    member val FakeFeedReader: InMemoryFeedReader = createInMemoryFeedReader ()

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
    
    member this.WithScope(f) = async {
            use scope = this.Server.Host.Services.CreateScope()
            
            do! f (ServiceProvider(scope.ServiceProvider)) |> Async.Ignore
        }
        