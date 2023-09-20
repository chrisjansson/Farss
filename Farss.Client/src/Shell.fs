module Shell

open Feliz

open Dto
open Fss.Types

type private ViewModel<'T> =
    | Loading
    | Loaded of 'T

type private SideMenuState = { Feeds: SubscriptionDto list }


open Fss

module private Style =
    let GridContainer =
        fss [
            Display.grid
            Custom "grid-template-columns" "auto 1fr"
            Custom "grid-template-rows" "75px 1fr"
            GridGap.value (px 0, px 0)
            GridTemplateAreas.value [ [ "Logo"; "Menu" ]; [ "Side-menu"; "Main" ] ]
            Height.value (pct 100)
        ]

    let headerColor = BackgroundColor.lightBlue

    let Logo = fss [ GridArea.value "Logo"; headerColor ]

    let Menu = fss [ GridArea.value "Menu"; headerColor ]

    let SideMenu =
        fss [
            GridArea.value "Side-menu"
            FontSize.value (px 16)
            Width.value (px 280)
            Height.value (pct 100)
            Custom "border-right" "1px solid lightgray"
        ]

    let Main = fss [ GridArea.value "Main"; OverflowY.auto ]

open Feliz.Router

[<ReactComponent>]
let Router2 () =
    let (currentUrl, updateUrl) = React.useState (Router.currentUrl ())

    React.router [
        router.onUrlChanged updateUrl
        router.children [
            match currentUrl with
            | [] -> Html.h1 "Index"
            | [ "users" ] -> Html.h1 "Users page"
            | [ "users"; Route.Int userId ] -> Html.h1 (sprintf "User ID %d" userId)
            | _ -> Html.h1 "Not found"
        ]
    ]

let private MemoArticles = React.memo ArticleList.Articles
let private MemoMenu = React.memo Toolbar.Menu

[<ReactComponent>]
let Main () =
    let (currentUrl, updateUrl) = React.useState (Router.currentUrl ())

    React.router [
        router.onUrlChanged updateUrl
        router.children [


            let selectedFeed =
                match currentUrl with
                | [] -> None
                | [ "feeds" ] -> None
                | [ "feeds"; Route.Guid feedId ] -> Some feedId
                | _ -> None

            Html.div [
                prop.className [ Style.GridContainer ]
                prop.children [
                    Html.div [
                        prop.classes [ Style.Logo ]
                        prop.children [
                            Html.div [
                                prop.style [
                                    style.display.flex
                                    style.flexDirection.column
                                    style.justifyContent.center
                                    style.height (length.percent 100)
                                    style.margin (0, 10)
                                ]
                                prop.children [ Html.span [ prop.text "Farss" ] ]
                            ]
                        ]
                    ]
                    Html.div [ prop.classes [ Style.Menu ]; prop.children [ MemoMenu() ] ]
                    Html.div [
                        prop.classes [ Style.SideMenu ]
                        prop.children [ SideMenu.SideMenu selectedFeed ]
                    ]
                    Html.div [ prop.classes [ Style.Main ]; prop.children [ MemoArticles() ] ]
                ]
            ]
        ]
    ]
