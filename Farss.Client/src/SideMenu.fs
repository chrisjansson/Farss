module SideMenu

open Dto
open Feliz


type SideMenuState = { Feeds: SubscriptionDto list }

type ViewModel<'T> =
    | Loading
    | Loaded of 'T


[<ReactComponent>]
let SideMenu () =
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
                          [
                            Html.div [ prop.className "side-menu-item side-menu-header"; prop.text "Feeds" ]
                            Html.div [ prop.className "side-menu-item side-menu-header"; prop.text "All" ]

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

          ]
