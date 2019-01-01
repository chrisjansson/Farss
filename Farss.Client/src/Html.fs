﻿module Html

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

let inline onClick msg = OnClick msg
let inline _type t = Type t
let inline value v = Value v
let inline onInput f = OnInput f

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

let convertToProps props dispatch = Seq.map (fun p -> convertToProp p dispatch) props

let applyDispatch (elements: Html<'msg> seq) (dispatch: Dispatch<'msg>) = 
    Seq.map (fun e -> e dispatch) elements

let fragment () (elements: Html<'msg> seq): Html<'msg> =
    fun d -> R.fragment [] (applyDispatch elements d)

let str (str: string): Html<'msg> =
    fun _ -> R.str str
    
let input (props: Attr<'msg> seq): Html<'msg> =
    fun d -> R.input (convertToProps props d)

let div (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.div (convertToProps props d) (applyDispatch children d)
        
let h1 (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.h1 (convertToProps props d) (applyDispatch children d)

let run (html: Html<'msg>) (dispatch: Dispatch<'msg>) =
    html dispatch