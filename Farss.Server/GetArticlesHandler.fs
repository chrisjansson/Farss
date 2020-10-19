module GetArticlesHandler

open System.Net
open Falco
open FalcoUtils
open Microsoft.AspNetCore.Http
open Persistence
open GetArticlesWorkflow
open Microsoft.Extensions.DependencyInjection

let getArticlesHandler: HttpHandler =
    fun (ctx: HttpContext) ->
        let ar = ctx.RequestServices.GetService<ArticleRepository>()

        let workflow: GetArticlesWorkflow = getArticlesWorkflowImpl ar

        let result = workflow ()
        ctx
        |> Response.withStatusCode (int HttpStatusCode.OK)
        |> Response.ofJson result
