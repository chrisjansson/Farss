module SideMenu

open Dto
open Feliz

type SideMenuState = { Feeds: SubscriptionDto list }

type ViewModel<'T> =
    | Loading
    | Loaded of 'T
    
module Style =
    open Fss    

    let SideMenuItem = fss [
        Margin.value (px 1, px 12)
        Padding.value (px 0, px 8)
        Height.value (px 32)
        Display.flex
        AlignItems.center
        UserSelect.none
        Custom "gap" "10px"
        Hover [
            BackgroundColor.rgba 31 30 36 0.08
            BorderRadius.value (px 3)
        ]
    ]
    
    let SideMenuTitle = fss [
        Custom "flex" "1"
        WhiteSpace.nowrap
        TextOverflow.ellipsis
        Overflow.hidden
    ]
    
    let SideMenuItemUnread = fss [    ]
    
    let SideMenuHeader = fss [
        FontWeight.bold
    ]

[<ReactComponent>]
let SideMenu () =
    let state, setState = React.useState (ViewModel<SideMenuState>.Loading)

    React.useEffectOnce (fun () ->
        ApiClient.getSubscriptions ()
        |> PromiseResult.resultEnd
            (fun r -> setState (Loaded { Feeds = r |> List.sortBy (fun f -> f.Title) }))
            (fun _ -> ())
        |> ignore)

    Html.div
        [ prop.style
              [ style.display.flex
                style.flexDirection.column
                style.height (length.percent 100) ]

          prop.children
              [ Html.div
                    [ prop.style [ style.custom ("flex", "1") ]
                      prop.children
                          [
                            Html.div [ prop.classes [ Style.SideMenuItem; Style.SideMenuHeader]; prop.text "Feeds" ]
                            Html.div [ prop.classes [ Style.SideMenuItem; Style.SideMenuHeader]; prop.text "All" ]

                            match state with
                            | Loading -> Html.text "Loading"
                            | Loaded m ->
                                React.fragment
                                    [ for f in m.Feeds do
                                          Html.div
                                              [ Html.div
                                                    [ prop.className Style.SideMenuItem
                                                      prop.children
                                                          [ Html.div
                                                                [ prop.className Style.SideMenuTitle
                                                                  prop.text f.Title ]
                                                            Html.div
                                                                [ prop.className Style.SideMenuItemUnread
                                                                  prop.text f.Unread ] ] ] ] ] ] ]

                Html.div [ Html.text "Add feed goes here" ] ]
          ]
