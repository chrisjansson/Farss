module SetArticleReadStatusWorkflow

open Domain
open Persistence

//todo: merge with general workflow error type
type WorkflowError =
    | InvalidParameter of string list
    | ArticleNotFound

type SetArticleReadStatusWorkflow = Dto.SetArticleReadStatusDto -> Result<unit, WorkflowError>
type SetArticleReadStatusWorkflowImpl = ArticleRepository -> SetArticleReadStatusWorkflow

let setArticleReadStatusWorkflowImpl: SetArticleReadStatusWorkflowImpl =
    fun ar commandDto ->
        let getArticle (articleId: ArticleId) =
            match ar.getAll() |> List.tryFind (fun a -> a.Id = articleId) with
            | Some article -> Ok article
            | None -> ArticleNotFound |> Error

        let setIsRead command article =
            let article = { article with IsRead = command.SetIsReadTo }
            ar.save(article)

        commandDto
            |> Dto.SetArticleReadStatusDto.toCommand
            |> Result.mapError (fun e -> InvalidParameter [e])
            |> Result.bind (fun c -> getArticle c.ArticleId |> Result.map (fun a -> (c, a)))
            |> Result.tee (fun (c, article) -> setIsRead c article)
            |> Result.map ignore