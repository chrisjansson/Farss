module GiraffeUtils

open Giraffe
open Persistence

let convertToHandler (result: Result<Unit, WorkflowError>) =
    match result with
    | Ok _ -> Successful.NO_CONTENT
    //todo: what to do with the exception?
    | Error (BadRequest (message, _)) -> RequestErrors.BAD_REQUEST message
