module SanitizeHtml

open Fable.Core


type private DOMParser =
    abstract member parseFromString: string -> string -> Browser.Types.Document

[<Emit("new DOMParser()")>]
let private createDomParser () : DOMParser = Util.jsNative

let private domParser = createDomParser ()

let sanitizeHtml (html: string) =
    //https://web.dev/trusted-types/
    let sanitized = DOMPurify.sanitizeHtml html
    let document = domParser.parseFromString sanitized "text/html"
    document.body.innerText
