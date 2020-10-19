module ModalPortal
//
//open Fable.Import
//open Fable.Import.Browser
//
//let private modalRoot = document.getElementById(ReactSettings.modalRootId)
//
//type ModalPortal() = 
//    inherit React.Component<unit, unit>()
//
//    let element = document.createElement("div")
//
//    override __.componentDidMount() = 
//        modalRoot.appendChild(element) |> ignore
//
//    override __.componentWillUnmount() =
//        modalRoot.removeChild(element) |> ignore
//
//    override this.render() = 
//        ReactDom.createPortal(Fable.Helpers.React.fragment [] this.children, element)
//
//let modalPortal (children: React.ReactElement seq) = 
//    Fable.Helpers.React.ofType<ModalPortal, _, _> () children
//
//let HtmlModalPortal (children: Html.Html<'msg> seq): Html.Html<'msg> =
//    fun d -> modalPortal (Html.runChildren children d)
