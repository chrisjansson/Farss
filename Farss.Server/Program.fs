namespace Farss.Server

open Microsoft.Extensions.DependencyInjection

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open ORMappingConfiguration

module Program =
    let exitCode = 0

    //TODO: Sanity check DB connection on startup, apply migrations etc. Early exit if not possible
    let CreateWebHostBuilder args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()            

    [<EntryPoint>]
    let main args =
        let webHost = CreateWebHostBuilder(args).Build()

        let startup () =
            use scope = webHost.Services.CreateScope()
            let readerContext = scope.ServiceProvider.GetRequiredService<ReaderContext>()
            readerContext.Database.EnsureCreated() |> ignore
        
        startup ()
        
        webHost.Run()
        exitCode
