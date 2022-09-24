module Farss.Client.AddFeedModal

open Browser.Types
open Dto
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
    | SelectFeedStep of
        {|
            PreviewResult: (int * Result<Dto.PreviewSubscribeToFeedResponseDto, Dto.FeedError>) list
            SelectedFeed: int option
            Title: string
        |}
and PreviewStepState =
    {
        PreviewUrl: string
        PreviewFailure: string option
    }
    
[<ReactComponent>]    
let private Input (placeholder: string, value: string, onChange: string -> unit) =
    React.fragment [
        Html.div [
            Html.label [
                prop.htmlFor placeholder
                prop.text placeholder
            ]
        ]

        Html.input [
            prop.id placeholder
            prop.placeholder placeholder
            prop.value value
            prop.onChange onChange
        ]
    ]
    
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
            
            let onClose (_: Event) =
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
            
            let previewSubscribeToFeed _ =
                match state with
                | PreviewStep d ->
                    let handleOk r = setState (SelectFeedStep {| PreviewResult = r |> List.indexed; SelectedFeed = None; Title = "" |})
                    let handleError e = setState (PreviewStep {| d with PreviewFailure = Some e |})
                    
                    ApiClient.previewSubscribeToFeed { Url = d.PreviewUrl }
                    |> PromiseResult.resultEnd
                           handleOk
                           handleError
                    |> ignore
                | _ -> failwith "Invalid state"
                
            let subscribeToFeed _ =
                match state with
                | SelectFeedStep d ->
                    let feed =
                        let _, feedPreview = d.PreviewResult.[d.SelectedFeed.Value]
                        match feedPreview with
                        | Ok f -> f
                        | _ -> failwith "Invalid state"
                    
                    ApiClient.subscribeToFeed { SubscribeToFeedDto.Title = d.Title; Url = feed.Url }
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
                            prop.style [
                                style.display.flex
                                style.flexDirection.column
                            ]
                            prop.children [
                                Html.div [
                                    prop.text "Add feed"
                                    prop.className "header"
                                ]
                                Html.div [
                                    prop.className "body"
                                    prop.children [
                                        match state with
                                        | PreviewStep state ->
                                            Input ("Feed url", state.PreviewUrl, changeUrl)
                                            match state.PreviewFailure with
                                            | Some e ->
                                                Html.div [
                                                    prop.style [
                                                        style.backgroundColor("#ffe5e5")
                                                        style.marginTop(10)
                                                        style.padding(5)
                                                        style.border("1px", borderStyle.solid, "red")
                                                        style.color "red"
                                                        style.borderRadius(2)
                                                    ]
                                                    prop.text $"There was an error fetching the result %A{e}"
                                                ]
                                            | _ -> ()
                                        | SelectFeedStep state ->
                                            let selectFeed (id: string) =
                                                let id =
                                                    if id = "" then None else Some (int id)
                                                
                                                let newState =
                                                    let title =
                                                        match id with
                                                        | Some id ->
                                                            let _, feed = state.PreviewResult.[id]
                                                            match feed with
                                                            | Ok feed -> feed.Title
                                                            | _ -> ""
                                                        | _ -> ""
                                                    
                                                    
                                                    SelectFeedStep {| state with SelectedFeed = id; Title = title |}
                                                setState newState
                                                                            
                                            let setTitle (title: string) =
                                                setState (SelectFeedStep {| state with Title = title |})
                                            
                                            match state.PreviewResult with
                                            | [] -> 
                                                Html.div [
                                                    prop.text "Found no feeds, meh"
                                                ]
                                            | feeds ->
                                                Html.div [
                                                    Html.div [
                                                        prop.text "Found"
                                                    ]
                                                    Html.select [
                                                        prop.onChange selectFeed
                                                        prop.children [
                                                            Html.option "Select feed"
                                                            for (index, f) in feeds do
                                                                match f with
                                                                | Ok f ->
                                                                    let feedText = $"{f.Title} - {f.Type}"
                                                                    Html.option [
                                                                        prop.value index
                                                                        prop.text feedText
                                                                    ]
                                                                | Error e ->
                                                                    let feedText = "Some error"
                                                                    Html.option [
                                                                        prop.value index
                                                                        prop.text feedText
                                                                    ]
                                                        ]
                                                    ]
                                                    
                                                    match state.SelectedFeed with
                                                    | Some id ->
                                                        let _, feed = state.PreviewResult.[id]
                                                        Html.div [
                                                            match feed with
                                                            | Ok r ->
                                                                prop.children [
                                                                    Html.div [
                                                                        Input("Feed title", state.Title, setTitle)
                                                                    ]
                                                                    
                                                                    Html.div [
                                                                        Html.text "Url: "
                                                                        Html.text r.Url
                                                                    ]
                                             
                                                                    Html.div [
                                                                        Html.b "Type: "
                                                                        match r.Type with
                                                                        | FeedType.Atom -> "Atom"
                                                                        | FeedType.Rss -> "RSS"
                                                                        |> Html.text
                                                                    ]
                                                                    
                                                                    Html.div [
                                                                        Html.b "Protocol: "
                                                                        match r.Protocol with
                                                                        | Protocol.Http -> "HTTP"
                                                                        | Protocol.Https -> "HTTPS"
                                                                        |> Html.text
                                                                    ]
        
                                               
                                                                ]
                                                            | Error _ ->
                                                                "Could not read or parse feed"
                                                                |> prop.text
                                                        ]
                                                    | None ->
                                                        Html.none
                                                ]
                                    ]
                                ]
                                Html.div [
                                    prop.className "footer"
                                    prop.text "footer"
                                    prop.children [
                                        Html.div [
                                            match state with
                                            | PreviewStep _ ->
                                                Html.button [
                                                    prop.type' "button"
                                                    prop.text "Preview"
                                                    prop.onClick previewSubscribeToFeed
                                                ]
                                            | SelectFeedStep _ ->
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
                        ]
                    ]
                ]
            ]
        )
