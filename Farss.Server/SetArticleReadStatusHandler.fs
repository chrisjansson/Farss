module SetArticleReadStatusHandler

open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open Persistence
open Dto
open SetArticleReadStatusWorkflow

let convertToHandler (result: Result<Unit, SetArticleReadStatusWorkflow.WorkflowError>) =
    match result with
    | Ok _ -> Successful.NO_CONTENT
    | Error (InvalidParameter parameters) -> 
        let message = sprintf "Invalid parameters: %s" <| System.String.Join(",", parameters)
        RequestErrors.BAD_REQUEST message
    | Error ArticleNotFound -> 
        RequestErrors.BAD_REQUEST "Article not found"

let setArticleReadStatusHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let! cmd = ctx.BindJsonAsync<SetArticleReadStatusDto>()
        let ar = ctx.GetService<ArticleRepository>()

        let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
        let handler = workflow >> convertToHandler
        
        return! handler cmd next ctx
    }