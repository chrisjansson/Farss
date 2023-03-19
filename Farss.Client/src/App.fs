module App

open Farss.Client
open Feliz

open Dto

type ViewModel<'T> =
    | Loading
    | Loaded of 'T
    
type SideMenuState =
    {
        Feeds: SubscriptionDto list
    }

let sideMenu =
    React.functionComponent(
        fun () ->
            
            let state, setState = React.useState(ViewModel<SideMenuState>.Loading)
            
            React.useEffectOnce(
                fun () ->
                    ApiClient.getSubscriptions ()
                    |> PromiseResult.resultEnd (fun r -> setState(Loaded { Feeds = r |> List.sortBy (fun f -> f.Title) })) (fun _ -> ())
                    |> ignore
            )
            
            Html.div [
                prop.style [
                    style.display.flex
                    style.flexDirection.column
                    style.height(length.percent(100))
                ]

                prop.children [
                    Html.div [
                        prop.style [
                            style.custom ("flex", "1")
                        ]
                        prop.children [
                            Html.div [
                                prop.className "side-menu-item side-menu-header"
                                prop.text "Feeds"
                            ]
                    
                            match state with
                            | Loading -> Html.text "Loading"
                            | Loaded m ->
                                React.fragment [
                                    for f in m.Feeds do
                                        Html.div [
                                            Html.div [
                                                prop.className "side-menu-item"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "side-menu-item-title"
                                                        prop.text f.Title
                                                    ]
                                                    Html.div [
                                                        prop.className "side-menu-item-unread"
                                                        prop.text f.Unread
                                                    ]
                                                ]
                                            ]
                                        ]
                                ]
                            ]
                    ]
                    
                    Html.div [
                        Html.text "Add feed goes here"
                    ]
                ]
                
            ]
        )
    

type MenuState =
    {
        IsOpen: bool
    }
    
[<ReactComponent>]
let menu () =
    let state, setState = React.useState({ IsOpen = false })
    
    let poll = React.useCallback(fun () -> ApiClient.poll ())
    
    Html.div [
        prop.style [
            style.display.flex
            style.flexDirection.row
            style.justifyContent.flexEnd
            style.alignItems.center
            style.height(length.percent(100))
            style.margin(0, 10)
        ]
        prop.children [
            Html.button [
                prop.type' "button"
                prop.text "Add"
                prop.onClick (fun _ -> setState { state with IsOpen = true })
            ]
            
            Html.button [
                prop.type' "button"
                prop.text "Poll"
                prop.onClick (fun _ -> poll () |> ignore)
            ]
            
            if state.IsOpen then
                AddFeedModal.AddFeedDialog (fun () -> setState { state with IsOpen = false })
        ]
        
    ]

[<ReactComponent>]
let main =
    Html.div [
        prop.classes [
            "grid-container"
        ]
        prop.children [
            Html.div [
                prop.classes [ "Logo" ]
                prop.children [
                    Html.div [
                        prop.style [
                            style.display.flex
                            style.flexDirection.column
                            style.justifyContent.center
                            style.height(length.percent(100))
                            style.margin(0, 10)
                        ]
                        prop.children [
                            Html.span [
                                prop.text "Farss"
                            ]
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.classes [ "Menu" ]
                prop.children [
                    menu ()
                ]
            ]
            Html.div [
                prop.classes [ "Side-menu" ]
                prop.children [
                    sideMenu ()
                ]
            ]
            Html.div [
                prop.classes [ "Main" ]
                prop.children [
                    ArticleList.Articles ()
                ]
            ]
        ]
    ]

let documentRoot = Browser.Dom.document.getElementById ReactSettings.appRootId

ReactDOM.render (main, documentRoot)
