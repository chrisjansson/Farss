module FeedIcon

open System
open Feliz

open Fss

module private Style =
    let Icon = fss [
        BorderRadius.value (pct 15)
    ]

[<ReactComponent>]
let FeedIcon (iconId:  Guid option, iconSize: int) =
    match iconId with
    | Some id -> 
        Html.img [ prop.className Style.Icon; prop.src (ApiUrls.GetFile id); prop.width iconSize; prop.height iconSize ]
    | None -> React.fragment []
