module ApiClient

open Fable.PowerPack

//TODO: Fetch convenience methods does not return response body as error

let subscribeToFeed (dto: Dto.SubscribeToFeedDto) =
    Fetch.tryPostRecord ApiUrls.SubscribeToFeed dto []
    |> Promise.mapResult ignore

let getSubscriptions () =
    let decoder = Thoth.Json.Decode.Auto.generateDecoder<Dto.SubscriptionDto list>()
    Fetch.tryFetchAs ApiUrls.GetSubscriptions decoder []
    
let deleteSubscription (dto: Dto.DeleteSubscriptionDto) =
    Fetch.tryPostRecord ApiUrls.DeleteSubscription dto []
    |> Promise.mapResult ignore

let getArticles () =
    let decoder = Thoth.Json.Decode.Auto.generateDecoder<Dto.ArticleDto list>()
    Fetch.tryFetchAs ApiUrls.GetArticles decoder []

let setArticleReadStatus (dto: Dto.SetArticleReadStatusDto) =
    Fetch.tryPostRecord ApiUrls.SetArticleReadStatus dto []
    |> Promise.mapResult ignore