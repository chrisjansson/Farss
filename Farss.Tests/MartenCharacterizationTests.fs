module MartenCharacterizationTests

open Expecto
open System
open System.Linq
open System.Linq.Expressions
open DatabaseTesting


type Expr = 
    static member Quote(e:Expression<System.Func<_, _>>) = e

module Query = 
    let where (predicate: Expression<Func<_, bool>>) (query: IQueryable<_>) =
        query.Where(predicate)

    let single (query: IQueryable<_>) =
        query.Single()

    let toList (query: IQueryable<_>) =
        query.ToList()
        
type TestDocument = 
    {
        Id: Guid
        Prop1: string
        Prop2: int
    }

[<Tests>]
let tests = 
    specs "Marten characterization tests" [
        let tests = [ "store document", fun (fixture: DatabaseTestFixture) ->
            let store = fixture.DocumentStore

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
        ]

        yield! testFixture createFixture tests
    ]