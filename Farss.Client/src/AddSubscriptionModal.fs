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
        LoadingPreview, (createLoadPreviewCmd m.Url)
    | (LoadingPreview, SubscriptionPreviewReceived (Ok r)) ->
        Model.PreviewSubscription, Cmd.none
    | (LoadingPreview, SubscriptionPreviewReceived (Error e)) ->
        PreviewFeedFailed, Cmd.none

open Html
open ModalPortal

let view model = 
    match model with
    | EnterFeedUrl m -> 
        let modalSettings = 
            { Modal.defaultSettings TextResources.AddSubscriptionModalTitle with 
                Buttons = [
                    { 
                        Title = TextResources.OkButtonTitle; 
                        OnClick = fun _ -> PreviewSubscription; 
                        Options = [ Fulma.Button.Color Fulma.Color.IsSuccess ] 
                    }
                    {
                        Title = TextResources.CancelButtonTitle;    
                        OnClick = fun _ -> Ignore; 
                        Options = [] 
                    }
                ]
            }

        HtmlModalPortal [
            Modal.modal modalSettings [ 
                Html.Bulma.Field.input TextResources.SubscriptionUrlInputPlaceholder [ value m.Url; placeholder TextResources.SubscriptionUrlInputPlaceholder; onInput EditUrl ]
            ]
        ]
    | LoadingPreview -> Html.div [] [ Html.str "Loading..." ] 
    | Model.PreviewSubscription -> Html.div [] [ Html.str "Preview here" ] 
    | PreviewFeedFailed -> Html.div [] [ Html.str "Error here" ] 


