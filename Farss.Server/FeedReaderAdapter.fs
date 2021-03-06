module FeedReaderAdapter

open CodeHollow.FeedReader
open System
open CodeHollow.FeedReader.Feeds

type  FeedError =
    | FetchError of Exception
    | ParseError of Exception

type Feed = 
    { 
        Title: string
        Icon: (string * byte[]) option
        Description: string
        Items: Item list
    }
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

type FeedReaderAdapter = 
    {
        getFromUrl: string -> AsyncResult<Feed, FeedError>
    }


module FeedItem = 
    open Domain

    let toArticle subscriptionId (item: Item) = result {
        let! guid = ArticleGuid.create item.Id
        let! timestamp = ArticleTimestamp.create item.Timestamp
        let! link = ArticleLink.create item.Link

        return Article.create item.Title guid subscriptionId item.Source (Option.defaultValue "" item.Content) item.Summary timestamp link
    }

//TODO: Download with timeout
let downloadBytesAsync (url: string) = Helpers.DownloadBytesAsync(url) |> Async.AwaitTask

let createAdapter (getBytesAsync: string -> Async<byte[]>): FeedReaderAdapter =
    let tryOrErrorAsync op errorConstructor arg = async {
        match! (Async.Catch (op arg)) with
        | Choice1Of2 r -> return Ok r
        | Choice2Of2 r -> return Error (errorConstructor r)
    }

    let tryOrError op errorConstructor arg =
        try 
            Ok (op arg)
        with
        | e -> Error (errorConstructor e)

    let fetch (url: string): AsyncResult<Feed, FeedError> = 
        let tryDownloadBytesAsync (url: string) = tryOrErrorAsync getBytesAsync FetchError url
        let parseBytes (bytes: byte[]) = FeedReader.ReadFromByteArray(bytes)
        let tryParseBytes (bytes: byte[]) = tryOrError parseBytes ParseError bytes
        
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
                    |> AsyncResult.mapResult (fun data -> (fileName, data))
                    |> Async.map transformDownloadResult
                
                url
                |> Option.map tryDownloadIconAsync
                |> Option.defaultWith (fun () -> async.Return None)
            tryOrErrorAsync inner FetchError url
            |> Async.map (transformDownloadResult >> Option.flatten)
        
        let mapFeed (feed: CodeHollow.FeedReader.Feed) = async {
                let optionOfNullOrEmpty (s: string) =
                    if String.IsNullOrEmpty(s) then
                        None
                    else
                        Some s
                
                let! feedIcon =
                    feed.ImageUrl
                    |> optionOfNullOrEmpty
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
                        | _ -> failwith "Unsupported feed type as of yet"
                            
                    
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
                return { 
                    Title = feed.Title
                    Icon = feedIcon
                    Description = feed.Description
                    Items = items
                }
            }

        tryDownloadBytesAsync url 
        |> AsyncResult.bind tryParseBytes
        |> AsyncResult.bindAsync mapFeed

    {
        getFromUrl = fetch
    }
