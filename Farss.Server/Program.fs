namespace Farss.Server

open System.IO
open System.Reflection
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open Npgsql
open ORMappingConfiguration
open grate.DependencyInjection
open grate.Migration
open grate.postgresql.DependencyInjection
open Microsoft.Extensions.Logging

module Program =
    let exitCode = 0

    let CreateWebHostBuilder args =
        WebHost.CreateDefaultBuilder(args).UseStartup<Startup>()

    let rec setupTargetSchema (fromCs: string) (toCs: string) =
        task {
            do! migrate fromCs

            let options = DbContextOptionsBuilder<ReaderContext>().UseNpgsql(toCs).Options
            let ctx = new ReaderContext(options, null)
            ctx.Database.EnsureDeleted() |> ignore
            ctx.Database.EnsureCreated() |> ignore
            return ()
        }

    and migrate (fromCs: string) =
        task {
            let serviceCollection = ServiceCollection()

            serviceCollection.AddLogging(fun c -> c.AddSimpleConsole().SetMinimumLevel(LogLevel.Warning) |> ignore)
            |> ignore

            let builder = NpgsqlConnectionStringBuilder(fromCs)
            builder.Database <- "postgres"
            let adminCs = builder.ToString()

            serviceCollection.AddGrate(fun builder ->
                let scriptDirectory = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "db")
                builder
                    .WithUserTokens([| "SchemaName=public" |])
                    .WithConnectionString(fromCs)
                    .WithAdminConnectionString(adminCs)
                    .WithSqlFilesDirectory(scriptDirectory)
                |> ignore)
            |> ignore

            serviceCollection.UsePostgreSQL() |> ignore
            let serviceProvider = serviceCollection.BuildServiceProvider()
            let grateMigrator = serviceProvider.GetRequiredService<IGrateMigrator>()
            do! grateMigrator.Migrate()
        }

    [<EntryPoint>]
    let main args =
        match args with
        | [| "setuptargetschema"; fromCs; toCs |] ->
            let t = setupTargetSchema fromCs toCs
            t.Wait()
            exitCode
        | _ ->
            let webHost = CreateWebHostBuilder(args).Build()

            let startup () =
                use scope = webHost.Services.CreateScope()
                let readerContext = scope.ServiceProvider.GetRequiredService<ReaderContext>()
                readerContext.Database.EnsureCreated() |> ignore

            startup ()

            webHost.Run()
            exitCode
