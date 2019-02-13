module Modal

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

let defaultSettings title = 
    {
        Title = title
        Buttons = []
        Close = None
    }

let modal (settings: Settings<_>) (children: Html.Html<'msg> seq) : Html.Html<'msg> =
    fun dispatch ->
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
                Modal.Card.body [ ] (Html.runChildren children dispatch)
                Modal.Card.foot [ ] (renderFooterButtons settings.Buttons)
            ]
        ]