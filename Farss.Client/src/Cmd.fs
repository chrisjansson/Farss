[<RequireQualifiedAccess>]
module Cmd

open Elmish

let ofPromiseResult 
    (task: 'arg -> Fable.Import.JS.Promise<Result<'a, 'e>>) 
    (arg: 'arg) (onSuccess: 'a -> 'msg) 
    (onError: 'e -> 'msg)
    : Cmd<'msg>  =
    let executeTask task arg dispatch = promise {
            let! result = task arg
            match result with
            | Ok r -> 
                let m = onSuccess r
                dispatch m
            | Error e ->
                let m = onError e
                dispatch m
        }
            
    let bind: Sub<'msg> = 
        fun dispatch ->
            executeTask task arg dispatch |> ignore
    [bind]