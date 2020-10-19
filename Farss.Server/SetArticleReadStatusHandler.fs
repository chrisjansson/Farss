module SetArticleReadStatusHandler

open Microsoft.AspNetCore.Http
open Persistence
open Dto
open Falco
open GiraffeUtils

let setArticleReadStatusHandler: HttpHandler =
    fun (ctx: HttpContext) ->
        let ar = ctx.GetService<ArticleRepository>()
        let cmd = FalcoUtils.Request.tryBindJsonOptions<SetArticleReadStatusDto> ctx

        let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
        
        cmd
        |> TaskResult.bind (fun cmd -> workflow cmd)
        |> Task.bind (fun x -> convertToJsonResultHandler x ctx)
