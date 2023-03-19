module App

open Fable.Core

open Farss.Client
open Feliz

open Dto

type ViewModel<'T> =
    | Loading
    | Loaded of 'T
    
type SideMenuState =
    {
        Feeds: SubscriptionDto list
    }

let sideMenu =
    React.functionComponent(
        fun () ->
            
            let state, setState = React.useState(ViewModel<SideMenuState>.Loading)
            
            React.useEffectOnce(
                fun () ->
                    ApiClient.getSubscriptions ()
                    |> PromiseResult.resultEnd (fun r -> setState(Loaded { Feeds = r |> List.sortBy (fun f -> f.Title) })) (fun _ -> ())
                    |> ignore
            )
            
            Html.div [
                prop.style [
                    style.display.flex
                    style.flexDirection.column
                    style.height(length.percent(100))
                ]

                prop.children [
                    Html.div [
                        prop.style [
                            style.custom ("flex", "1")
                        ]
                        prop.children [
                            Html.div [
                                prop.className "side-menu-item side-menu-header"
                                prop.text "Feeds"
                            ]
                    
                            match state with
                            | Loading -> Html.text "Loading"
                            | Loaded m ->
                                React.fragment [
                                    for f in m.Feeds do
                                        Html.div [
                                            Html.div [
                                                prop.className "side-menu-item"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "side-menu-item-title"
                                                        prop.text f.Title
                                                    ]
                                                    Html.div [
                                                        prop.className "side-menu-item-unread"
                                                        prop.text f.Unread
                                                    ]
                                                ]
                                            ]
                                        ]
                                ]
                            ]
                    ]
                    
                    Html.div [
                        Html.text "Add feed goes here"
                    ]
                ]
                
            ]
        )
    
type ArticlesState =
    {
        Articles: Dto.ArticleDto list
        Feeds: Dto.SubscriptionDto list
        SelectedArticle: ArticleDto option
    }
    
type DOMParser =
    abstract member parseFromString: string -> string -> Browser.Types.Document

[<Emit("new DOMParser()")>]
let createDomParser () : DOMParser = Fable.Core.Util.jsNative
let domParser = createDomParser ()

let sanitizeArticleContent (article: ArticleDto) =
    let getTextContent (html: string) =
        let sanitized = DOMPurify.sanitizeHtml html
        let document = domParser.parseFromString sanitized "text/html"
        document.body.innerText
        
    { article with Summary = Option.map getTextContent article.Summary; Content = DOMPurify.sanitize article.Content }

[<ReactComponent>]
let ArticleRow (feed: SubscriptionDto, article: ArticleDto, selectArticle) =
    Html.div [
        prop.className "article"
        prop.children [
            Html.div [
                prop.className "feed-icon"
            ]
            Html.div [
                prop.className "feed-title"
                prop.text feed.Title
            ]
            Html.div [
                prop.className "article-date"
                prop.text (article.PublishedAt.ToString("yyyy-MM-dd hh:mm"))
            ]
            Html.div [
                prop.className "article-tools"
            ]
            Html.div [
                prop.classes [
                    
                    "article-title"
                    if not article.IsRead then
                        "article-title-unread"
                ]
                prop.onClick (fun _ -> selectArticle article)
                prop.text article.Title
            ]
            Html.div [
                // let isSelected = Some article = m.SelectedArticle
                
                prop.className [
                    "article-content"
                    // if not isSelected then
                    "summary"
                ]
                
                // if isSelected then
                    // prop.innerHtml article.Content
                // else
                prop.text (article.Summary |> Option.defaultValue "")
            ]
        ]
    ]

[<ReactComponent>]
let Article (article: Dto.ArticleDto) =
    
    Html.div [
        Html.div [
            prop.className "selected-article-title"
            prop.text article.Title
        ]
        Html.div [
            prop.className "article-content"
            prop.innerHtml article.Content
        ]
    ]

let articles =
    React.functionComponent(
        fun () ->
            
            let state, setState = React.useState(ViewModel<ArticlesState>.Loading)
            
            let fetchData () = promise {
                let articlesP = ApiClient.getArticles ()
                let feedsP = ApiClient.getSubscriptions ()
                
                let! articles = articlesP
                let! feeds = feedsP
                
                return
                    match articles, feeds with
                    | Ok r1, Ok r2 -> Ok (r1, r2)
                    | Error e, Ok _ -> Error [e]
                    | Ok _, Error e -> Error [e]
                    | Error e1, Error e2 -> Error [e1; e2]
            }
            
            React.useEffectOnce(
                fun () ->
                    fetchData ()
                    |> PromiseResult.map (fun (a, f) -> (List.map sanitizeArticleContent a, f))
                    |> PromiseResult.resultEnd (fun (r, f) -> setState(Loaded { Articles = r; Feeds = f |> List.sortBy (fun x -> x.Title); SelectedArticle = None })) (fun _ -> ())
                    |> ignore
            )
            Html.div [
                prop.className "main"
                prop.children [
                    match state with
                    | Loading -> Html.text "Loading"
                    | Loaded m ->
                        let renderArticle (article: Dto.ArticleDto) =
                            let feed = m.Feeds |> List.find (fun x -> x.Id = article.FeedId)
                            
                            let selectArticle (article: ArticleDto) =
                                setState (Loaded { m with SelectedArticle = Some article })
                            
                            React.keyedFragment (article.Title, [
                                ArticleRow(feed, article, selectArticle)
                            ])
                        
                        Html.div [
                            prop.classes [ "articles-container" ]
                            prop.children [
                                for a in m.Articles |> List.sortByDescending (fun x -> x.PublishedAt) do
                                    renderArticle a
                            ]
                        ]
                        Html.div [
                            prop.className "reading-separator"                        
                        ]
                        Html.div[
                            prop.className "article-reading-pane"
                            
                            prop.children [
                                match m.SelectedArticle with
                                | Some a -> Article a
                                | _ -> ()
                            ]
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
            
            let poll = React.useCallback(fun () -> ApiClient.poll ())
            
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
                    
                    Html.button [
                        prop.type' "button"
                        prop.text "Poll"
                        prop.onClick (fun _ -> poll () |> ignore)
                    ]
                    
                    if state.IsOpen then
                        AddFeedModal.AddFeedDialog (fun () -> setState { state with IsOpen = false })
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
