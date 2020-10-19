module SetArticleReadStatusWorkflowTests

open Domain
open Expecto
open Persistence
open System
open SetArticleReadStatusWorkflow

let setup test = async {
        let articleRepository = Persistence.ArticleRepositoryImpl.createInMemory()

        do! test articleRepository
    }

module Expect =
    let articleToBeRead (article: Article) (articleRepository: ArticleRepository) =
        let actualArticle = articleRepository.getAll() |> List.find (fun a -> a.Id = article.Id)
        Expect.isTrue actualArticle.IsRead "Article should be read"

    let articleToBeUnread (article: Article) (articleRepository: ArticleRepository) =
        let actualArticle = articleRepository.getAll() |> List.find (fun a -> a.Id = article.Id)
        Expect.isFalse actualArticle.IsRead "Article should not be read"

    let invalidParameter (result: Result<_,WorkflowError>) invalidParameters =
        match result with 
        | Error (InvalidParameter actualParameters) -> Expect.equal actualParameters invalidParameters "Invalid parameters"
        | _ -> Tests.failtest "Expected invalid parameter"

    let error (result: Result<_, _>) expected =
        match result with
        | Ok _ -> Tests.failtestf "Expected workflow error. Acutal: %A" result
        | Error e ->  Expect.equal e expected "Error should be"

    //let badRequest (result: Result<_, SetArticleReadStatusWorkflow.WorkflowError>) expected =
    //    match result with
    //    | Ok _ -> Tests.failtestf "Expected workflow error. Acutal: %A" result
    //    | Error wfe -> 
    //        let expectedWfe = BadRequestM expected


[<Tests>]
let tests = 
    testList "Set article read status workflow" [
        let tests = [
            "Fails when no id is given", fun ar -> async {
                
                let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
                let command: Dto.SetArticleReadStatusDto = { ArticleId = None; SetIsReadTo = Some true }
                let result = workflow command
            
                Expect.invalidParameter result [ "ArticleId" ]
            }

            "Fails when no SetIsReadTo is given", fun ar -> async {
                
                let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
                let command: Dto.SetArticleReadStatusDto = { ArticleId = Some (Guid.NewGuid()); SetIsReadTo = None }
                let result = workflow command
            
                Expect.invalidParameter result [ "SetIsReadTo" ]
            }
    
            "Fails when article does not exist", fun ar -> async {
                let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
                let command: Dto.SetArticleReadStatusDto = { ArticleId = Some (Guid.NewGuid()); SetIsReadTo = Some (true) }
                let result = workflow command

                Expect.error result (WorkflowError.BadRequest ("ArticleNotFound", None))
            }

            "Sets article to read", fun (ar: ArticleRepository) -> async {
                let article = Build.article()
                ar.save article
                
                let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
                let command: Dto.SetArticleReadStatusDto = { ArticleId = Some (article.Id); SetIsReadTo = Some (true) }
                let result = workflow command
            
                Expect.articleToBeRead article ar
                Expect.isOk result "Request was successful"
            }

            "Sets article to unread", fun (ar: ArticleRepository) -> async {
                let article = Build.article()
                ar.save article
                
                let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
                let command: Dto.SetArticleReadStatusDto = { ArticleId = Some (article.Id); SetIsReadTo = Some (false) }
                let result = workflow command
            
                Expect.articleToBeUnread article ar
                Expect.isOk result "Request was successful"
            }
        ]

        yield! testFixtureAsync setup tests
    ]