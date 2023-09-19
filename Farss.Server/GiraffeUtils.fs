module GiraffeUtils

open Giraffe
open Microsoft.AspNetCore.StaticFiles

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
                
let convertToFileResultHandler (result: Result<string * byte[], WorkflowError>): HttpHandler =
    fun next ctx ->
        match result with
        | Ok (name, data) ->
            let contentType =
                let provider = FileExtensionContentTypeProvider()
                match provider.TryGetContentType(name) with
                | (true, contentType) -> contentType
                | _ -> 
                    let defaultContentType = "application/octet-stream"
                    defaultContentType
            Successful.ok (setContentType contentType >=> setBody data) next ctx
        | Error e ->
            match e with
            | WorkflowError.BadRequest (message, ex) ->                
                RequestErrors.badRequest (text message) next ctx
            | WorkflowError.InvalidParameter parameters ->
                let message = sprintf "Invalid parameters: %s" <| System.String.Join(",", parameters)                
                RequestErrors.badRequest (text message) next ctx
