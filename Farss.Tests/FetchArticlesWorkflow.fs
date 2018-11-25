module FetchArticlesWorkflow

open Expecto
open Persistence

let setup test = async {
    
        let subscriptionRepository = Persistence.create () 
        let articleRepository = Persistence.ArticleRepositoryImpl.createInMemory()
        let adapter = FakeFeedReaderAdapter.

        do! test subscriptionRepository articleRepository
    
    }

[<Tests>]
let tests = 
    testList "Fetch articles workflow" [
        let tests = [
            "Does nothing when no feeds exists", fun subs (articles: ArticleRepository) -> async {
                //run workflow

                let result = Ok ()
                
                Expect.isOk result "Workflow result"
                Expect.isEmpty (articles.getAll()) "Articles"
            } 
        ]
        yield! testFixtureAsync setup tests

        //no feeds, no articles does nothing
        //feed with no articles does nothing
        //feed with one new article adds one
        //feed with existing article does nothing
        //multiple feeds multiple articles, read and not read
        //failing reads?
    ]