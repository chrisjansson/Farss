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
        Description: string
        Items: Item list
    }
and Item = 
    {
        Title: string
        Id: string
        Content: string
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

        return Article.create item.Title guid subscriptionId item.Content timestamp link
    }

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

        let mapFeed (feed: CodeHollow.FeedReader.Feed) = 
        
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

                { 
                    Item.Title = item.Title
                    Id = item.Id
                    Content = item.Content
                    Timestamp = extractedTimestamp |> Option.map (ensureUtcTimestamp >> toDateTimeOffset >> zeroTimeZoneOffset)
                    Link = Some item.Link
                }

            let items = 
                feed.Items 
                |> Seq.map mapItem
                |> List.ofSeq

            //TODO: parse publishingDate/LastBuildDate for RSS feeds and UpdatedDate for Atom feeds. Can probably be used to skip item checking
            { 
                Title = feed.Title 
                Description = feed.Description
                Items = items
            }

        tryDownloadBytesAsync url 
        |> AsyncResult.bind tryParseBytes
        |> AsyncResult.mapResult mapFeed

    {
        getFromUrl = fetch
    }
