module Spec

open TestStartup
open Microsoft.Extensions.DependencyInjection

type TC<'C> = 'C * TestWebApplicationFactory
type ATC<'C> = Async<TC<'C>>

let inScope op (f: TestWebApplicationFactory) =
    use scope = f.Server.Host.Services.CreateScope()
    op scope.ServiceProvider

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