module DatabaseTesting

open Postgres
open Npgsql
open Marten
open System
open Microsoft.Extensions.Configuration

let mutable hasCreatedDatabase = false

let initializeTestDatabase (masterDatabase: PostgresConnectionString) databaseName =
    let connectionString = createConnectionString masterDatabase
    use dbConnection = new NpgsqlConnection(connectionString)
    dbConnection.Open()

    if not hasCreatedDatabase then do
        let kill = dbConnection.CreateCommand()
        kill.CommandText <- (sprintf """SELECT 
        pg_terminate_backend(pid) 
    FROM 
        pg_stat_activity 
    WHERE 
        -- don't kill my own connection!
        pid <> pg_backend_pid()
        -- don't kill the connections to other databases
        AND datname = '%s'
        ;""" databaseName)
        kill.ExecuteNonQuery() |> ignore

        let drop = dbConnection.CreateCommand()
        drop.CommandText <- sprintf "DROP DATABASE IF EXISTS %s" databaseName
        drop.ExecuteNonQuery() |> ignore

        let create = dbConnection.CreateCommand()
        create.CommandText <- sprintf "CREATE DATABASE %s" databaseName
        create.ExecuteNonQuery() |> ignore

        hasCreatedDatabase <- true
    let cs2 = createConnectionString <| { masterDatabase with Database = databaseName }
    use dbConnection2 = new NpgsqlConnection(cs2)
    dbConnection2.Open()
        
    let deleteSchema = dbConnection2.CreateCommand()
    deleteSchema.CommandText <- "DROP SCHEMA IF EXISTS public CASCADE"
    deleteSchema.ExecuteNonQuery() |> ignore

    let createSchema = dbConnection2.CreateCommand()
    createSchema.CommandText <- "CREATE SCHEMA public"
    createSchema.ExecuteNonQuery() |> ignore


type DatabaseTestFixture(connectionString: PostgresConnectionString, documentStore: DocumentStore) =
    member __.ConnectionString with get () = connectionString
    member __.DocumentStore with get () = documentStore

    interface IDisposable with
        member this.Dispose() =
            this.DocumentStore.Dispose()

let buildConfiguration () =
    let envConfiguration = ConfigurationBuilder()
                            .AddEnvironmentVariables()
                            .Build()

    let isCi = not <| System.String.IsNullOrWhiteSpace(envConfiguration.GetValue<string>("APPVEYOR"))

    let cb =
        ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json")

    let cb = 
        if isCi then
            cb.AddJsonFile("appsettings.appveyor.json")
        else
            cb
    cb.Build()

let createFixture2 () = 
    let configuration = buildConfiguration ()
    let connectionStringData = loadConnectionString configuration
    
    let databaseName = "farss_tests"
    initializeTestDatabase connectionStringData databaseName

    let testConnectionString = { connectionStringData with Database = databaseName }
    
    let cs = createConnectionString testConnectionString
    let store = DocumentStore.For(fun a -> 
        a.Connection(cs)
        a.PLV8Enabled <- false
        a.RegisterDocumentType<Domain.Subscription>()
        a.RegisterDocumentType<Domain.Article>()
        a.DdlRules.TableCreation <- CreationStyle.DropThenCreate)
    


    new DatabaseTestFixture(testConnectionString, store)

let createFixture test () = 
    use fixture = createFixture2 ()
    test fixture