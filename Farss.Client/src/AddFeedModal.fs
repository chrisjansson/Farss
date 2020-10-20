module Farss.Client.AddFeedModal

open Browser.Types
open Portal
open Feliz
open Fable.Core.JsInterop

type AddFeedDialogProps =
    {
        OnClose: unit -> unit
    }
    
type AddFeedDialogState =
    {
        PreviewUrl: string
        PreviewResult: Result<Dto.PreviewSubscribeToFeedResponseDto, string> option
    }

let addFeedDialog =
    React.functionComponent(
        fun (props: AddFeedDialogProps) ->
            let onRef = React.useRef None
            
            React.useEffectOnce(
                fun () ->
                    match onRef.current with
                    | Some e ->
                        e?showModal()
                    | _ -> ()
                    ()
                )
            
            let state, setState = React.useState({ PreviewUrl= ""; PreviewResult = None })
            
            let close (returnValue: string) =
                match onRef.current with
                | Some e -> e?close(returnValue)
                | None -> ()
            
            let onClose (e: Event) =
                let returnValue = e.currentTarget?returnValue
                printfn "closed with %A" returnValue
                props.OnClose ()
                
            let onCancel _ =
                match onRef.current with
                | Some e -> e?returnValue <- "cancel"
                | None -> ()
            
            let cancel _ =
                close "cancel"
                
            let changeUrl s =
                setState({ state with PreviewUrl = s })
            
            let previewSubscribeToFeed _ =
                ApiClient.previewSubscribeToFeed { Url = state.PreviewUrl }
                |> PromiseResult.resultEnd (fun r -> setState { state with PreviewResult = Some (Ok r) }) (fun e -> setState { state with PreviewResult = Some (Error e) })
                |> ignore
                
            let subscribeToFeed title _ =
                ApiClient.subscribeToFeed { Dto.SubscribeToFeedDto.Title = title; Url = state.PreviewUrl }
                |> Promise.mapResult (fun r -> close "ok";r)
                |> ignore
                
            portal [
                Html.dialog [
                    prop.ref onRef
                    prop.custom("onClose", onClose)
                    prop.custom("onCancel", onCancel)
                    prop.children [
                        Html.div [
                            match state.PreviewResult with
                            | None
                            | Some (Error _) ->
                                Html.input [
                                    prop.value state.PreviewUrl
                                    prop.onChange changeUrl
                                 ]
                            | Some (Ok res) ->
                                Html.div [
                                    prop.text (sprintf "Found feed: %A" res.Title)
                                ]
                            
                            match state.PreviewResult with
                            | Some (Error e) ->
                                Html.div [
                                    prop.text (sprintf "There was an error fetching the result %A" e)
                                ]
                            | _ -> ()
                            
                            match state.PreviewResult with
                            | None
                            | Some (Error _) ->
                                Html.button [
                                    prop.type' "button"
                                    prop.text "Preview"
                                    prop.onClick previewSubscribeToFeed
                                ]
                            | Some (Ok r) ->
                                Html.button [
                                    prop.type' "button"
                                    prop.text "Add"
                                    prop.onClick (subscribeToFeed r.Title)
                                ]
                            Html.button [
                                prop.type' "button"
                                prop.text "Cancel"
                                prop.onClick cancel
                            ]
                        ]
                    ]
                ]
            ]
        )