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

//todo: refactor
let setArticleReadStatusWorkflowImpl: SetArticleReadStatusWorkflowImpl =
    fun ar commandDto ->
        if not commandDto.ArticleId.HasValue then
            InvalidParameter [ nameof <@ commandDto.ArticleId @>] |> Error
        else if not commandDto.SetIsReadTo.HasValue then   
            InvalidParameter [ nameof <@ commandDto.SetIsReadTo @>] |> Error
        else
            match ar.getAll() |> List.tryFind (fun a -> a.Id = commandDto.ArticleId.Value) with
            | Some article ->
                let article = { article with IsRead = commandDto.SetIsReadTo.Value }
        
                ar.save(article)
                Ok ()
            | None -> ArticleNotFound |> Error

