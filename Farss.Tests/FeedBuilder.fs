module FeedBuilder 

open System.Xml
open Microsoft.SyndicationFeed
open Microsoft.SyndicationFeed.Rss

[<RequireQualifiedAccess>]
module Types =
    type Feed = { 
        Title: string
        Description: string option
        Items:  Item list }
    and Item = { Title: string; Id: string option }

type StringWriterWithEncoding(encoding: System.Text.Encoding) =
    inherit System.IO.StringWriter()

    override this.Encoding with get() = encoding

let feedItem (title: string) = SyndicationItem(Title = title)

let feedItem2 (title: string) = 
    { Types.Item.Title = title; Types.Item.Id = None }

let withId (id: string) (item: Types.Item) = 
    { item with Types.Item.Id = Some id }

let withDescription (description: string) (item: SyndicationItem) =
    item.Description <- description
    item

let withDescription2 (description: string) (feed: Types.Feed) =
    { feed with Types.Feed.Description = Some description }

let withItem (item: Types.Item) (feed: Types.Feed) =
    { feed with Types.Feed.Items = feed.Items @ [item] }

let feed (title: string) =  { Types.Feed.Title = title; Types.Feed.Items = []; Types.Feed.Description = None }

let toRss2 (feed: Types.Feed) = 
    let sw = new StringWriterWithEncoding(System.Text.Encoding.UTF8);
    use xmlWriter = XmlWriter.Create(sw, XmlWriterSettings(Async=true))
    let writer = RssFeedWriter(xmlWriter)
    writer.WriteTitle(feed.Title).Wait()
    if feed.Description.IsSome then do
        writer.WriteDescription(feed.Description.Value).Wait()
    for item in feed.Items do
        let feedItem = SyndicationItem()
        feedItem.Title <- item.Title
        // Not setting a guid is not great
        if item.Id.IsSome then do
            feedItem.Id <- item.Id.Value
    
        writer.Write(feedItem).Wait()
    (writer.Flush()).Wait()
    xmlWriter.Flush()
    xmlWriter.Dispose()
    sw.ToString()


let toRss (feed: SyndicationItem) = 
    let sw = new StringWriterWithEncoding(System.Text.Encoding.UTF8);
    use xmlWriter = XmlWriter.Create(sw, XmlWriterSettings(Async=true))
    let writer = RssFeedWriter(xmlWriter)
    (writer.Write(feed)).Wait()
    (writer.Flush()).Wait()
    xmlWriter.Flush()
    xmlWriter.Dispose()
    sw.ToString()
