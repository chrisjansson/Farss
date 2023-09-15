module Portal

open Feliz
open Fable.React

let private portalRoot = Browser.Dom.document.getElementById(ReactSettings.modalRootId)

let portal (children: _ seq) =
    let content = React.fragment children 
    ReactDom.createPortal(content, portalRoot)
