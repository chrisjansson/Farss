module ApiClient

open Fable.Core.JsInterop
open Fable.PowerPack

//TODO: Fetch convenience methods does not return response body as error

module Fetch =
    open Thoth.Json
    open Fetch.Fetch_types

    let tryFetchAsWithPayload<'response> url =
        let responseDecoder = Decode.Auto.generateDecoder<'response>()
        //Return partially applied lambda so responseDecoder can be cached at will
        fun payload -> 
            let serializedPayload = Thoth.Json.Encode.Auto.toString(0, payload)
            let body = Body !^ serializedPayload

            Fetch.tryFetchAs url responseDecoder [ body ]

let previewSubscribeToFeed (dto: Dto.PreviewSubscribeToFeedDto) =
    Fetch.tryFetchAsWithPayload<Dto.PreviewSubscribeToFeedResponseDto> ApiUrls.PreviewSubscribeToFeed dto

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

let poll () =
    Fetch.tryPostRecord ApiUrls.PollSubscriptions () [] |> Promise.mapResult ignore