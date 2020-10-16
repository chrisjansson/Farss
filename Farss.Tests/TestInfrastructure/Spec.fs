module Spec

open TestStartup
open Microsoft.Extensions.DependencyInjection
open Expecto

type TC<'C> = 'C * TestWebApplicationFactory
type ATC<'C> = Async<TC<'C>>

let inScope op (f: TestWebApplicationFactory) =
    use scope = f.Server.Host.Services.CreateScope()
    op scope.ServiceProvider

let withService<'TService> (op: 'TService -> unit) (f: TestWebApplicationFactory) =
    use scope = f.Server.Host.Services.CreateScope()
    let service = scope.ServiceProvider.GetRequiredService<'TService>()
    op service

let withServiceAsync<'TService> op (f: TestWebApplicationFactory) = async {
        use scope = f.Server.Host.Services.CreateScope()
        let service = scope.ServiceProvider.GetRequiredService<'TService>()
        do! op service
    }
    

type AsyncTestStep<'T, 'U> = ATC<'T> -> ATC<'U>

let (>>>) (l: AsyncTestStep<'a,'b>) (r: AsyncTestStep<'b,'c>): AsyncTestStep<'a, 'c> = 
    let f arg = 
        let nextAtc = l arg
        r nextAtc
    f
   

let pipe: AsyncTestStep<_, _> =
        fun atc -> async {
        let! (x, f) = atc
        return  (x, f)
    }

let Given = pipe
let When = pipe
let Then = pipe
let And = pipe

let toTest (testStep: unit -> AsyncTestStep<unit, _>) = async {
        let df = DatabaseTesting.createFixture2 ()
        use f = new TestWebApplicationFactory(df)
        f.CreateClient() |> ignore
        f.Server.AllowSynchronousIO <- true

        let stuff: ATC<unit> = async.Return ((), f)
        do! testStep() stuff |> Async.Ignore
    }
    
let spec name t = 
    testAsync name {
        do! t |> toTest
    }


module Step =
    let mapAsync (step: TC<_> -> _): AsyncTestStep<_, _> =
        fun atc -> async {
            let! (c, f) = atc
            let! result = step (c,f)
            return (result, f)
        }

    let map (step: TC<_> -> _): AsyncTestStep<_, _> =
        fun atc -> async {
            let! (c, f) = atc
            let result = step (c,f)
            return (result, f)
        }