module App

open Feliz


let documentRoot = Browser.Dom.document.getElementById ReactSettings.appRootId

let root = ReactDOM.createRoot(documentRoot)
root.render(Shell.Main())
