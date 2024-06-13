module ApiClient

open System
open Browser.Types
open Fable.Core.JsInterop
open Thoth.Json
//open Fetch


module Fetch =
    open Thoth.Json
    open Fetch
    
    let inline private prependBaseUrl (url: string) =
        let pathname = Browser.Dom.window.location.pathname
        pathname + url
        

    let inline tryFetchAs url (responseDecoder: Decoder<_>) parameters =
        let decode = Decode.fromString responseDecoder
        let decodeResponse (response: Response) = response.text () |> Promise.map decode
        let url = prependBaseUrl url
        
        tryFetch url parameters
        |> Promise.mapResultError (fun e -> e.Message)
        |> PromiseResult.bind decodeResponse

    let inline tryFetchAsWithPayload<'response, 'payload> (url: string) (payload: 'payload) =
        let responseDecoder =
            Decode.Auto.generateDecoderCached<'response> (caseStrategy = CaseStrategy.CamelCase)

        let serializedPayload =
            Encode.Auto.toString (0, payload, caseStrategy = CaseStrategy.CamelCase)

        let body = Body !^serializedPayload
        let method = Method HttpMethod.POST
        tryFetchAs url responseDecoder [ method; body ]

    let inline private sendRecord
        (url: string)
        (record: 'T)
        (properties: RequestProperties list)
        httpMethod
        : Fable.Core.JS.Promise<Response> =
        let defaultProps = [
            RequestProperties.Method httpMethod
            requestHeaders [ ContentType "application/json" ]
            RequestProperties.Body !^(Encode.Auto.toString (0, record, caseStrategy = CaseStrategy.CamelCase))
        ]
        // Append properties after defaultProps to make sure user-defined values
        // override the default ones if necessary
        List.append defaultProps properties |> fetch url

    /// Sends a HTTP post with the record serialized as JSON.
    /// This function already sets the HTTP Method to POST sets the json into the body.
    let inline postRecord<'T>
        (url: string)
        (record: 'T)
        (properties: RequestProperties list)
        : Fable.Core.JS.Promise<Response> =
        let url = prependBaseUrl url
        sendRecord url record properties HttpMethod.POST

    let inline tryPostRecord<'T>
        (url: string)
        (record: 'T)
        (properties: RequestProperties list)
        : Fable.Core.JS.Promise<Result<Response, Exception>> =
        postRecord url record properties |> Promise.result

//    /// Sends a HTTP put with the record serialized as JSON.
//    /// This function already sets the HTTP Method to PUT, sets the json into the body.
//    let putRecord (url: string) (record:'T) (properties: RequestProperties list): Fable.Core.JS.Promise<Response> =
//        sendRecord url record properties HttpMethod.PUT
//
//    let tryPutRecord (url: string) (record:'T) (properties: RequestProperties list): Fable.Core.JS.Promise<Result<Response, Exception>> =
//        putRecord url record properties |> Promise.result
//
//    /// Sends a HTTP patch with the record serialized as JSON.
//    /// This function already sets the HTTP Method to PATCH sets the json into the body.
//    let patchRecord (url: string) (record:'T) (properties: RequestProperties list) : Fable.Core.JS.Promise<Response> =
//        sendRecord url record properties HttpMethod.PATCH
//
//    /// Sends a HTTP OPTIONS request.
//    let tryOptionsRequest (url:string) : Fable.Core.JS.Promise<Result<Response, Exception>> =
//        fetch url [RequestProperties.Method HttpMethod.OPTIONS] |> Promise.result

let inline previewSubscribeToFeed (dto: Dto.PreviewSubscribeToFeedQueryDto) =
    Fetch.tryFetchAsWithPayload<Result<Dto.PreviewSubscribeToFeedResponseDto, Dto.FeedError> list, Dto.PreviewSubscribeToFeedQueryDto>
        ApiUrls.PreviewSubscribeToFeed
        dto

let subscribeToFeed (dto: Dto.SubscribeToFeedDto) =
    Fetch.tryPostRecord ApiUrls.SubscribeToFeed dto [] |> Promise.mapResult ignore

let getSubscriptions () =
    let decoder =
        Thoth.Json.Decode.Auto.generateDecoder<Dto.SubscriptionDto list> (caseStrategy = CaseStrategy.CamelCase)

    Fetch.tryFetchAs ApiUrls.GetSubscriptions decoder []

//let deleteSubscription (dto: Dto.DeleteSubscriptionDto) =
//    Fetch.tryPostRecord ApiUrls.DeleteSubscription dto []
//    |> Promise.mapResult ignore
//

let getArticles (feed: Guid option) (count: int) : Fable.Core.JS.Promise<Result<Dto.ArticleDto list, _>> =
    Fetch.tryFetchAsWithPayload ApiUrls.GetArticles {
        Dto.GetArticlesQuery.Count = count
        Dto.Feed = feed
    }

//let setArticleReadStatus (dto: Dto.SetArticleReadStatusDto) =
//    Fetch.tryPostRecord ApiUrls.SetArticleReadStatus dto []
//    |> Promise.mapResult ignore
//
let poll () =
    Fetch.tryPostRecord ApiUrls.PollSubscriptions () [] |> Promise.mapResult ignore
