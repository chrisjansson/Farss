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
    Cmd.none

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
    let modalSettings = 
        { Modal.defaultSettings TextResources.AddSubscriptionModalTitle with 
            Buttons = [
                { 
                    Title = TextResources.OkButtonTitle; 
                    OnClick = fun _ -> Ignore; 
                    Options = [ Fulma.Button.Color Fulma.Color.IsSuccess ] 
                }
                {
                    Title = TextResources.CancelButtonTitle; 
                    OnClick = fun _ -> Ignore; 
                    Options = [] 
                }
            ]
        }
    
    match model with
    | EnterFeedUrl m -> 

        HtmlModalPortal [
            Modal.modal modalSettings [ 
                Html.input [ Value m.Url; OnInput (fun s -> EditUrl s) ]
            ]
        ]
    | _ -> Html.div [] []


