module GetFileHandler

open Microsoft.AspNetCore.Http
open Persistence
open Dto
open Falco
open GiraffeUtils

let getFileHandler: HttpHandler =
    fun (ctx: HttpContext) ->
        let ar = ctx.GetService<FileRepository>()

        let parseId (id: string) =
            match System.Guid.TryParse id with
            | true, v -> Some v
            | _ -> None
        
        let cmd =
            let routeValue =
                Request.getRouteValues ctx
                |> Map.tryFind "id"
                |> Option.bind parseId
                |> Option.map (fun id -> { GetFileDto.Id = id })
            match routeValue with
            | Some v -> Ok v
            | None -> Error (WorkflowError.InvalidParameter ["id"])
            
        let workflow = GetFileWorkflow.impl ar
        
        System.Threading.Tasks.Task.CompletedTask cmd
        |> TaskResult.bind (fun cmd -> workflow cmd)
        |> Task.bind (fun x -> convertToJsonResultHandler x ctx)
