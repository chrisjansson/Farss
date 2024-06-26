module FeedReaderAdapter

open System.Threading.Tasks
open AngleSharp
open CodeHollow.FeedReader
open System
open CodeHollow.FeedReader.Feeds
open Farss.Server
open Persistence

type  FeedError =
    | FetchError of Exception
    | ParseError of Exception

type Feed = 
    { 
        Title: string
        Icon: (string * byte[]) option
        Description: string
        Items: Item list
        Type: FeedType
    }
and FeedType =
    | Atom
    | Rss
and Item = 
    {
        Title: string
        Id: string
        Content: string option
        Source: string
        Summary: string option
        Timestamp: DateTimeOffset option
        Link: string option
    }

type DiscoveredFeed =
    {
        FeedType: FeedType
        Url: string
        Title: string
        Icon: (string * byte[]) option
        Protocol: Protocol
    }
and Protocol =
    | Http
    | Https
    
type [<RequireQualifiedAccess>] BaseDiscoveryError =
    | FetchError of Exception

type FeedReaderAdapter = 
    {
        discoverFeeds: string -> Task<Result<Result<DiscoveredFeed, FeedError> list, BaseDiscoveryError>>
        getFromUrl: string -> TaskResult<Feed, FeedError>
    }

module FeedItem = 
    open Domain

    let toArticle subscriptionId tenantId (item: Item) = result {
        let! guid = ArticleGuid.create item.Id
        let! timestamp = ArticleTimestamp.create item.Timestamp
        let! link = ArticleLink.create item.Link

        return Article.create item.Title guid subscriptionId tenantId item.Source (Option.defaultValue "" item.Content) item.Summary timestamp link
    }

//TODO: Download with timeout
let downloadBytesAsync (url: string) = Helpers.DownloadBytesAsync(url)
let downloadAsync (repository: HttpCacheRepository) (url: string) =
    let getCacheHeaders = CachedHttpClient.getCacheHeadersImpl repository
    let cacheResponse = CachedHttpClient.cacheResponseImpl repository
    let getContent = repository.getContent
    
    CachedHttpClient.getCached getCacheHeaders cacheResponse getContent url

