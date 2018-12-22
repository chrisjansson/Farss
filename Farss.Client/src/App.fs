module App

open System
open Elmish
open Elmish.React
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

module GuiCmd =
    let loadSubsAndArticles =
        let inner () = 
            ApiClient.getSubscriptions ()
            |> PromiseResult.bind(fun r -> ApiClient.getArticles () |> PromiseResult.map (fun r2 -> r, r2))

        Cmd.ofPromiseResult inner () Msg.Loaded Msg.LoadingError

    let deleteSubscription (id: Guid) =
        let dto: Dto.DeleteSubscriptionDto = { Id = Some id }
        Cmd.ofPromiseResult ApiClient.deleteSubscription dto (fun _ -> SubscriptionDeleted) SubscriptionDeleteFailed

    let alert (message: string) =
        Cmd.ofSub (fun _ -> Fable.Import.Browser.window.alert message)

let init(): Model * Cmd<Msg> = 
    
    let cmd = GuiCmd.loadSubsAndArticles
    Loading, cmd

let update (msg:Msg) (model:Model) =
    match msg with
    | Loaded (subs, articles) -> Model.Loaded (subs, articles), Cmd.none
    | LoadingError _ -> model, (GuiCmd.alert "Datta loading error hurr durr")
    | DeleteSubscription id -> 
        let cmd = GuiCmd.deleteSubscription id
        Loading, cmd
    | SubscriptionDeleted ->
        init()
    | SubscriptionDeleteFailed _ -> model, (GuiCmd.alert "Subscription delete failed")

module HTML = Html
        
let renderLoading () = 
    HTML.div [] [ HTML.str "Loading..."  ]

let renderLoaded (model: (Dto.SubscriptionDto list * Dto.ArticleDto list)) =
    let subscriptions, articles = model
    
    let renderSubscription (subscription: SubscriptionDto) =
        HTML.div [] [
            HTML.str subscription.Url
            HTML.input [ HTML.Attr.Type "button"; HTML.Attr.Value "x"; HTML.Attr.OnClick (DeleteSubscription subscription.Id)  ] 
        ]
        
    let renderArticle (article: ArticleDto) =
        HTML.str article.Title

    HTML.div [] [
        HTML.div [] [
            HTML.h1 [] [HTML.str "Subscriptions"]
            HTML.fragment () [
                yield! subscriptions |> List.map (fun s -> renderSubscription s)
            ]
        ] 
        HTML.div [] [
            HTML.h1 [] [HTML.str "Articles"]
            HTML.fragment () [
                yield! articles |> List.map renderArticle
            ]
        ]
    ]

let view (model:Model) dispatch =
    match model with
    | Loading -> HTML.run (renderLoading ()) dispatch
    | Model.Loaded (subs, articles) -> HTML.run (renderLoaded (subs, articles)) dispatch

Program.mkProgram init update view
    |> Program.withReact "elmish-app"
    |> Program.withConsoleTrace
    |> Program.run
