module Html.Bulma

open Html

let inline input (props: Attr<'msg> list) = 
    className "input" :: props |> Html.input

let inline field (children: Html<'msg> seq): Html<'msg> =
    div [ className "field" ] children

let inline control (children: Html<'msg> seq): Html<'msg> =
    div [ className "control" ] children

let inline label (text: string) =
    label [ className "label" ] [ str text ]

module Field =
    let inline input text (props: Attr<'msg> list) =
        field [
            label text
            control [
                input props
            ]
        ]
        