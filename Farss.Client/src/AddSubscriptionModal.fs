module AddSubscriptionModal

open Elmish
//Open
//Paste url of feed/website
//Ok/Next (poll to check if feed works)
    //if ok Fill in title, let user edit
        //show subscribe
    //if error show error


type Model = 
    | EnterFeedUrl of EnterFeedUrlModel
    | LoadingPreview
    | PreviewSubscription
    | PreviewFeedFailed
and EnterFeedUrlModel = { Url: string }


type Message =
    | EditUrl of string
    | PreviewSubscription
    | SubscriptionPreviewReceived of Result<unit, unit>

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
