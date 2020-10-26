[<RequireQualifiedAccess>]
module DOMPurify

type DOMPurify =
    abstract member sanitize: string -> string
    abstract member sanitize: string * Config -> string
    
and Config =
    abstract member KEEP_CONTENT: bool with get, set
    
let private instance: DOMPurify = Fable.Core.JsInterop.importDefault "dompurify"

open Fable.Core.JsInterop

let sanitize (s: string) =
    let config = createEmpty<Config>
    config.KEEP_CONTENT <- true
    
    instance.sanitize (s, config)
    
    
type SanitizeHtmlConfig =
    abstract member disallowedTagsMode: string with get, set
    
let private sanitizeInstance (_: string) (_: SanitizeHtmlConfig): string = Fable.Core.JsInterop.importDefault "sanitize-html"

let sanitizeHtml s =
    let config = createEmpty<SanitizeHtmlConfig>
    config.disallowedTagsMode <- "recursiveEscape"
    sanitizeInstance s config