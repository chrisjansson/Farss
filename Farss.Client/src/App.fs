module App

Fable.Core.JsInterop.importSideEffects "dialog-polyfill/dist/dialog-polyfill.css"

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

open Farss.Client
open Feliz

type ViewModel<'T> =
    | Loading
    | Loaded of 'T
    
type SideMenuState =
    {
        Feeds: Dto.SubscriptionDto list
    }

let sideMenu =
    React.functionComponent(
        fun () ->
            
            let state, setState = React.useState(ViewModel<SideMenuState>.Loading)
            
            React.useEffectOnce(
                fun () ->
                    ApiClient.getSubscriptions ()
                    |> PromiseResult.resultEnd (fun r -> setState(Loaded { Feeds = r })) (fun _ -> ())
                    |> ignore
            )
            
            Html.div [
                Html.div [ prop.text "Feeds" ]
                
                match state with
                | Loading -> Html.text "Loading"
                | Loaded m ->
                    Html.ul [
                        for f in m.Feeds do
                            Html.li [ prop.text f.Title ]
                    ]
            ]
        )
    
type ArticlesState =
    {
        Articles: Dto.ArticleDto list
    }
    
let sanitizeArticleContent (article: ArticleDto) =
    let getTextContent (html: string) =
        let sanitized = DOMPurify.sanitize html
        let el = Browser.Dom.document.createElement("div")
        el.innerHTML <- sanitized
        el.innerText
        
    { article with Summary = Option.map getTextContent article.Summary; Content = DOMPurify.sanitize article.Content }
    
let articles =
    React.functionComponent(
        fun () ->
            
            let state, setState = React.useState(ViewModel<ArticlesState>.Loading)
            
            React.useEffectOnce(
                fun () ->
                    ApiClient.getArticles ()
                    |> PromiseResult.map (List.map sanitizeArticleContent)
                    |> PromiseResult.resultEnd (fun r -> setState(Loaded { Articles = r })) (fun _ -> ())
                    |> ignore
            )
            Html.div [
                match state with
                | Loading -> Html.text "Loading"
                | Loaded m ->
                    let renderArticle (article: Dto.ArticleDto) =
                        React.fragment [
                            Html.div [
                                prop.className "feed-icon"
                                prop.text "feed text"
                            ]
                            Html.div [
                                prop.className "feed-title"
                                prop.text "Feed title"
                            ]
                            Html.div [
                                prop.className "article-date"
                                prop.text (sprintf "%A" article.PublishedAt)
                            ]
                            Html.div [
                                prop.className "article-tools"
                                prop.text "tools"
                            ]
                            Html.div [
                                prop.className "article-title"
                                prop.text article.Title

                            ]
                            Html.div [
                                prop.className "article-content"
                                prop.text (article.Summary |> Option.defaultValue "")
                            ]
                        ]
                    
                    Html.div [
                        prop.classes [ "articles-container" ]
                        prop.children [
                            for a in m.Articles do
                                renderArticle a
                        ]
                    ]
            ]
    )
    
type MenuState =
    {
        IsOpen: bool
    }
    
let menu =
    React.functionComponent(
        fun () ->
            let state, setState = React.useState({ IsOpen = false })
            
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
                        prop.onClick (fun _ -> setState { state with IsOpen = true })
                    ]
                    if state.IsOpen then
                        AddFeedModal.addFeedDialog { OnClose = fun () -> setState { state with IsOpen = false } }
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
                    Html.div [
                        prop.classes [ "Main" ]
                        prop.children [
                            articles ()
                        ]
                    ]
                ]
            ]
        )

let documentRoot = Browser.Dom.document.getElementById ReactSettings.appRootId

ReactDOM.render (main, documentRoot)