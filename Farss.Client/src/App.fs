module App

open Farss.Client
open Feliz

open Dto
open Fss.Types

type ViewModel<'T> =
    | Loading
    | Loaded of 'T

type SideMenuState = { Feeds: SubscriptionDto list }

let sideMenu =
    React.functionComponent (fun () ->

        let state, setState = React.useState (ViewModel<SideMenuState>.Loading)

        React.useEffectOnce (fun () ->
            ApiClient.getSubscriptions ()
            |> PromiseResult.resultEnd
                (fun r -> setState (Loaded { Feeds = r |> List.sortBy (fun f -> f.Title) }))
                (fun _ -> ())
            |> ignore)

        Html.div
            [ prop.style
                  [ style.display.flex
                    style.flexDirection.column
                    style.height (length.percent (100)) ]

              prop.children
                  [ Html.div
                        [ prop.style [ style.custom ("flex", "1") ]
                          prop.children
                              [ Html.div [ prop.className "side-menu-item side-menu-header"; prop.text "Feeds" ]

                                match state with
                                | Loading -> Html.text "Loading"
                                | Loaded m ->
                                    React.fragment
                                        [ for f in m.Feeds do
                                              Html.div
                                                  [ Html.div
                                                        [ prop.className "side-menu-item"
                                                          prop.children
                                                              [ Html.div
                                                                    [ prop.className "side-menu-item-title"
                                                                      prop.text f.Title ]
                                                                Html.div
                                                                    [ prop.className "side-menu-item-unread"
                                                                      prop.text f.Unread ] ] ] ] ] ] ]

                    Html.div [ Html.text "Add feed goes here" ] ]

              ])


type MenuState = { IsOpen: bool }

[<ReactComponent>]
let Menu () =
    let state, setState = React.useState ({ IsOpen = false })

    let poll = React.useCallback (fun () -> ApiClient.poll ())

    Html.div
        [ prop.style
              [ style.display.flex
                style.flexDirection.row
                style.justifyContent.flexEnd
                style.alignItems.center
                style.height (length.percent (100))
                style.margin (0, 10) ]
          prop.children
              [ Html.button
                    [ prop.type' "button"
                      prop.text "Add"
                      prop.onClick (fun _ -> setState { state with IsOpen = true }) ]

                Html.button
                    [ prop.type' "button"
                      prop.text "Poll"
                      prop.onClick (fun _ -> poll () |> ignore) ]

                if state.IsOpen then
                    AddFeedModal.AddFeedDialog(fun () -> setState { state with IsOpen = false }) ]

          ]

open Fss
open Fss.Types
open Fss.Fable
open Fss.Types.Color

module Style =
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
                                        style.height (length.percent (100))
                                        style.margin (0, 10) ]
                                  prop.children [ Html.span [ prop.text "Farss" ] ] ] ] ]
                Html.div [ prop.classes [ Style.Menu ]; prop.children [ Menu () ] ]
                Html.div [ prop.classes [ Style.SideMenu ]; prop.children [ sideMenu () ] ]
                Html.div [ prop.classes [ Style.Main ]; prop.children [ ArticleList.Articles() ] ] ] ]

let documentRoot = Browser.Dom.document.getElementById ReactSettings.appRootId

let root = ReactDOM.createRoot(documentRoot)
root.render(Main())
