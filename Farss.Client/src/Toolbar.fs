module Toolbar

open Farss.Client
open Feliz
open Fable.Core.JsInterop
open Browser.Types

type private MenuState = { IsOpen: bool }

[<ReactComponent>]
let rec Menu () =
    let state, setState = React.useState ({ IsOpen = false })
    let isInfoOpen, setIsInfoOpen = React.useState ({ IsOpen = false })

    let poll = React.useCallback (fun () -> ApiClient.poll ())

    Html.div [
        prop.style [
            style.display.flex
            style.flexDirection.row
            style.justifyContent.flexEnd
            style.alignItems.center
            style.height (length.percent (100))
            style.margin (0, 10)
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

            Html.button [
                prop.type' "button"
                prop.text "Info"
                prop.onClick (fun _ -> setIsInfoOpen { isInfoOpen with IsOpen = true })
            ]

            if state.IsOpen then
                AddFeedModal.AddFeedDialog(fun () -> setState { state with IsOpen = false })

            if isInfoOpen.IsOpen then
                Info(fun () -> setIsInfoOpen { isInfoOpen with IsOpen = false })
        ]
    ]

and [<ReactComponent>] Info (onClose: unit -> unit) : ReactElement =
    let onRef = React.useRef None

    let state, setState = React.useState<Dto.StartupInformationDto option> (None)

    React.useEffectOnce (fun () ->
        ApiClient.echo ()
            |> Promise.map (fun r -> match r with | Ok r -> r | x -> failwithf "%A" x)
            |> Promise.tap (fun s -> setState (Some s))
            |> ignore
    )

    React.useEffectOnce (fun () ->
        match onRef.current with
        | Some e -> e?showModal ()
        | _ -> ()

        ())

    let close (returnValue: string) =
        match onRef.current with
        | Some e -> e?close (returnValue)
        | None -> ()

    let onClose (_: Event) = onClose ()

    let onCancel _ =
        match onRef.current with
        | Some e -> e?returnValue <- "cancel"
        | None -> ()

    Portal.portal [
        Html.dialog [
            prop.ref onRef
            prop.custom ("onClose", onClose)
            prop.custom ("onCancel", onCancel)
            prop.children [
                match state with
                | Some s -> Html.text (sprintf "%A" s)
                | None -> ()
            ]
        ]
    ]
