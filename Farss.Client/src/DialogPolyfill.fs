module Farss.Client.DialogPolyfill

open Browser.Types

type DialogPolyfill =
    abstract member registerDialog: Element -> unit

let dialogPolyfill: DialogPolyfill = Fable.Core.JsInterop.importDefault "dialog-polyfill"