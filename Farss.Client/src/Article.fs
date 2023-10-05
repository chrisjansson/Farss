module Article

open Browser.Types
open Dto
open Feliz

module private Style =
    open Fss

    let ArticleContent =
        fss [
            FontFamily.value "'Merriweather', serif"
            ! (Selector.Tag Types.Html.Img) [ MaxWidth.value (px 700) ]
            ! (Selector.Tag Types.Html.A) [ Color.hex "#2261b7"; Visited [ Color.hex "#597aa8" ] ]
            ! (Selector.Tag Types.Html.Pre) [
                BackgroundColor.hex "#eaeaea"
                FontFamily.value "'Source Code Pro', monospace"
                BorderWidth.value (px 1)
                BorderStyle.solid
                BorderColor.lightGray
                MaxWidth.value (px 700)
                Overflow.auto
                Padding.value (px 5)
            ]
            ! (Selector.Tag Types.Html.Code) [
                BackgroundColor.hex "#eaeaea"
                FontFamily.value "'Source Code Pro', monospace"
                BorderWidth.value (px 1)
                BorderStyle.solid
                BorderColor.lightGray
                MaxWidth.value (px 700)
                Overflow.auto
            ]
            ! (Selector.Tag Types.Html.Figure) [ MarginLeft.value (px 0); MarginRight.value (px 0) ]
        ]

    let ArticleTitle = fss [ FontSize.value (px 27); FontWeight.bold ]

    let ReadingPane = fss [ OverflowY.auto; Padding.value (px 10) ]


[<ReactComponent>]
let Article (article: ArticleDto) : Fable.React.ReactElement =
    let readingPane = React.useRef None

    React.useEffect (
        (fun () ->

            match readingPane.current with
            | Some (element: HTMLElement) ->
                element.scrollTo(0, 0)
            | None -> ()),
        [| article.Id :> obj |]
    )

    Html.div [
        prop.ref readingPane
        prop.className Style.ReadingPane
        prop.children [
            Html.div [ prop.className Style.ArticleTitle; prop.text article.Title ]
            Html.div [ prop.classes [ Style.ArticleContent ]; prop.innerHtml article.Content ]
        ]
    ]
