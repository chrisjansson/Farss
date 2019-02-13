module AddSubscriptionModal

open Elmish
open ModalPortal

module R =  Fable.Helpers.React
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

module TextResources =
    [<Literal>]
    let AddSubscriptionModalTitle = "Add subscription"

    [<Literal>]
    let OkButtonTitle = "Ok"

    [<Literal>]
    let CancelButtonTitle = "Cancel"

module CardModal =

    open Fulma
    open Fable.Helpers.React
    
    type Settings<'msg> = 
        {
            Title: string
            Buttons: ButtonModel<'msg> list
            Close: (unit -> 'msg) option
        }

    and ButtonModel<'msg> = 
        {
            OnClick: unit -> 'msg 
            Title: string
            Options: Button.Option list
        }

    let defaultSettings () = 
        {
            Title = "Default text"
            Buttons = []
            Close = None
        }

    let cardModal (settings: Settings<_>) dispatch =
        let renderButton (model: ButtonModel<'msg>) =
            let onClick = Button.Option.OnClick (fun _ -> dispatch (model.OnClick ()))
            Button.button (onClick::model.Options) [ str model.Title ]

        let renderFooterButtons (buttons: ButtonModel<'msg> list) =
            buttons |> List.map renderButton

        Modal.modal [ Modal.IsActive true ] [ 
            Modal.background [ ] [ ]
            Modal.Card.card [ ] [ 
                Modal.Card.head [ ] [ 
                    yield Modal.Card.title [ ] [ str settings.Title ]
                    match settings.Close with
                    | Some m -> yield Delete.delete [ Delete.OnClick (fun _ -> dispatch (m ())) ] [ ] 
                    | None -> yield! []
                ]
                Modal.Card.body [ ] [ str "Some content" ]
                Modal.Card.foot [] (renderFooterButtons settings.Buttons)
            ]
        ]

let view model = 
    let modalSettings = 
        { CardModal.defaultSettings () with 
            Title = TextResources.AddSubscriptionModalTitle 
            Buttons = [
                { 
                    Title = TextResources.OkButtonTitle; 
                    OnClick = fun _ -> (); 
                    Options = [ Fulma.Button.Color Fulma.Color.IsSuccess ] 
                }
                {
                    Title = TextResources.CancelButtonTitle; 
                    OnClick = fun _ -> (); 
                    Options = [] 
                }
            ]
        }
    
    modalPortal [


        CardModal.cardModal modalSettings ignore
        //R.button [] [ R.str "Hello!!" ]
    ]


