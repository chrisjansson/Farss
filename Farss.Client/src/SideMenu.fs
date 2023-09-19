module SideMenu

open Dto
open Elmish.HMR
open Feliz
open Fss
open Fss.Feliz

type private SideMenuState = { Feeds: SubscriptionDto list }

type private ViewModel<'T> =
    | Loading
    | Loaded of 'T

module Style =

    let feedRowHeight = 32

    let SideMenuItem =
        fss [
            Margin.value (px 0, px 12)
            Padding.value (px 0, px 8)
            Display.flex
            AlignItems.center
            UserSelect.none
            Hover [ BackgroundColor.rgba 31 30 36 0.08; BorderRadius.value (px 3) ]
        ]

    let SideMenuTitle =
        fss [ Custom "flex" "1"; WhiteSpace.nowrap; TextOverflow.ellipsis; Overflow.hidden ]

    let SideMenuItemUnread = fss []

    let SideMenuHeader = fss [
        FontWeight.bold
        Height.value (px feedRowHeight)
    ]

    let FeedListGrid =
        fss [ Display.grid; Custom "grid-template-columns" "auto 1fr auto" ]

    let FeedItemContainer =
        fss [
            Display.grid
            GridTemplateColumns.subgrid
            GridColumn.value "1/4"
            Height.value (px feedRowHeight)
            Grid.GridColumnGap.value (px 10)
        ]

    let iconSize = feedRowHeight

    let FeedIcon =
        fss [ Width.value (px iconSize); Height.value (pct 100); BackgroundColor.green ]

[<ReactComponent>]
let SideMenu () =
    let state, setState = React.useState (ViewModel<SideMenuState>.Loading)

    React.useEffectOnce (fun () ->
        ApiClient.getSubscriptions ()
        |> PromiseResult.resultEnd
            (fun r ->
                setState (
                    Loaded {
                        Feeds = r |> List.sortBy (fun f -> f.Title)
                    }
                ))
            (fun _ -> ())
        |> ignore)

    Html.div [
        prop.style [
            style.display.flex
            style.flexDirection.column
            style.height (length.percent 100)
        ]

        prop.children [
            Html.div [
                prop.style [ style.custom ("flex", "1") ]
                prop.children [
                    Html.div [ prop.classes [ Style.SideMenuItem; Style.SideMenuHeader ]; prop.text "Feeds" ]

                    match state with
                    | Loading -> Html.text "Loading"
                    | Loaded m ->
                        Html.div [
                            prop.classes [ Style.FeedListGrid ]
                            prop.children [
                                Html.div [
                                    prop.classes [ Style.FeedItemContainer; Style.SideMenuItem ]
                                    prop.children [
                                        Html.div [ prop.className Style.FeedIcon ]
                                        Html.div "All"
                                        Html.div "All"
                                    ]
                                ]

                                for f in m.Feeds do
                                    Html.div [
                                        prop.classes [ Style.FeedItemContainer; Style.SideMenuItem ]
                                        prop.children [
                                            Html.div [ prop.className Style.FeedIcon ]
                                            Html.div [ prop.className Style.SideMenuTitle; prop.text f.Title ]
                                            Html.div [ prop.className Style.SideMenuItemUnread; prop.text f.Unread ]
                                        ]
                                    ]

                            ]
                        ]

                ]
            ]

            Html.div [ Html.text "Add feed goes herea" ]
        ]
    ]
