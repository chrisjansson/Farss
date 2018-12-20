module App

open System
open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Dto

type Model =
    | Loading
    | Loaded of Dto.SubscriptionDto list * Dto.ArticleDto list

type Msg = 
    | Loaded of Dto.SubscriptionDto list * Dto.ArticleDto list
    | LoadingError of string
        //todo: change for domain alias
    | DeleteSubscription of Guid
    | SubscriptionDeleted
    | SubscriptionDeleteFailed of exn

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

let alert (message: string) =
    Cmd.ofSub (fun _ -> Fable.Import.Browser.window.alert message)


module GuiCmd =
    let loadSubsAndArticles () = 
        ApiClient.getSubscriptions ()
        |> PromiseResult.bind(fun r -> ApiClient.getArticles () |> PromiseResult.map (fun r2 -> r, r2))

    let deleteSubscription (id: Guid) =
        let dto: Dto.DeleteSubscriptionDto = { Id = Some id }
        ApiClient.deleteSubscription dto 
        

let init(): Model * Cmd<Msg> = 
    
    let cmd = Cmd.ofPromiseResult GuiCmd.loadSubsAndArticles () Msg.Loaded Msg.LoadingError
    Loading, cmd


let update (msg:Msg) (model:Model) =
    match msg with
    | Loaded (subs, articles) -> Model.Loaded (subs, articles), Cmd.none
    | LoadingError _ -> model, (alert "Datta loading error hurr durr")
    | DeleteSubscription id -> 
        let cmd = Cmd.ofPromiseResult GuiCmd.deleteSubscription id (fun _ -> SubscriptionDeleted) SubscriptionDeleteFailed
        Loading, cmd
    | SubscriptionDeleted ->
        init()
    | SubscriptionDeleteFailed _ -> model, (alert "Subscription delete failed")
let renderLoading () = 
    div [] [ str "Loading..." ]

let onClick dispatch msg =
    OnClick (fun _ -> dispatch msg)

let renderLoaded (model: (Dto.SubscriptionDto list * Dto.ArticleDto list)) dispatch =
    let subscriptions, articles = model
    
    let renderSubscription (subscription: SubscriptionDto) dispatch =
        div [] [
            str subscription.Url
            input [ Type "button"; Value "x"; onClick dispatch (DeleteSubscription subscription.Id)  ] 
        ]
        
    let renderArticle (article: ArticleDto) =
        str article.Title

    div [] [
        div [] [
            h1 [] [str "Subscriptions"]
            fragment [] [
                yield! subscriptions |> List.map (fun s -> renderSubscription s dispatch)
            ]
        ] 
        div [] [
            h1 [] [str "Articles"]
            fragment [] [
                yield! articles |> List.map renderArticle
            ]
        ]
    ]

let view (model:Model) dispatch =
    match model with
    | Loading -> renderLoading ()
    | Model.Loaded (subs, articles) -> renderLoaded (subs, articles) dispatch

Program.mkProgram init update view
    |> Program.withReact "elmish-app"
    |> Program.withConsoleTrace
    |> Program.run
