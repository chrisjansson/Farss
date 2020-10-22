[<RequireQualifiedAccess>]
module DOMPurify

type DOMPurify =
    abstract member sanitize: string -> string
    
let private instance: DOMPurify = Fable.Core.JsInterop.importDefault "dompurify"

let sanitize = instance.sanitize