module Html

open Elmish
module R = Fable.Helpers.React
module Props = Fable.Helpers.React.Props
type IHTMLProp = Props.IHTMLProp


type Attr<'msg> = 
    | OnClick of 'msg
    | Type of string
    | Value of string

let onClick = OnClick
let _type = Type
let value = Value

type Html<'msg> = Dispatch<'msg> -> Fable.Import.React.ReactElement

let convertToProp attr dispatch =
    match attr with
    | OnClick msg -> Props.OnClick (fun _ -> dispatch msg) :> IHTMLProp
    | Type t -> Props.Type t :> IHTMLProp
    | Value v -> Props.Value v :> IHTMLProp

let convertToProps props dispatch = Seq.map (fun p -> convertToProp p dispatch) props

let applyDispatch (elements: Html<'msg> seq) (dispatch: Dispatch<'msg>) = 
    Seq.map (fun e -> e dispatch) elements

let input (props: Attr<'msg> seq): Html<'msg> =
    fun d ->
        let props = convertToProps props d
        R.input props

let fragment () (elements: Html<'msg> seq): Html<'msg> =
    fun d -> R.fragment [] (applyDispatch elements d)

let str (str: string): Html<'msg> =
    fun _ -> R.str str

let div (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.div (convertToProps props d) (applyDispatch children d)
        
let h1 (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.h1 (convertToProps props d) (applyDispatch children d)

let run (html: Html<'msg>) (dispatch: Dispatch<'msg>) =
    html dispatch