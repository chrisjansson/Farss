module App

open Elmish
open Elmish.React
//TODO: Only shadow HMR in development. Requires change in webpack.config as well. See Fable.Elmish.HMR
open Elmish.HMR
open Dto
open Html
open Model

module Msg =
    let map modelMap msgMap update msg model =
        let m, cmd = update msg model

        let m = modelMap m
        let cmd = Cmd.map msgMap cmd
        m,cmd

let init(): Model * Cmd<Msg> = 
    let cmd = GuiCmd.loadSubsAndArticles
    Loading, cmd

let update (msg:Msg) (model:Model) =
    match msg with
    | Loaded (subs, articles) -> Model.Loaded { Subscriptions = subs; Articles = articles; AddSubscriptionModel = None }, Cmd.none
    | LoadingError _ -> model, (GuiCmd.alert "Datta loading error hurr durr")
    | DeleteSubscription id -> 
        let cmd = GuiCmd.deleteSubscription id
        Loading, cmd
    | SubscriptionDeleted ->
        init()
    | SubscriptionDeleteFailed _ -> model, (GuiCmd.alert "Subscription delete failed")
    | Poll -> model, GuiCmd.poll
    | Reload -> Loading, GuiCmd.loadSubsAndArticles
    | OpenAddSubscription ->
        match model with
        | Model.Loaded m ->
            let m = { m with AddSubscriptionModel = AddSubscriptionModal.init () }
            Model.Loaded m, Cmd.none
        | _ -> model, Cmd.none
    | AddSubscriptionMsg msg ->
        match model with 
        | Model.Loaded m ->
            match msg with
            | AddSubscriptionModel.Close ->
                init()
                //Model.Loaded { m with AddSubscriptionModel = None }, Cmd.none
            | _ ->
                Msg.map (fun cm -> Model.Loaded { m with AddSubscriptionModel = cm }) AddSubscriptionMsg AddSubscriptionModal.udpate msg m.AddSubscriptionModel
        | _ -> model, Cmd.none

let renderLoading () = 
    div [] [ str "Loading..."  ]

let renderLoaded (model: Loaded) =
    let { Subscriptions = subscriptions; Articles = articles } = model
    
    let renderFeed (subscription: SubscriptionDto) = 
        div [ className "feed-container" ] [
            span [ className "has-text-weight-semibold feed-header2 truncate" ] [ str subscription.Title ]
            span [ className "has-text-weight-semibold" ] [ 
                Html.button [ onClick (Msg.DeleteSubscription subscription.Id) ] [ str "x" ]
            ]
        ]

    let renderFeeds feeds = List.map renderFeed feeds

    let renderArticle (article: ArticleDto) =
        if article.IsRead then
            div [] [ str article.Title ]
        else    
            div [ className "has-text-weight-semibold" ] [ 
                a [ href article.Link ] [ str article.Title ]
                str (sprintf "%A" article.PublishedAt)
            ]
        
    let renderArticles = List.map renderArticle

    fragment () [
        yield div [ className "root-grid" ] [
            div [ className "head has-background-info" ] [
                h4 [ className "title is-4 navbar-item has-text-white" ] [ str "Farss" ]
            ]
            div [ className "toolbar has-background-info" ] [
            ]
            div [ className "left-pane" ] [
                div [] [
                    h4 [ className "has-text-weight-semibold feed-header" ] [ 
                        str "Feeds" 
                        input [ _type "button"; value "Add"; onClick Msg.OpenAddSubscription ]
                    ]
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
    match model with
    | Loading -> Html.run (renderLoading ()) dispatch
    | Model.Loaded m-> Html.run (renderLoaded m) dispatch

//TODO: add error handler
Program.mkProgram init update view
    |> Program.withReact ReactSettings.appRootId
    |> Program.withConsoleTrace
    |> Program.run
