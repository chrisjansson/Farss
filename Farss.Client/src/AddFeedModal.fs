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
    | PreviewStep of 
        {|
            PreviewUrl: string
            PreviewFailure: string option
        |}
    | NameStep of
        {|
            Url: string
            Title: string
            PreviewResult: Dto.PreviewSubscribeToFeedResponseDto
        |}

let addFeedDialog =
    React.functionComponent(
        fun (props: AddFeedDialogProps) ->
            let onRef = React.useRef None
            
            React.useEffectOnce(
                fun () ->
                    match onRef.current with
                    | Some e ->
                        DialogPolyfill.dialogPolyfill.registerDialog e
                        e?showModal()
                    | _ -> ()
                    ()
                )
            
            let state, setState = React.useState(PreviewStep {| PreviewUrl= ""; PreviewFailure = None |})
            
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
                match state with
                | PreviewStep d ->
                    setState(PreviewStep {| d with PreviewUrl = s |})
                | _ -> failwith "Invalid state"
            
            let changeTitle s =
                match state with
                | NameStep d ->
                    setState(NameStep {| d with Title = s |})
                | _ -> failwith "Invalid state"
            
            let previewSubscribeToFeed _ =
                match state with
                | PreviewStep d ->
                    ApiClient.previewSubscribeToFeed { Url = d.PreviewUrl }
                    |> PromiseResult.resultEnd (fun r -> setState (NameStep {| Url = d.PreviewUrl; Title = r.Title; PreviewResult = r |})) (fun e -> setState (PreviewStep {| d with PreviewFailure = Some e |}))
                    |> ignore
                | _ -> failwith "Invalid state"
                
            let subscribeToFeed _ =
                match state with
                | NameStep d ->
                    ApiClient.subscribeToFeed { Dto.SubscribeToFeedDto.Title = d.Title; Url = d.Url }
                    |> Promise.mapResult (fun r -> close "ok";r)
                    |> ignore
                | _ ->
                    failwith "Invalid state"
            
            portal [
                Html.dialog [
                    prop.ref onRef
                    prop.custom("onClose", onClose)
                    prop.custom("onCancel", onCancel)
                    prop.children [
                        Html.div [
                            match state with
                            | PreviewStep state ->
                                Html.input [
                                    prop.value state.PreviewUrl
                                    prop.onChange changeUrl
                                ]
                                match state.PreviewFailure with
                                | Some e ->
                                    Html.div [
                                        prop.text (sprintf "There was an error fetching the result %A" e)
                                    ]
                                | _ -> ()
                                Html.button [
                                    prop.type' "button"
                                    prop.text "Preview"
                                    prop.onClick previewSubscribeToFeed
                                ]
                            | NameStep state ->
                                Html.div [
                                    prop.text "Found a feed, now name it"
                                ]
                                Html.input [
                                    prop.value state.Title
                                    prop.onChange changeTitle
                                ]
                                Html.button [
                                    prop.type' "button"
                                    prop.text "Add"
                                    prop.onClick subscribeToFeed
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