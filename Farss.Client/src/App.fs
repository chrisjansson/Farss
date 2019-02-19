module App

open Elmish
open Elmish.React
//TODO: Only shadow HMR in development. Requires change in webpack.config as well. See Fable.Elmish.HMR
open Elmish.HMR
open Dto
open Html
open Model

let init(): Model * Cmd<Msg> = 
    let cmd = GuiCmd.loadSubsAndArticles
    Loading, cmd

let update (msg:Msg) (model:Model) =
    match msg with
    | Loaded (subs, articles) -> Model.Loaded { Subscriptions = subs; Articles = articles; SubInput = ""; AddSubscriptionModel = None }, Cmd.none
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
            let m = { m with AddSubscriptionModel = AddSubscriptionModal.init () }
            Model.Loaded m, Cmd.none
        | _ -> model, Cmd.none
    | SubscriptionSucceeded ->
        model, GuiCmd.alert "Succeeded"
    | Msg.SubscriptionFailed exn ->
        model, GuiCmd.alert (sprintf "Failed %A" exn.Message)
    | Msg.AddSubscriptionMsg msg ->
        match model with 
        | Model.Loaded m ->
            match msg with
            | AddSubscriptionModel.Close ->
                Model.Loaded { m with AddSubscriptionModel = None }, Cmd.none
            | _ ->
                match m.AddSubscriptionModel with
                | Some asm -> 
                    //TODO: Chilld.map
                    let asm, asmCmd = AddSubscriptionModal.udpate msg asm
                    let m = { m with AddSubscriptionModel = Some asm }
                    let cmd = Cmd.map Msg.AddSubscriptionMsg asmCmd
                    Model.Loaded m, cmd
                | None ->
                    model, Cmd.none
        | _ -> model, Cmd.none
    

let renderLoading () = 
    div [] [ str "Loading..."  ]

let renderLoaded (model: Loaded) =
    let { Subscriptions = subscriptions; Articles = articles; SubInput = inp } = model
    
    let renderFeed (subscription: SubscriptionDto) = 
        div [ className "feed-container" ] [
            span [ className "has-text-weight-semibold feed-header2 truncate" ] [ str subscription.Url ]
            span [ className "has-text-weight-semibold" ] [ str "???" ]
        ]

    let renderFeeds feeds = List.map renderFeed feeds

    let renderArticle (article: ArticleDto) =
        div [] [ str article.Title ]

    let renderArticles = List.map renderArticle

    fragment () [
        yield div [ className "root-grid" ] [
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

        match model.AddSubscriptionModel with
        | Some m -> yield Html.map Msg.AddSubscriptionMsg (AddSubscriptionModal.view m)
        | None-> ()
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
    ////Nav.NavComp ()
    match model with
    | Loading -> Html.run (renderLoading ()) dispatch
    //| Model.Loaded m -> Html.run (AddSubscriptionModal.view m.AddSubscriptionModel) (Msg.AddSubscriptionMsg >> dispatch)
    | Model.Loaded m-> Html.run (renderLoaded m) dispatch

//TODO: add error handler
Program.mkProgram init update view
    |> Program.withReact ReactSettings.appRootId
    |> Program.withConsoleTrace
    |> Program.run
