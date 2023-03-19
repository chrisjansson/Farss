module ArticleList

open Dto
open Fable.Core
open Feliz

open Fss
open Fss.Types
open Fss.Fable

module Style =
    let ArticlesContainer = fss [
            Display.grid
            Custom "grid-template-columns" "auto 1fr auto"
            Custom "grid-auto-rows" "1fr auto auto"
            MaxWidth.value (px 400)
            OverflowY.auto
        ]

[<ReactComponent>]
let ArticleRow (feed: SubscriptionDto, article: ArticleDto, selectArticle, isSelected: bool) =
    Html.div [
        prop.className [
            "article"
            if isSelected then
                "article-selected"
        ]
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
                prop.className [
                    "article-content"
                    "summary"
                ]

                prop.text (article.Summary |> Option.defaultValue "")
            ]
        ]
    ]

type ViewModel<'T> =
    | Loading
    | Loaded of 'T
    
type ArticlesState =
    {
        Articles: ArticleDto list
        Feeds: SubscriptionDto list
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
let rec Articles () =
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
                        ArticleRow(feed, article, selectArticle, m.SelectedArticle = Some article)
                    ])
                
                Html.div [
                    prop.classes [ Style.ArticlesContainer ]
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
    
and [<ReactComponent>] Article (article: ArticleDto): Fable.React.ReactElement =
    
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
