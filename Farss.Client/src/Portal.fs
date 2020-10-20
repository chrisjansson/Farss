module ModalPortal

open Feliz
open Fable.React

let private portalRoot = Browser.Dom.document.getElementById(ReactSettings.modalRootId)

type private Portal() = 
    inherit Component<unit, unit>()

    let element = Browser.Dom.document.createElement("div")

    override __.componentDidMount() = 
        portalRoot.appendChild(element) |> ignore

    override __.componentWillUnmount() =
        portalRoot.removeChild(element) |> ignore

    override this.render() = 
        ReactDOM.createPortal(React.fragment this.children ,element)

let portal (children: _ seq) =
    Helpers.ofType<Portal, _, _> () children
