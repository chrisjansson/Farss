module App

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props

type Model =
    | Loading
    | Loaded of exn * exn

type Msg = 
    | Loaded of Dto.SubscriptionDto list * Dto.ArticleDto list
    | LoadingError of string

module PromiseResult =
    let map f p = promise {
        let! res = p
        return 
            match res with
            | Ok v -> Ok (f v)
            | Error e -> Error e
    }

    let bind f p = promise {
        let! res = p
        match res with
        | Ok v -> return! f v
        | Error e -> return Error e
    }    

module Cmd =
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

let init(): Model * Cmd<Msg> = 
    let loadSubsAndArticles () = 
        ApiClient.getSubscriptions ()
        |> PromiseResult.bind(fun r -> ApiClient.getArticles () |> PromiseResult.map (fun r2 -> r, r2))

    let cmd = Cmd.ofPromiseResult loadSubsAndArticles () Msg.Loaded Msg.LoadingError
    Loading, cmd

let update (msg:Msg) (model:Model) =
    model, Cmd.none

let view (model:Model) dispatch =
  div []
      [ button [  ] [ str "+" ]
        div [] [ str (string model) ]
        button [ ] [ str "-" ] ]

Program.mkProgram init update view
    |> Program.withReact "elmish-app"
    |> Program.withConsoleTrace
    |> Program.run
