module App

open Dto
//
//module Msg =
//    let map modelMap msgMap update msg model =
//        let m, cmd = update msg model
//
//        let m = modelMap m
//        let cmd = Cmd.map msgMap cmd
//        m,cmd
//
//let init(): Model * Cmd<Msg> = 
//    let cmd = GuiCmd.loadSubsAndArticles
//    Loading, cmd
//    
//let mapArticles articles =
//    articles |> List.map (fun (a: Dto.ArticleDto) -> { Dto = a; IsExpanded = false })
//
//let update (msg:Msg) (model:Model) =
//    match msg with
//    | Loaded (subs, articles) -> Model.Loaded { Subscriptions = subs; Articles = mapArticles articles; AddSubscriptionModel = None }, Cmd.none
//    | LoadingError _ -> model, (GuiCmd.alert "Datta loading error hurr durr")
//    | DeleteSubscription id -> 
//        let cmd = GuiCmd.deleteSubscription id
//        Loading, cmd
//    | SubscriptionDeleted ->
//        init()
//    | SubscriptionDeleteFailed _ -> model, (GuiCmd.alert "Subscription delete failed")
//    | Poll -> model, GuiCmd.poll
//    | Reload -> Loading, GuiCmd.loadSubsAndArticles
//    | OpenAddSubscription ->
//        match model with
//        | Model.Loaded m ->
//            let m = { m with AddSubscriptionModel = AddSubscriptionModal.init () }
//            Model.Loaded m, Cmd.none
//        | _ -> model, Cmd.none
//    | AddSubscriptionMsg msg ->
//        match model with 
//        | Model.Loaded m ->
//            match msg with
//            | AddSubscriptionModel.Close ->
//                init()
//                //Model.Loaded { m with AddSubscriptionModel = None }, Cmd.none
//            | _ ->
//                Msg.map (fun cm -> Model.Loaded { m with AddSubscriptionModel = cm }) AddSubscriptionMsg AddSubscriptionModal.udpate msg m.AddSubscriptionModel
//        | _ -> model, Cmd.none
//    | ToggleExpanded article ->
//        match model with
//        | Model.Loaded m ->
//            let articles =
//                List.map (fun a ->
//                if a = article then
//                    { a with IsExpanded = not a.IsExpanded }
//                else
//                    { a with IsExpanded = false }) m.Articles
//            Model.Loaded { m with Articles = articles }, Cmd.none
//        | _ -> model, Cmd.none
//            
//
//let renderLoading () = 
//    div [] [ str "Loading..."  ]
//
//let renderLoaded (model: Loaded) =
//    let { Subscriptions = subscriptions; Articles = articles } = model
//    let articles = articles |>  List.sortByDescending (fun a -> a.Dto.PublishedAt) 
//    
//    let renderFeed (subscription: SubscriptionDto) = 
//        div [ className "feed-container" ] [
//            span [ className "feed-header2 truncate" ] [ str subscription.Title ]
//            span [ ] [ 
//                Html.button [ onClick (Msg.DeleteSubscription subscription.Id) ] [ str "x" ]
//            ]
//        ]
//
//    let renderFeeds feeds = List.map renderFeed feeds
//
//    let renderArticle (article: Article) =
//        
//        if article.Dto.IsRead then
//            let sign = if article.IsExpanded then "-" else "+"
//            fragment () [
//                div [ className "expander" ] [ str sign ]
//                div [ className "title title-container" ] [
//                    str article.Dto.Title
//                ]
//                div [ className "date" ] [
//                    str (article.Dto.PublishedAt.ToString("yyyy-MM-dd"))
//                ]
//            ]
//            
//        else
//            let sign = if article.IsExpanded then "-" else "+"
//            fragment () [
//                yield div [ className "expander" ] [ str sign ]
//                yield div [ className "has-text-weight-semibold title-container"; onClick (ToggleExpanded article) ] [ 
//                    a [ href article.Dto.Link ] [ str article.Dto.Title ]
//                ]
//                yield div [ className "date" ] [
//
//                    str (article.Dto.PublishedAt.ToString("yyyy-MM-dd"))
//                ]
//                
//                if article.IsExpanded then
//                    yield div [ className "article-container" ] [
//                        div [ dangerouslySetInnerHTML article.Dto.Content ] []
//                    ]
//            ]
//            
//        
//    let renderArticles = List.map renderArticle
//
//    fragment () [
//        yield div [ className "root-grid" ] [
//            div [ className "head has-background-info" ] [
//                h4 [ className "title is-4 navbar-item has-text-white" ] [ str "Farss" ]
//            ]
//            div [ className "toolbar" ] [
//                input [ _type "button"; value "Add"; onClick Msg.OpenAddSubscription ]
//                input [ _type "button"; value "Poll"; onClick Msg.Poll ]
//            ]
//            div [ className "left-pane" ] [
//                div [] [
//                    h4 [ className "has-text-weight-semibold feed-header" ] [ 
//                        str "Feeds" 
//                    ]
//                ]
//                
//                div [ className "feeds-container" ] (renderFeeds subscriptions)
//            ]
//            div [ className "main" ] [
//                str "Articles n stufffsnsnsn"
//                div [ className "articles-grid" ] [
//                    fragment () (renderArticles articles)
//                ]
//            ]
//        ]
//
//        match model.AddSubscriptionModel with
//        | Some m -> yield Html.map Msg.AddSubscriptionMsg (AddSubscriptionModal.view m)
//        | None-> ()
//    ]
//
//let view (model:Model) dispatch =
//    match model with
//    | Loading -> Html.run (renderLoading ()) dispatch
//    | Model.Loaded m-> Html.run (renderLoaded m) dispatch
//
////TODO: add error handler
//Program.mkProgram init update view
//    |> Program.withReact ReactSettings.appRootId
//    |> Program.withConsoleTrace
//    |> Program.run

open Feliz

let sideMenu =
    React.functionComponent(
        fun () ->
            Html.div [
                Html.div [ prop.text "Feeds" ]
                Html.ul [
                    Html.li [ prop.text "First" ]
                    Html.li [ prop.text "Second" ]
                    Html.li [ prop.text "Third" ]
                ]
            ]
        )
    
let menu =
    React.functionComponent(
        fun () ->
            Html.div [
                prop.style [
                    style.display.flex
                    style.flexDirection.row
                    style.justifyContent.flexEnd
                    style.alignItems.center
                    style.height(length.percent(100))
                    style.margin(0, 10)
                ]
                prop.children [
                    Html.button [
                        prop.type' "button"
                        prop.text "Add"
                    ]
                ]
                
            ]
        )

let main =
    React.functionComponent(
        fun () ->
            Html.div [
                prop.classes [
                    "grid-container"
                ]
                prop.children [
                    Html.div [
                        prop.classes [ "Logo" ]
                        prop.children [
                            Html.div [
                                prop.style [
                                    style.display.flex
                                    style.flexDirection.column
                                    style.justifyContent.center
                                    style.height(length.percent(100))
                                    style.margin(0, 10)
                                ]
                                prop.children [
                                    Html.span [
                                        prop.text "Farss"
                                    ]
                                ]
                            ]
                        ]
                    ]
                    Html.div [
                        prop.classes [ "Menu" ]
                        prop.children [
                            menu ()
                        ]
                    ]
                    Html.div [
                        prop.classes [ "Side-menu" ]
                        prop.children [
                            sideMenu ()
                        ]
                    ]
                    Html.div [ prop.classes [ "Main" ] ]
                ]
            ]
        )

let documentRoot = Browser.Dom.document.getElementById ReactSettings.appRootId

ReactDOM.render (main, documentRoot)