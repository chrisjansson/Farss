module AddSubscriptionModal

open Elmish
open AddSubscriptionModel

module R =  Fable.Helpers.React
//Open
//Paste url of feed/website
//Ok/Next (poll to check if feed works)
    //if ok Fill in title, let user edit
        //show subscribe
    //if error show error


let createLoadPreviewCmd (url: string) = 
    let dto: Dto.PreviewSubscribeToFeedQueryDto = { Url = url }
    Cmd.ofPromiseResult ApiClient.previewSubscribeToFeed dto (fun r -> SubscriptionPreviewReceived (Ok r)) (fun e -> SubscriptionPreviewReceived (Error e))

let udpate (msg: Message) (model: Model) =
    match (model, msg) with
    | (EnterFeedUrl _, EditUrl url) -> 
        EnterFeedUrl ({ Url = url }), Cmd.none
    | (EnterFeedUrl m, PreviewSubscription) ->
        LoadingPreview m.Url, (createLoadPreviewCmd m.Url)
    | (LoadingPreview url, SubscriptionPreviewReceived (Ok r)) ->
        Model.PreviewSubscription (url, r.Title), Cmd.none
    | (LoadingPreview url, SubscriptionPreviewReceived (Error e)) ->
        PreviewFeedFailed (url, e), Cmd.none

open Html
open ModalPortal

let view model = 
    let modalContent =
        match model with
        | EnterFeedUrl m -> 
            let footer = [
                Html.Bulma.Button.success TextResources.NextButtonTitle PreviewSubscription
                Html.Bulma.Button.button TextResources.CancelButtonTitle Ignore
            ]

            let content = [ 
                Html.Bulma.Field.input TextResources.SubscriptionUrlInputPlaceholder [ value m.Url; placeholder TextResources.SubscriptionUrlInputPlaceholder; onInput EditUrl ]
            ]
            (content, footer)
            
        | LoadingPreview _ -> 
            let content = [ Html.str "Loading..." ] 
            (content, [])
        | Model.PreviewSubscription (url, title) -> 
            let footer = [
                Html.Bulma.Button.success TextResources.NextButtonTitle Ignore
                Html.Bulma.Button.button TextResources.CancelButtonTitle Ignore
            ]

            let content = [ 
                Html.Bulma.Field.readonlyInput TextResources.SubscriptionUrlInputPlaceholder [ value url; placeholder TextResources.SubscriptionUrlInputPlaceholder; onInput EditUrl ]
                Html.Bulma.Field.input TextResources.SubscriptionTitlePlaceholder [ value title; placeholder TextResources.SubscriptionTitlePlaceholder ]
            ]
            (content, footer)
        | PreviewFeedFailed (url, error) ->
            let footer = [
                Html.Bulma.Button.success TextResources.NextButtonTitle PreviewSubscription
                Html.Bulma.Button.button TextResources.CancelButtonTitle Ignore
            ]
            
            let content = [ 
                Html.Bulma.Field.input TextResources.SubscriptionUrlInputPlaceholder [ value url; placeholder TextResources.SubscriptionUrlInputPlaceholder; onInput EditUrl ]
                Html.str "Error here"
                Html.pre [] [ str error ]
            ] 
            (content, footer)

    let modalSettings = Modal.defaultSettings TextResources.AddSubscriptionModalTitle
    let (content, footer) = modalContent

    HtmlModalPortal [
        Modal.modal modalSettings content footer
    ]

