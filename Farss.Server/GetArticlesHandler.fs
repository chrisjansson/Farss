module GetArticlesHandler

open Giraffe
open Microsoft.AspNetCore.Http
open Persistence 
open GetArticlesWorkflow
open Microsoft.Extensions.DependencyInjection

let getArticlesHandler: HttpHandler =
    fun next (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<Dto.GetArticlesQuery>()

            let ar = ctx.RequestServices.GetService<ArticleRepository>()            
            let workflow: GetArticlesWorkflow = getArticlesWorkflowImpl ar
            
            let result = workflow dto
            return!  Successful.ok (json result) next ctx
        }