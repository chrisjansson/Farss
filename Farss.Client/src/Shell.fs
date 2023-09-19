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
        fss
            [ Display.grid
              Custom "grid-template-columns" "auto 1fr"
              Custom "grid-template-rows" "75px 1fr"
              GridGap.value (px 0, px 0)
              GridTemplateAreas.value [ [ "Logo"; "Menu" ]; [ "Side-menu"; "Main" ] ]
              Height.value (pct 100) ]

    let headerColor = BackgroundColor.lightBlue
    
    let Logo = fss [ GridArea.value "Logo"; headerColor ]

    let Menu = fss [ GridArea.value "Menu"; headerColor ]

    let SideMenu =
        fss
            [ GridArea.value "Side-menu"
              FontSize.value (px 16)
              Width.value (px 280)
              Height.value (pct 100)
              Custom "border-right" "1px solid lightgray" ]

    let Main = fss [ GridArea.value "Main"; OverflowY.auto ]
    
[<ReactComponent>]
let Main () =
    Html.div
        [ prop.className [ Style.GridContainer ]
          prop.children
              [ Html.div
                    [ prop.classes [ Style.Logo ]
                      prop.children
                          [ Html.div
                                [ prop.style
                                      [ style.display.flex
                                        style.flexDirection.column
                                        style.justifyContent.center
                                        style.height (length.percent 100)
                                        style.margin (0, 10) ]
                                  prop.children [ Html.span [ prop.text "Farss" ] ] ] ] ]
                Html.div [ prop.classes [ Style.Menu ]; prop.children [ Toolbar.Menu () ] ]
                Html.div [ prop.classes [ Style.SideMenu ]; prop.children [ SideMenu.SideMenu () ] ]
                Html.div [ prop.classes [ Style.Main ]; prop.children [ ArticleList.Articles() ] ] ] ]
