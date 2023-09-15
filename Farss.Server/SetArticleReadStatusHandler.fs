module SetArticleReadStatusHandler

open Microsoft.AspNetCore.Http
open Persistence
open Dto
open Giraffe
open GiraffeUtils

let setArticleReadStatusHandler: HttpHandler =
    fun next (ctx: HttpContext) ->
        let ar = ctx.GetService<ArticleRepository>()
        let cmd = ctx.BindJsonAsync<SetArticleReadStatusDto>()

        let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
        
        cmd
        |> Task.map workflow
        |> Task.bindR (fun x -> convertToJsonResultHandler x next ctx)
