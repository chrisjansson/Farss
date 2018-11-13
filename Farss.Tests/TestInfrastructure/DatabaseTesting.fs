module DatabaseTesting

open Postgres
open Npgsql
open Marten
open System

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

let createFixture test () = 
    let connectionStringData = { Host = "localhost"; Database = "postgres"; Username = "postgres"; Password = "postgres" }

    let databaseName = "farss_tests"
    initializeTestDatabase connectionStringData databaseName

    let testConnectionString = { connectionStringData with Database = databaseName }
    
    let cs = createConnectionString testConnectionString
    let store = DocumentStore.For(cs)
    
    use fixture = new DatabaseTestFixture(testConnectionString, store)
    test fixture