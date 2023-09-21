module Icons

open Fable.Core

type IconProps =
    | Size of int
    | Color of string
    | Stroke of float
//https://lucide.dev/icons/?focus=
let inline private  icon (name: string) (props: IconProps seq): Fable.React.ReactElement =
    let comp = JsInterop.import name "@tabler/icons-react"
    let props = JsInterop.keyValueList CaseRules.LowerFirst props
    Feliz.Interop.reactApi.createElement (comp, props) 

type Icon =
    static member IconFileRss = icon "IconFileRss"


