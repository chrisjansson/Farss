module Modal

open Fulma
open Fable.Helpers.React
    
type Settings<'msg> = 
    {
        Title: string
        Close: (unit -> 'msg) option
    }
let defaultSettings title = 
    {
        Title = title
        Close = None
    }

let modal (settings: Settings<_>) (children: Html.Html<'msg> seq) (footer: Html.Html<'msg> seq): Html.Html<'msg> =
    fun dispatch ->
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
                Modal.Card.foot [ ] (Html.runChildren footer dispatch)
            ]
        ]