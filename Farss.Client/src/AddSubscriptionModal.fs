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
        LoadingPreview m, (createLoadPreviewCmd m.Url)
    | (LoadingPreview m, SubscriptionPreviewReceived (Ok r)) ->
        Model.PreviewSubscription ({ Url = m.Url; Title = r.Title; Error = None }), Cmd.none
    | (LoadingPreview m, SubscriptionPreviewReceived (Error e)) ->
        EnterFeedUrl ({ m with Error = (Some e) }), Cmd.none
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
    let renderEnterUrl (model: EnterFeedUrlModel) isLoading =
        let content = [ 
            Html.Bulma.fieldset isLoading [
                yield Html.Bulma.Field.input TextResources.SubscriptionUrlInputPlaceholder [ value model.Url; placeholder TextResources.SubscriptionUrlInputPlaceholder; onInput EditUrl ]
                match model.Error with
                | Some error -> 
                    yield Html.Bulma.label "Error"
                    yield Html.Bulma.notification [ str error ]
                | _ -> () 
            ]
        ]

        let footer = [
            Html.Bulma.Button.button [ onClick PreviewSubscription; Bulma.Button.isSuccess; Bulma.Button.isLoading isLoading; Bulma.Button.isDisabled isLoading ] TextResources.NextButtonTitle; 
            Html.Bulma.Button.button [ onClick Close ] TextResources.CancelButtonTitle; 
        ]

        (content, footer)

    let renderPreview (model: PreviewSubscriptionModel) isLoading =
        let { Url = url; Title = title; Error = error } = model

        let content = [ 
            Html.Bulma.fieldset isLoading [
                yield Html.Bulma.Field.readonlyInput TextResources.SubscriptionUrlInputPlaceholder [ value url; placeholder TextResources.SubscriptionUrlInputPlaceholder; ]
                yield Html.Bulma.Field.input TextResources.SubscriptionTitlePlaceholder [ value title; placeholder TextResources.SubscriptionTitlePlaceholder; onInput EditTitle ]
                match error with
                | Some error -> 
                    yield Html.Bulma.label "Error"
                    yield Html.Bulma.notification [ str error ]
                | _ -> () 
            ]
        ]

        let footer = [
            Html.Bulma.Button.button [ onClick Subscribe; Bulma.Button.isSuccess; Bulma.Button.isLoading isLoading; Bulma.Button.isDisabled isLoading ] TextResources.SubscribeButtonTitle
            Html.Bulma.Button.button [ onClick Close ] TextResources.CancelButtonTitle
        ]

        (content, footer)

    let modalContent =
        match model with
        | EnterFeedUrl m -> 
            renderEnterUrl m false
        | LoadingPreview m  ->
            renderEnterUrl m true
        | LoadingSubscribe m -> 
            renderPreview m true
        | Model.PreviewSubscription m -> 
            renderPreview m false

    let modalSettings = Modal.defaultSettings TextResources.AddSubscriptionModalTitle
    let (content, footer) = modalContent

    HtmlModalPortal [
        Modal.modal modalSettings content footer
    ]

