module Toolbar

open Farss.Client
open Feliz


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
