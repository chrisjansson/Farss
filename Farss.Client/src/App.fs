module App

open System
open Elmish
open Elmish.React
open Dto
open Html
open Model

let init(): Model * Cmd<Msg> = 
    let cmd = GuiCmd.loadSubsAndArticles
    Loading, cmd

let update (msg:Msg) (model:Model) =
    match msg with
    | Loaded (subs, articles) -> Model.Loaded { Subscriptions = subs; Articles = articles; SubInput = "" }, Cmd.none
    | LoadingError _ -> model, (GuiCmd.alert "Datta loading error hurr durr")
    | DeleteSubscription id -> 
        let cmd = GuiCmd.deleteSubscription id
        Loading, cmd
    | SubscriptionDeleted ->
        init()
    | SubscriptionDeleteFailed _ -> model, (GuiCmd.alert "Subscription delete failed")
    | Poll -> model, GuiCmd.poll
    | Reload -> Loading, GuiCmd.loadSubsAndArticles
    | OnChangeSub str -> 
        match model with 
        | Model.Loaded l ->
            Model.Loaded ({ l with SubInput = str }), Cmd.none
        | _ -> model, Cmd.none
    | AddSubscription ->
        match model with
        | Model.Loaded m ->
            let cmd = GuiCmd.subscribeToFeed m.SubInput
            model, cmd
        | _ -> model, Cmd.none
    | SubscriptionSucceeded ->
        model, GuiCmd.alert "Succeeded"
    | Msg.SubscriptionFailed exn ->
        model, GuiCmd.alert (sprintf "Failed %A" exn.Message)

let renderLoading () = 
    div [] [ str "Loading..."  ]

let renderLoaded (model: (Dto.SubscriptionDto list * Dto.ArticleDto list * string)) =
    let subscriptions, articles, inp = model
    
    let renderFeed (subscription: SubscriptionDto) = 
        div [ className "feed-container" ] [
            span [ className "has-text-weight-semibold feed-header2 truncate" ] [ str subscription.Url ]
            span [ className "has-text-weight-semibold" ] [ str "???" ]
        ]

    let renderFeeds feeds = List.map renderFeed feeds

    let renderArticle (article: ArticleDto) =
        div [] [ str article.Title ]

    let renderArticles = List.map renderArticle

    div [ className "root-grid" ] [
        div [ className "head has-background-info" ] [
            h4 [ className "title is-4 navbar-item has-text-white" ] [ str "Farss" ]
        ]
        div [ className "toolbar has-background-info" ] [
            input [ value inp; onInput Msg.OnChangeSub ]
            input [ _type "button"; value "Add"; onClick Msg.AddSubscription ]
        ]
        div [ className "left-pane" ] [
            div [] [
                h4 [ className "has-text-weight-semibold feed-header" ] [ str "Feeds" ]
            ]
            
            div [ className "feeds-container" ] (renderFeeds subscriptions)
        ]
        div [ className "main" ] [
            str "Articles n stufffsnsnsn"
            fragment () (renderArticles articles)
        ]
    ]

//module Nav =    
//    open Fulma
//    open Fable.Helpers.React

//    module P = Fable.Helpers.React.Props
//    module R = Fable.Helpers.React

//    let nav (isOpen: bool) dispatch =
//        let brandLogo _ =
//            Navbar.Item.a [ Navbar.Item.Props [ (P.Href "/") ]; Navbar.Item.CustomClass "has-text-white has-text-weight-bold" ] [
//                unbox "FARSS"
//            ]

//        let burger _ =
//            let classes = R.classList [
//                "is-active", isOpen
//                "navbar-burger", true
//                "has-text-white", true
//            ]

//            let click = P.OnClick (fun _ -> dispatch ())

//            R.a [ classes; click ] [
//                R.span [] [] 
//                R.span [] [] 
//                R.span [] [] 
//            ]

//        Navbar.navbar [ Navbar.CustomClass "has-background-primary" ] [
//            Navbar.Brand.div [] [
//                brandLogo ()
//                burger ()
//            ]

//            Navbar.menu [ 
//                if isOpen then
//                    yield Navbar.Menu.CustomClass "is-active"
//            ] [
//                Navbar.Start.div [] [
//                    Navbar.Item.a [ 
//                        if not isOpen then
//                            yield Navbar.Item.CustomClass "has-text-white"
//                    ] [ unbox "Home" ]
//                ]
//            ]
//        ]
    
//    let NavComp _ =
//        let init _ = false
//        let update () state = not state
//        let view (m: ReactiveComponents.Model<_, bool>) d = nav m.state d
//        Fable.Helpers.React.reactiveCom init update view "" () []


let view (model:Model) dispatch =
    //Nav.NavComp ()
    match model with
    | Loading -> Html.run (renderLoading ()) dispatch
    | Model.Loaded { Subscriptions = subs; Articles = articles; SubInput = s } -> Html.run (renderLoaded (subs, articles, s)) dispatch

//TODO: add error handler
Program.mkProgram init update view
    |> Program.withReact ReactSettings.appRootId
    |> Program.withConsoleTrace
    |> Program.run
