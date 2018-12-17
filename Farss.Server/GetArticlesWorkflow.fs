module GetArticlesWorkflow

open Persistence

type GetArticlesWorkflow = unit -> Dto.ArticleDto list
type GetArticlesWorkflowImpl = ArticleRepository -> GetArticlesWorkflow

let getArticlesWorkflowImpl: GetArticlesWorkflowImpl =
    fun ar () ->
        ar.getAll()
        |> List.map (fun a -> { Dto.ArticleDto.Title = a.Title; IsRead = a.IsRead; PublishedAt = a.PublishedAt })
        