module Persistence
open Domain

type FeedRepository =
    {
        getAll: unit -> Feed list
        save: Feed -> unit
    }

let create () =
    let mutable feeds = []
    let getAll () = feeds
    let save (feed: Feed) = feeds <- feed :: feeds
    {
        getAll = getAll
        save = save
    }

type WorkflowError =
    | BadRequest of string * System.Exception