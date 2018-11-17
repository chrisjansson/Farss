module DatabaseTesting

open Postgres
open Npgsql
open Marten
open System
open Microsoft.Extensions.Configuration

let initializeTestDatabase (masterDatabase: PostgresConnectionString) databaseName =
    let connectionString = createConnectionString masterDatabase
    use dbConnection = new NpgsqlConnection(connectionString)
    dbConnection.Open()

    let drop = dbConnection.CreateCommand()
    drop.CommandText <- sprintf "DROP DATABASE IF EXISTS %s" databaseName
    drop.ExecuteNonQuery() |> ignore

    let create = dbConnection.CreateCommand()
    create.CommandText <- sprintf "CREATE DATABASE %s" databaseName
    create.ExecuteNonQuery() |> ignore

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

let createFixture test () = 
    let configuration = buildConfiguration ()
    let connectionStringData = Postgres.loadConnectionString configuration
    
    let databaseName = "farss_tests"
    initializeTestDatabase connectionStringData databaseName

    let testConnectionString = { connectionStringData with Database = databaseName }
    
    let cs = createConnectionString testConnectionString
    let store = DocumentStore.For(cs)
    
    use fixture = new DatabaseTestFixture(testConnectionString, store)
    test fixture