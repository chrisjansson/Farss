module SetArticleReadStatusHandler

open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open Persistence
open Dto

let setArticleReadStatusHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let! cmd = ctx.BindJsonAsync<SetArticleReadStatusDto>()
        let ar = ctx.GetService<ArticleRepository>()

        let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
        do workflow cmd |> ignore

        return! Successful.NO_CONTENT next ctx
    }

