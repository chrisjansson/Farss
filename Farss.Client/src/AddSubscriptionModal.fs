module AddSubscriptionModal

open Elmish
open AddSubscriptionModel

module R =  Fable.Helpers.React

let createLoadPreviewCmd (url: string) = 
    let dto: Dto.PreviewSubscribeToFeedQueryDto = { Url = url }
    Cmd.ofPromiseResult ApiClient.previewSubscribeToFeed dto (fun r -> SubscriptionPreviewReceived (Ok r)) (fun e -> SubscriptionPreviewReceived (Error e))

let createSubscribeCmd (url: string) (title: string) =
    let dto: Dto.SubscribeToFeedDto = { Url = url; Title = title }
    Cmd.ofPromiseResult ApiClient.subscribeToFeed dto (fun _ -> SubscribeToFeedReceived (Ok ())) (fun e -> SubscribeToFeedReceived (Error (e.ToString())))

let udpate (msg: Message) (model: Model) =
    match (model, msg) with
    | (EnterFeedUrl m, EditUrl url) -> 
        EnterFeedUrl ({ m with Url = url }), Cmd.none
    | (EnterFeedUrl m, PreviewSubscription) ->
        LoadingPreview m.Url, (createLoadPreviewCmd m.Url)
    | (LoadingPreview url, SubscriptionPreviewReceived (Ok r)) ->
        Model.PreviewSubscription ({ Url = url; Title = r.Title; Error = None }), Cmd.none
    | (LoadingPreview url, SubscriptionPreviewReceived (Error e)) ->
        EnterFeedUrl ({ Url = url; Error = (Some e) }), Cmd.none
    | (Model.PreviewSubscription m, EditTitle title) ->
        Model.PreviewSubscription ({ m with Title = title }), Cmd.none
    | (Model.PreviewSubscription m, Subscribe) ->
        let cmd = createSubscribeCmd m.Url m.Title
        Model.LoadingSubscribe m, cmd
    | (Model.LoadingSubscribe _, SubscribeToFeedReceived (Ok _)) ->
        model, Cmd.ofMsg Close
    | (Model.LoadingSubscribe m, SubscribeToFeedReceived (Error e)) ->
        let model = Model.PreviewSubscription ({ Url = m.Url; Title = m.Title; Error = (Some e) })
        model, Cmd.none
    | (_, Close) ->
        model, Cmd.none

open Html
open ModalPortal

let view model = 
    let modalContent =
        match model with
        | EnterFeedUrl m -> 
            let footer = [
                Html.Bulma.Button.success TextResources.NextButtonTitle PreviewSubscription
                Html.Bulma.Button.button TextResources.CancelButtonTitle Close
            ]

            let content = [ 
                yield Html.Bulma.Field.input TextResources.SubscriptionUrlInputPlaceholder [ value m.Url; placeholder TextResources.SubscriptionUrlInputPlaceholder; onInput EditUrl ]
                match m.Error with
                | Some error -> 
                    yield Html.Bulma.label "Error"
                    yield Html.Bulma.notification [ str error ]
                | _ -> () 
            ]
            (content, footer)
        | LoadingPreview _ 
        | LoadingSubscribe _ ->
            let content = [ Html.str "Loading..." ] 
            (content, [])
        | Model.PreviewSubscription { Url = url; Title = title; Error = error } -> 
            let footer = [
                Html.Bulma.Button.success TextResources.SubscribeButtonTitle Subscribe
                Html.Bulma.Button.button TextResources.CancelButtonTitle Close
            ]

            let content = [ 
                yield Html.Bulma.Field.readonlyInput TextResources.SubscriptionUrlInputPlaceholder [ value url; placeholder TextResources.SubscriptionUrlInputPlaceholder; ]
                yield Html.Bulma.Field.input TextResources.SubscriptionTitlePlaceholder [ value title; placeholder TextResources.SubscriptionTitlePlaceholder; onInput EditTitle ]
                match error with
                | Some error -> 
                    yield Html.Bulma.label "Error"
                    yield Html.Bulma.notification [ str error ]
                | _ -> () 
            ]
            (content, footer)

    let modalSettings = Modal.defaultSettings TextResources.AddSubscriptionModalTitle
    let (content, footer) = modalContent

    HtmlModalPortal [
        Modal.modal modalSettings content footer
    ]

