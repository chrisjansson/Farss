﻿module Html

open Elmish
open Fable.Import.React
open Fable.Core.JsInterop
open Fable.Helpers.React.Props

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
    | Disabled
    | DisabledB of bool
    | Href of string
    | DangerouslySetInnerHTML of string

let inline onClick msg = OnClick msg
let inline _type t = Type t
let inline value v = Value v
let inline onInput f = OnInput f
let inline className<'msg> cn: Attr<'msg> = ClassName cn
let inline placeholder text = Placeholder text
let readonly = ReadOnly
let disabled = Disabled
let inline href s = Href s

let dangerouslySetInnerHTML s = DangerouslySetInnerHTML s

type Html<'msg> = Dispatch<'msg> -> Fable.Import.React.ReactElement

let convertToProp attr dispatch =
    let onChangeR (e: FormEvent) f =
        let value = unbox<string> e.currentTarget?value
        let msg = f value
        dispatch msg

    match attr with
    | OnClick msg -> Props.OnClick (
                                       fun e ->
                                           if not e.ctrlKey then do
                                            e.preventDefault()
                                            dispatch msg
                                   ) :> IHTMLProp
    | Type t -> Props.Type t :> IHTMLProp
    | Value v -> Props.Value v :> IHTMLProp
    | OnInput f -> Props.OnChange (fun e -> onChangeR e f) :> IHTMLProp
    | ClassName cn -> Props.ClassName cn :> IHTMLProp
    | Placeholder text -> Props.Placeholder text :> IHTMLProp
    | ReadOnly -> Props.ReadOnly true :> IHTMLProp
    | Disabled -> Props.Disabled true :> IHTMLProp
    | DisabledB b -> Props.Disabled b :> IHTMLProp
    | Href s -> Props.Href s :> IHTMLProp
    | DangerouslySetInnerHTML s -> Props.DangerouslySetInnerHTML ({ DangerousHtml.__html = s }) :> IHTMLProp

let convertToProps props dispatch = Seq.map (fun p -> convertToProp p dispatch) props

let applyDispatch (elements: Html<'msg> seq) (dispatch: Dispatch<'msg>) = 
    Seq.map (fun e -> e dispatch) elements

let fragment () (elements: Html<'msg> seq): Html<'msg> =
    fun d -> R.fragment [] (applyDispatch elements d)

let inline str (str: string): Html<'msg> =
    fun _ -> R.str str
    
let a (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.a (convertToProps props d) (applyDispatch children d)
    
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

let pre (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.pre (convertToProps props d) (applyDispatch children d)

let fieldset (props: Attr<'msg> seq) (children: Html<'msg> seq): Html<'msg> =
    fun d -> R.fieldset (convertToProps props d) (applyDispatch children d)

let run (html: Html<'msg>) (dispatch: Dispatch<'msg>) =
    html dispatch

let runChildren (children: Html<'msg> seq) (dispatch: Dispatch<'msg>) = applyDispatch children dispatch

let map (mapper: 'msgA -> 'msgB) (html: Html<'msgA>): Html<'msgB> =
    fun d -> html (fun m -> d (mapper m))

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
            
    let inline notification (children: Html<'msg> seq): Html<'msg> = 
        div [ className "notification" ] children

    let inline fieldset isDisabled (children: Html<'msg> seq): Html<'msg> =
        if isDisabled then 
            fieldset [ disabled ] children
        else 
            fieldset [] children

    module Button =
        let aggregateClassName (props: Attr<'msg> list) =
            let folder ((classNames, props): string * (Attr<'msg> list)) (a: Attr<'msg>) =
                match a with
                | ClassName c -> (c + " " + classNames, props)
                | p -> (classNames, p::props)

            let (cn, props) = List.fold folder ("", []) props
            className cn::props

        let inline button props text =
            let props = (className "button"::props)
            let props = aggregateClassName props

            button props [ str text ]

        let inline isSuccess<'msg> = className<'msg> "is-success"

        let inline isDisabled<'msg> isDisabled: Attr<'msg> =
            DisabledB isDisabled

        let inline isLoading<'msg> isLoading = 
            if isLoading then 
                className<'msg> "is-loading"
            else    
                className<'msg> ""

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