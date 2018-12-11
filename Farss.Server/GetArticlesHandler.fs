module GetArticlesHandler

open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open Persistence
open GetArticlesWorkflow

let getArticlesHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let ar = ctx.GetService<ArticleRepository>()

            let workflow: GetArticlesWorkflow = getArticlesWorkflowImpl ar

            let result = workflow ()

            return! Successful.ok (json result) next ctx
        }