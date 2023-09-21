module Article

open Dto
open Feliz


[<ReactComponent>]
let Article (article: ArticleDto) : Fable.React.ReactElement =
    Html.div [
        Html.div [ prop.className "selected-article-title"; prop.text article.Title ]
        Html.div [ prop.className "article-content"; prop.innerHtml article.Content ]
    ]
