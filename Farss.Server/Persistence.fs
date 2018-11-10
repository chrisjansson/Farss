module Persistence
open Domain

type FeedRepository =
    {
        getAll: unit -> Feed list
        save: Feed -> unit
        delete: FeedId -> unit
    }

let create () =
    let mutable feeds = []
    let getAll () = feeds
    let save (feed: Feed) = feeds <- feed :: feeds
    let delete (id: FeedId) = 
        let feeds' = List.filter (fun (f: Feed) -> f.Id <> id) feeds
        feeds <- feeds'

    {
        getAll = getAll
        save = save
        delete = delete
    }

type WorkflowError =
    | BadRequest of string * System.Exception
    | InvalidParameter of string list