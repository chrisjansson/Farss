module SetArticleReadStatusWorkflow

open Domain
open Persistence

open Microsoft.FSharp.Quotations

//todo: extract
let nameof (q:Expr<_>) = 
  match q with 
  | Patterns.Let(_, _, DerivedPatterns.Lambdas(_, Patterns.Call(_, mi, _))) -> mi.Name
  | Patterns.PropertyGet(_, mi, _) -> mi.Name
  | DerivedPatterns.Lambdas(_, Patterns.Call(_, mi, _)) -> mi.Name
  | _ -> failwith "Unexpected format"

let any<'R> : 'R = failwith "!"

//todo: merge with general workflow error type
type WorkflowError =
    | InvalidParameter of string list
    | ArticleNotFound

type SetArticleReadStatusWorkflow = Dto.SetArticleReadStatusDto -> Result<unit, WorkflowError>
type SetArticleReadStatusWorkflowImpl = ArticleRepository -> SetArticleReadStatusWorkflow

module Nullable =
    open System
    let value (name: string) (v: Nullable<'a>) =
        if not v.HasValue then
            Error name
        else
            Ok v.Value

type ResultBuilder() =
    member x.Bind(comp, func) = Result.bind func comp
    member x.Return(value) = Ok value

let result = ResultBuilder()

//todo: refactor
let setArticleReadStatusWorkflowImpl: SetArticleReadStatusWorkflowImpl =
    fun ar commandDto ->
        let toCommand (dto: Dto.SetArticleReadStatusDto): Result<SetArticleReadStatusCommand, string> = result {
            let! articleId = dto.ArticleId |> Nullable.value (nameof <@ commandDto.ArticleId @>)
            let! setIsReadTo = dto.SetIsReadTo |> Nullable.value (nameof <@ commandDto.SetIsReadTo @>)
            
            return { ArticleId = articleId; SetIsReadTo = setIsReadTo }
        }

        let getArticle (articleId: ArticleId) =
            match ar.getAll() |> List.tryFind (fun a -> a.Id = articleId) with
            | Some article -> Ok article
            | None -> ArticleNotFound |> Error

        let setIsRead command article =
            let article = { article with IsRead = command.SetIsReadTo }
            ar.save(article)

        commandDto
            |> toCommand
            |> Result.mapError (fun e -> InvalidParameter [e])
            |> Result.bind (fun c -> getArticle c.ArticleId |> Result.map (fun a -> (c, a)))
            |> Result.tee (fun (c, article) -> setIsRead c article)
            |> Result.map ignore