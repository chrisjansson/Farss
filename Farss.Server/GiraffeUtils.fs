module GiraffeUtils

open Giraffe
open Persistence

let private convertToHandlerInternal okHandler (result: Result<'r, WorkflowError>) =
    match result with
    | Ok result -> okHandler result
    //todo: what to do with the exception?
    | Error (BadRequest (message, _)) -> RequestErrors.BAD_REQUEST message
    | Error (InvalidParameter parameters) -> 
        let message = sprintf "Invalid parameters: %s" <| System.String.Join(",", parameters)
        RequestErrors.BAD_REQUEST message

let convertToHandler (result: Result<Unit, WorkflowError>) =
    convertToHandlerInternal (fun _ -> Successful.NO_CONTENT) result

let convertToJsonResultHandler (result: Result<'a, WorkflowError>) =
    convertToHandlerInternal (fun content -> Successful.ok (json content)) result
