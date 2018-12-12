module SetArticleReadStatusWorkflowTests

open Domain
open Expecto
open Persistence
open FakeFeedReaderAdapter
open System
open FeedReaderAdapter
open System.Threading.Tasks
open SetArticleReadStatusWorkflow

let setup test = async {
        let articleRepository = Persistence.ArticleRepositoryImpl.createInMemory()

        do! test articleRepository
    }

module Expect =
    let articleToBeRead (article: Article) (articleRepository: ArticleRepository) =
        let actualArticle = articleRepository.getAll() |> List.find (fun a -> a.Id = article.Id)
        Expect.isTrue actualArticle.IsRead "Article should be read"

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
                let command: Dto.SetArticleReadStatusDto = { ArticleId = Nullable() }
                let result = workflow command
            
                Expect.invalidParameter result [ "ArticleId" ]
            }

            "Fails when article does not exist", fun ar -> async {
                let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
                let command: Dto.SetArticleReadStatusDto = { ArticleId = Nullable(Guid.NewGuid()) }
                let result = workflow command

                Expect.error result ArticleNotFound
            }
            //non extant article
            "Sets article to read", fun (ar: ArticleRepository) -> async {
                let article = Build.article()
                ar.save article
                
                let workflow = SetArticleReadStatusWorkflow.setArticleReadStatusWorkflowImpl ar
                let command: Dto.SetArticleReadStatusDto = { ArticleId = Nullable(article.Id) }
                let result = workflow command
            
                Expect.articleToBeRead article ar
                Expect.isOk result "Request was successful"
            }
        ]

        yield! testFixtureAsync setup tests
    ]