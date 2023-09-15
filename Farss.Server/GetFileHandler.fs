module GetFileHandler

open Persistence
open Dto
open Giraffe
open GiraffeUtils
open Microsoft.AspNetCore.Routing

let tryGet (id: string) (values: RouteValueDictionary) =
    match values.TryGetValue(id) with
    | true, x -> Some x
    | _ -> None

let getFileHandler: HttpHandler =
    fun next ctx ->
        let ar = ctx.GetService<FileRepository>()

        let parseId (id: obj) =
            match System.Guid.TryParse (id :?> string) with
            | true, v -> Some v
            | _ -> None
        
        let cmd =
            let routeValue =
                ctx.GetRouteData ()
                |> (fun x -> x.Values)
                |> tryGet "id"
                |> Option.bind parseId
                |> Option.map (fun id -> { GetFileDto.Id = id })
            match routeValue with
            | Some v -> Ok v
            | None -> Error (WorkflowError.InvalidParameter ["id"])
            
        let workflow = GetFileWorkflow.impl ar
        
        cmd
        |> Result.bind workflow
        |> (fun x -> convertToJsonResultHandler x next ctx)
    