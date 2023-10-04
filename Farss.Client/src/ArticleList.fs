module ArticleList

open System
open Dto
open Feliz

open Fss
open Fss.Types

module private Style =
    let ArticlesContainerWrapper = fss [
        Display.flex
        FlexDirection.column
        OverflowY.auto
    ]

    let ArticlesContainer =
        fss
            [ Display.grid
              Custom "grid-template-columns" "auto 1fr auto"
              Custom "grid-auto-rows" "1fr" ]

    let Article isSelected =
        fss
            [ Display.grid
              GridColumn.value "1 / 4"
              GridTemplateColumns.subgrid
              Cursor.pointer
              BorderBottomColor.hex "#ececec"
              BorderBottomWidth.value (px 1)
              BorderBottomStyle.solid
              Hover [ BackgroundColor.hex "#ececec" ]
              BorderWidth.value (px 1)
              BorderStyle.solid
              BorderColor.transparent
              if isSelected then
                  BackgroundColor.hex "#EEF4FC"
                  BorderColor.blue
              PaddingRight.value (px 4)
              PaddingBottom.value (px 4)
              GridRowEnd.span 3 ]

    let iconSize = 28

    let ArticleIcon =
        fss
            [ Display.flex
              JustifyContent.center
              AlignItems.center
              PaddingTop.value (px 4)
              PaddingBottom.value (px 4) ]

    let HeaderCommon =
        [ FontSize.value (em 0.85)
          Color.hex "555"
          Display.flex
          FlexDirection.column
          JustifyContent.center ]

    let FeedTitle = fss [ yield! HeaderCommon ]

    let ArticleDate = fss [ yield! HeaderCommon ]

    let ArticleTools = fss [ Width.value (px 40); GridRow.value "span 2" ]

    let ArticleTitle isRead =
        fss
            [ GridColumnStart.value 2
              GridColumnEnd.value 4
              PaddingTop.value (px 5)
              PaddingBottom.value (px 3)

              if not isRead then
                  FontWeight.bold ]

    let ArticleSummaryContainer = fss [ GridColumnStart.value 2; GridColumnEnd.value 4 ]

    let ArticleSummary =
        fss
            [ FontFamily.value "'Merriweather', serif;"
              FontSize.value (em 0.7)
              Color.hex "555"
              Overflow.hidden
              Custom "display" "-webkit-box"
              Custom "-webkit-line-clamp" "3"
              Custom "-webkit-box-orient" "vertical" ]

    let ReadingPane = fss [ OverflowY.auto; Padding.value (px 10) ]

    let ReadingSeparator =
        fss [ Width.value (px 1); FlexShrink.value 0; BackgroundColor.hex "ececec" ]


[<ReactComponent>]
let ArticleRow (feed: SubscriptionDto, article: ArticleDto, selectArticle, isSelected: bool) =
    Html.div
        [ prop.className [ Style.Article isSelected ]
          prop.onClick (fun _ -> selectArticle article)
          prop.children
              [ Html.div
                    [ prop.className Style.ArticleIcon
                      prop.children [ FeedIcon.FeedIcon(feed.Icon, Style.iconSize) ] ]
                Html.div [ prop.className Style.FeedTitle; prop.text feed.Title ]
                Html.div
                    [ prop.className Style.ArticleDate
                      prop.text (article.PublishedAt.ToString("yyyy-MM-dd hh:mm")) ]
                Html.div [ prop.className Style.ArticleTools ]
                Html.div [ prop.classes [ Style.ArticleTitle article.IsRead ]; prop.text article.Title ]
                Html.div
                    [ prop.className [ Style.ArticleSummaryContainer ]
                      prop.children
                          [ Html.div
                                [ prop.className [ Style.ArticleSummary ]
                                  prop.text (article.Summary |> Option.defaultValue "") ] ] ]

                ] ]

type ViewModel<'T> =
    | Loading
    | Loaded of 'T

type ArticlesState =
    { Articles: ArticleDto list
      Feeds: SubscriptionDto list }

let private sanitizeArticleContent (article: ArticleDto) =
    { article with
        Summary =
            [ Option.map SanitizeHtml.getSanitizedInnerText article.Summary
              Some(SanitizeHtml.getSanitizedInnerText article.Content) ]
            |> List.tryPick id
        Content = SanitizeHtml.sanitizeHtml article.Content }

[<ReactComponent>]
let rec Articles
    (props:
        {| SelectedFeed: Guid option
           SelectedArticle: Guid option |})
    =
    let state, setState = React.useState ViewModel<ArticlesState>.Loading

    let fetchData () =
        promise {
            let articlesP = ApiClient.getArticles 50
            let feedsP = ApiClient.getSubscriptions ()

            let! articles = articlesP
            let! feeds = feedsP

            return
                match articles, feeds with
                | Ok r1, Ok r2 -> Ok(r1, r2)
                | Error e, Ok _ -> Error [ e ]
                | Ok _, Error e -> Error [ e ]
                | Error e1, Error e2 -> Error [ e1; e2 ]
        }

    React.useEffectOnce (fun () ->
        fetchData ()
        |> PromiseResult.map (fun (a, f) -> (List.map sanitizeArticleContent a, f))
        |> PromiseResult.resultEnd
            (fun (r, f) ->
                setState (
                    Loaded
                        { Articles = r
                          Feeds = f |> List.sortBy (fun x -> x.Title) }
                ))
            (fun _ -> ())
        |> ignore)

    Html.div
        [ prop.className "main"
          prop.children
              [ match state with
                | Loading -> Html.text "Loading"
                | Loaded m ->
                    let renderArticle (article: ArticleDto) =
                        let feed = m.Feeds |> List.find (fun x -> x.Id = article.FeedId)

                        let selectArticle (article: ArticleDto) =

                            match props.SelectedFeed with
                            | Some feedId ->
                                Feliz.Router.Router.navigatePath (
                                    "feeds",
                                    feedId.ToString(),
                                    "articles",
                                    article.Id.ToString()
                                )
                            | None ->
                                Feliz.Router.Router.navigatePath ("feeds", "all", "articles", article.Id.ToString())

                        React.keyedFragment (
                            article.Title,
                            [ ArticleRow(feed, article, selectArticle, props.SelectedArticle = Some article.Id) ]
                        )

                    Html.div
                        [

                          prop.classes [ Style.ArticlesContainerWrapper ]
                          prop.children
                              [ Html.div
                                    [ prop.classes [ Style.ArticlesContainer ]
                                      prop.children
                                          [ for a in
                                                m.Articles
                                                |> List.filter (fun a ->
                                                    if props.SelectedFeed.IsNone then
                                                        true
                                                    else
                                                        (Some a.FeedId) = props.SelectedFeed)
                                                |> List.sortByDescending (fun x -> x.PublishedAt) do
                                                renderArticle a ] ] ] ]

                    Html.div [ prop.className Style.ReadingSeparator ]

                    Html.div[prop.className Style.ReadingPane

                             prop.children
                                 [ match props.SelectedArticle with
                                   | Some aId ->

                                       let a = m.Articles |> List.tryFind (fun a -> a.Id = aId)

                                       match a with
                                       | Some a -> Article.Article a
                                       | _ -> ()
                                   | _ -> () ]] ] ]
