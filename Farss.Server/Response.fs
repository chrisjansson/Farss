module FalcoUtils

open System.IO
open System.Text
open System.Threading.Tasks
open Falco    
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Thoth.Json.Net

module Response =
    let ofJson
    //    (options : JsonSerializerOptions) 
        (obj : 'a) : HttpHandler =
        Response.withContentType "application/json; charset=utf-8"
        >> fun ctx -> (task {
            let encoder = Encode.Auto.generateEncoderCached<'a> (caseStrategy = CaseStrategy.CamelCase)
            let json = encoder obj |> Encode.toString 4
            do! ctx.Response.WriteString Encoding.UTF8 json
            return ()
        }) 
        
    //let ofJson    
    //    (obj : 'a) : HttpHandler =    
    //    Response.withContentType "application/json; charset=utf-8"
    //    >> ofJsonOptions Constants.defaultJsonOptions obj

module Request =
    let tryBindJsonOptions<'a>
//        (options : JsonSerializerOptions)
        (ctx : HttpContext) : Task<Result<'a, _>> = task { 
        let decoder = Decode.Auto.generateDecoderCached<'a>(caseStrategy = CaseStrategy.CamelCase)
        let reader = new StreamReader(ctx.Request.Body)
        let! (content: string) = reader.ReadToEndAsync()
        let result =
            Decode.fromString decoder content
            |> Result.mapError (fun e -> WorkflowError.BadRequest (e, None))
        return  result
    }