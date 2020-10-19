module GiraffeUtils

open System.Net
open Falco
open FalcoUtils

let convertToJsonResultHandler (result: Result<'a, WorkflowError>) =
    fun ctx ->
        match result with
        | Ok r ->
            ctx
            |> Response.withContentType "application/json"
            |> Response.withStatusCode 200
            |> Response.ofJson r
        | Error e ->
            match e with
            | WorkflowError.BadRequest (message, ex) ->
                ctx
                |> Response.withStatusCode (int HttpStatusCode.BadRequest)
                |> Response.ofPlainText message
            | WorkflowError.InvalidParameter parameters ->
                let message = sprintf "Invalid parameters: %s" <| System.String.Join(",", parameters)
                ctx
                |> Response.withStatusCode (int HttpStatusCode.BadRequest)
                |> Response.ofPlainText message
