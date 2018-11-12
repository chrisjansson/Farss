module MartenCharacterizationTests

open Expecto
open Marten
open System
open System.Linq
open System.Linq.Expressions
open Npgsql

type PostgresConnectionString = 
    {
        Host: string
        Database: string
        Username: string
        Password: string
    }

type TestDocument = 
    {
        Id: Guid
        Prop1: string
        Prop2: int
    }

type Expr = 
    static member Quote(e:Expression<System.Func<_, _>>) = e

module Query = 
    let where (predicate: Expression<Func<_, bool>>) (query: IQueryable<_>) =
        query.Where(predicate)

    let single (query: IQueryable<_>) =
        query.Single()

    let toList (query: IQueryable<_>) =
        query.ToList()

[<Tests>]
let tests = testList "Marten characterization tests" [
        ptest "store document" {
            //todo: teardown

            let connectionStringData = { Host = "localhost"; Database = "postgres"; Username = "postgres"; Password = "postgres" }
            let connectionString = sprintf "host=%s;database=%s;password=%s;username=%s" connectionStringData.Host connectionStringData.Database connectionStringData.Password connectionStringData.Username
            
            let dbConnection = new NpgsqlConnection(connectionString)
            dbConnection.Open()

            let drop = dbConnection.CreateCommand()
            drop.CommandText <- "DROP DATABASE IF EXISTS farss_tests"
            drop.ExecuteNonQuery() |> ignore

            let create = dbConnection.CreateCommand()
            create.CommandText <- "CREATE DATABASE farss_tests"
            create.ExecuteNonQuery() |> ignore
            
            dbConnection.Close()
            dbConnection.Dispose()

            let testConnectionStringData = { connectionStringData with Database = "farss_tests" }
            let connectionString = sprintf "host=%s;database=%s;password=%s;username=%s" testConnectionStringData.Host testConnectionStringData.Database testConnectionStringData.Password testConnectionStringData.Username

            let store = DocumentStore.For(connectionString)

            let expected = { Id = Guid.NewGuid(); Prop1 = "Hello world"; Prop2 = 4711 }

            using (store.LightweightSession()) (fun s ->
                s.Store(expected)
                s.SaveChanges()
            )

            using (store.LightweightSession()) (fun s ->
                let actual = 
                    s.Query<TestDocument>()
                    |> Query.where (Expr.Quote (fun td -> td.Id = expected.Id))
                    |> Query.single
                
                Expect.equal actual expected "rountrip document"

                let all =
                    s.Query<TestDocument>()
                    |> Query.toList

                Expect.equal all.Count 1 "Document count"
            )
        }
    ]