let createAdapter (getBytesAsync: string -> Task<byte[]>) (getAsync: string -> Task<string>): FeedReaderAdapter =
    let tryOrErrorAsync op errorConstructor arg = task {
        try
            let! result = op arg
            return Ok result
        with
            | r ->
               return Error (errorConstructor r) 
    }

    let tryOrError op errorConstructor arg =
        try 
            Ok (op arg)
        with
        | e -> Error (errorConstructor e)

    let tryDownloadBytesAsync (url: string) = tryOrErrorAsync getBytesAsync FetchError url
    let tryDownloadAsync (url: string) = tryOrErrorAsync getAsync id url
    
    let parseAsync (content: string): Task<Result<Feed, FeedError>> =
        let parseContent (content: string) = FeedReader.ReadFromString(content)
        let tryParseContent (content: string) = tryOrError parseContent ParseError content
        
        let tryDownloadIcon (url: string option) =
            let transformDownloadResult (result: Result<_, _>) =
                match result with
                | Ok r -> Some r
                | Error _ -> None
            let inner (url: string option) =
                let filename (url: string) =
                    let uri = Uri(url)
                    System.IO.Path.GetFileName(uri.LocalPath)
                
                let tryDownloadIconAsync (url: string) =
                    let fileName = filename url
                    
                    tryDownloadBytesAsync url
                    |> TaskResult.map (fun data -> (fileName, data))
                    |> Task.map transformDownloadResult
                
                url
                |> Option.map tryDownloadIconAsync
                |> Option.defaultWith (fun () -> Task.FromResult None)
            tryOrErrorAsync inner FetchError url
            |> Task.map (transformDownloadResult >> Option.flatten)
        
        let mapFeed (feed: CodeHollow.FeedReader.Feed) = task {
                let optionOfNullOrEmpty (s: string) =
                    if String.IsNullOrEmpty(s) then
                        None
                    else
                        Some s
                
                let document =
                    feed.Link
                    |> optionOfNullOrEmpty
                    |> Option.map getAsync
                    
                let! document =
                    match document with
                    | Some t -> t |> Task.map Some
                    | None -> Task.FromResult None
                
                let! faviconUrl =     
                    match document with
                    | Some content ->
                        task {
                            try 
                                let bc = BrowsingContext.New(Configuration.Default)
                                let! document = bc.OpenAsync(System.Action<Io.VirtualResponse>(fun r -> r.Content(content) |> ignore))
                                return document.QuerySelectorAll("link[rel~='icon']")
                                |> List.ofSeq
                                |> List.tryHead
                                |> Option.map (fun e -> e.GetAttribute("href")) //Check for existance
                                |> Option.bind optionOfNullOrEmpty
                            with
                            | _ -> return None
                        }
                    | None ->
                        None
                        |> Task.FromResult
                
                let feedIcon =
                    feed.ImageUrl
                    |> optionOfNullOrEmpty
               
                let defaultFavicon =
                    try 
                        feed.Link
                            |> optionOfNullOrEmpty
                            |> Option.map (fun x -> Uri(x, UriKind.RelativeOrAbsolute))
                            |> Option.map (fun uri -> Uri(uri.GetLeftPart(UriPartial.Authority)))
                            |> Option.map (fun basePath -> Uri(basePath, "/favicon.ico"))
                            |> Option.map (fun path -> path.ToString())
                    with
                    | _ -> None
                
                let! feedIcon =
                    [ faviconUrl; feedIcon; defaultFavicon ]
                    |> List.tryPick id
                    |> tryDownloadIcon
                    
                let mapItem (item: FeedItem) =
                    let extractedTimestamp =
                        match item.SpecificItem with
                        | :? AtomFeedItem as afi ->
                            (* 
                                UpdatedDate is required and PublishingDate is optional in Atom.
                                Also, from the specification about PublishingDate: Contains the time of the initial creation or first availability of the entry. 
                                So for use as a should update existing item or not updatedDate is what we want
                            *)
                            afi.UpdatedDate |> Option.ofNullable
                        | _ -> 
                            item.PublishingDate |> Option.ofNullable

                    let ensureUtcTimestamp (timestamp: DateTime) =
                        if timestamp.Kind <> DateTimeKind.Utc then  
                            failwith "Date should be converted to UTC by FeedReader library"
                        else
                            timestamp

                    let toDateTimeOffset (timestamp: DateTime) =
                        DateTimeOffset(timestamp)

                    let zeroTimeZoneOffset (timestamp: DateTimeOffset) =
                        timestamp.ToUniversalTime()

                    let content = 
                        Option.ofObj item.Content
                        |> Option.orElse (Option.ofObj item.Description)
                    
                    //There might exist some ambiguity between whether to use description or content
                    //https://stackoverflow.com/questions/7220670/difference-between-description-and-contentencoded-tags-in-rss2
                    // and https://validator.w3.org/feed/docs/atom.html#content, let's just solve it empirically
                    let summary = item.Description |> Option.ofObj
                    
                    let source =
                        match item.SpecificItem with
                        | :? Rss20FeedItem as i ->
                            i.Element.ToString()
                        | :? AtomFeedItem as i ->
                            i.Element.ToString()
                        | :? MediaRssFeedItem  as i ->
                            i.Element.ToString()
                        | _ ->
                            failwithf "Unsupported feed type as of yet %A" (item.SpecificItem.GetType())
                    
                    { 
                        Item.Title = item.Title
                        Id = item.Id
                        Content = content
                        Source = source
                        Summary = summary
                        Timestamp = extractedTimestamp |> Option.map (ensureUtcTimestamp >> toDateTimeOffset >> zeroTimeZoneOffset)
                        Link = Some item.Link
                    }

                
                
                let items = 
                    feed.Items 
                    |> Seq.map mapItem
                    |> List.ofSeq

                //TODO: parse publishingDate/LastBuildDate for RSS feeds and UpdatedDate for Atom feeds. Can probably be used to skip item checking
                
                let _type =
                    match feed.Type with
                    | CodeHollow.FeedReader.FeedType.Atom -> FeedType.Atom
                    | CodeHollow.FeedReader.FeedType.Rss 
                    | CodeHollow.FeedReader.FeedType.Rss_0_91
                    | CodeHollow.FeedReader.FeedType.Rss_0_92
                    | CodeHollow.FeedReader.FeedType.Rss_1_0
                    | CodeHollow.FeedReader.FeedType.MediaRss
                    | CodeHollow.FeedReader.FeedType.Rss_2_0 -> FeedType.Rss
                    | _ -> failwith "Unknown feed type"
                
                return { 
                    Title = feed.Title
                    Icon = feedIcon
                    Description = feed.Description
                    Items = items
                    Type = _type
                }
            }
            
        let parsedBytes = 
            content
            |> tryParseContent
            
        match parsedBytes with
        | Ok b ->
            task {
                let! x = mapFeed b
                return Ok x
            }
        | Error e -> Task.FromResult (Error e)
        
    let fetch (url: string): TaskResult<Feed, FeedError> = 
        task {
            let! result = tryDownloadAsync url
            match result with
            | Ok result ->
                let! result = parseAsync result
                return result
            | Error e ->
                return Error (FeedError.FetchError e)
        }
       
    let discoverFeeds (baseUrl: string): Task<Result<Result<DiscoveredFeed, FeedError> list, BaseDiscoveryError>> =
        task {
            let! content = tryDownloadAsync baseUrl
           
            let toDiscoveredFeed (url: string) (feed: Feed): DiscoveredFeed =
                {
                    Url = url
                    Title = feed.Title
                    FeedType = feed.Type
                    Icon = feed.Icon
                    Protocol =
                        let url = Uri(url)
                        if url.Scheme = "http" then
                            Protocol.Http
                        else
                            Protocol.Https
                }
                
            let x = 
                match content with
                | Ok content ->
                    let urls = FeedReader.ParseFeedUrlsFromHtml(content) |> Seq.toList
                    match urls with
                    | [] ->
                        task {
                            let! feed = parseAsync content
                            let result: Result<Result<DiscoveredFeed, FeedError> list, _> =  
                                match feed with
                                | Ok f -> Ok [ Ok  (toDiscoveredFeed baseUrl f) ]
                                | Error e -> Ok [ Error  e ]
                                
                            return result
                        }
                    | urls ->
                        task {
                            let mutable items = []
                            for u in urls do
                                let uri = Uri(u.Url, UriKind.RelativeOrAbsolute)
                                let path = if uri.IsAbsoluteUri then u.Url else Uri(Uri(baseUrl), u.Url).ToString()
                                let! x = fetch path |> TaskResult.map (fun f -> (path, f))
                                items <- x::items
                                
                            let urls = items 
                            let urls = urls |> List.map (Result.map (fun (url, feed) -> toDiscoveredFeed url feed))
                            return (Ok urls)
                        }
   
                | Error exn ->
                    BaseDiscoveryError.FetchError exn
                    |> Error
                    |> Task.FromResult
                    
            return! x
        }
    
    {
        getFromUrl = fetch
        discoverFeeds = discoverFeeds
    }
    
