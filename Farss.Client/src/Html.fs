module Html

open Elmish
open Fable.Import.React
open Fable.Core.JsInterop

module R = Fable.Helpers.React
module Props = Fable.Helpers.React.Props
type IHTMLProp = Props.IHTMLProp

type Attr<'msg> = 
    | OnClick of 'msg
    | OnInput of (string -> 'msg)
    | Type of string
    | Value of string
    | ClassName of string
    | Placeholder of string
    | ReadOnly

let inline onClick msg = OnClick msg
let inline _type t = Type t
let inline value v = Value v
let inline onInput f = OnInput f
let inline className cn = ClassName cn
let inline placeholder text = Placeholder text
let readonly = ReadOnly

type Html<'msg> = Dispatch<'msg> -> Fable.Import.React.ReactElement

let convertToProp attr dispatch =
    let onChangeR (e: FormEvent) f =
        let value = unbox<string> e.currentTarget?value
        let msg = f value
        dispatch msg

    match attr with
    | OnClick msg -> Props.OnClick (fun _ -> dispatch msg) :> IHTMLProp
    | Type t -> Props.Type t :> IHTMLProp
    | Value v -> Props.Value v :> IHTMLProp
    | OnInput f -> Props.OnChange (fun e -> onChangeR e f) :> IHTMLProp
    | ClassName cn -> Props.ClassName cn :> IHTMLProp
    | Placeholder text -> Props.Placeholder text :> IHTMLProp
    | ReadOnly -> Props.ReadOnly true :> IHTMLProp

let convertToProps props dispatch = Seq.map (fun p -> convertToProp p dispatch) props

let applyDispatch (elements: Html<'msg> seq) (dispatch: Dispatch<'msg>) = 
    Seq.map (fun e -> e dispatch) elements

let fragment () (elements: Html<'msg> seq): Html<'msg> =
    fun d -> R.fragment [] (applyDispatch elements d)

let inline str (str: string): Html<'msg> =
    fun _ -> R.str str
    
let input (props: Attr<'msg> seq): Html<'msg> =
    fun d -> R.input (convertToProps props d)

let div (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.div (convertToProps props d) (applyDispatch children d)

let label (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.label (convertToProps props d) (applyDispatch children d)
        
let h1 (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.h1 (convertToProps props d) (applyDispatch children d)
        
let h4 (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.h4 (convertToProps props d) (applyDispatch children d)
     
let span (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.span (convertToProps props d) (applyDispatch children d)

let button (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.button (convertToProps props d) (applyDispatch children d)

let run (html: Html<'msg>) (dispatch: Dispatch<'msg>) =
    html dispatch

let runChildren (children: Html<'msg> seq) (dispatch: Dispatch<'msg>) = applyDispatch children dispatch

module Bulma =

    let inline input (props: Attr<'msg> list) = 
    
        className "input" :: props |> input

    let inline readonlyInput (props: Attr<'msg> list) = 
        className "input is-static" :: readonly :: props |> input

    let inline field (children: Html<'msg> seq): Html<'msg> =
        div [ className "field" ] children

    let inline control (children: Html<'msg> seq): Html<'msg> =
        div [ className "control" ] children

    let inline label (text: string) =
        label [ className "label" ] [ str text ]

    module Button =
        let inline button (text: string) onClickMsg =
            input [ value text; onClick onClickMsg; _type "button"; className "button"; ]

        let inline success (text: string) onClickMsg =
            input [ value text; onClick onClickMsg; _type "button"; className "button is-success";]

    module Field =
        let inline input text (props: Attr<'msg> list) =
            field [
                label text
                control [
                    input props
                ]
            ]

        let inline readonlyInput text (props: Attr<'msg> list) =
            field [
                label text
                control [
                    readonlyInput props
                ]
            ]