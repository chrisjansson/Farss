module GiraffeUtils

open Giraffe

let convertToJsonResultHandler (result: Result<'a, WorkflowError>): HttpHandler =
    fun next ctx ->
        match result with
        | Ok r ->
            Successful.ok (json r) next ctx 
        | Error e ->
            match e with
            | WorkflowError.BadRequest (message, ex) ->                
                RequestErrors.badRequest (text message) next ctx
            | WorkflowError.InvalidParameter parameters ->
                let message = sprintf "Invalid parameters: %s" <| System.String.Join(",", parameters)                
                RequestErrors.badRequest (text message) next ctx
