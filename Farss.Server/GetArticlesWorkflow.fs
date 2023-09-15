module GetArticlesWorkflow

open Persistence

type GetArticlesWorkflow = Dto.GetArticlesQuery -> Dto.ArticleDto list
type GetArticlesWorkflowImpl = ArticleRepository -> GetArticlesWorkflow

let getArticlesWorkflowImpl: GetArticlesWorkflowImpl =
    fun ar dto ->
        ar.getTop dto.Count
        |> List.map (fun a -> { Dto.ArticleDto.Title = a.Title; IsRead = a.IsRead; PublishedAt = a.Timestamp; Link = a.Link; Content = a.Content; Summary = a.Summary; FeedId = a.Subscription })
        