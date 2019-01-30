module FeedBuilder 

open System.Xml
open Microsoft.SyndicationFeed
open Microsoft.SyndicationFeed.Rss
open Microsoft.SyndicationFeed.Atom
open System

[<RequireQualifiedAccess>]
module Types =
    type Feed = { 
        Title: string
        Description: string option
        Items:  Item list }
    and Item = { 
        Title: string; 
        Id: string option; 
        Content: string option; 
        PublishingDate: DateTimeOffset option
        UpdatedDate: DateTimeOffset option
        Link: string option }

type StringWriterWithEncoding(encoding: System.Text.Encoding) =
    inherit System.IO.StringWriter()

    override this.Encoding with get() = encoding

let feedItem (title: string) = SyndicationItem(Title = title)

let feedItem2 (title: string) = 
    { Types.Item.Title = title; Types.Item.Id = None; Types.Item.Content = None; Types.Item.PublishingDate = None; Types.Item.UpdatedDate = None; Types.Item.Link = None }

let withId (id: string) (item: Types.Item) = 
    { item with Types.Item.Id = Some id }
    
let withContent (content: string) (item: Types.Item) = 
    { item with Types.Item.Content = Some content }

let withPublishingDate (publishingDate: DateTimeOffset) (item: Types.Item) =
    { item with Types.Item.PublishingDate = Some publishingDate }

let withUpdatedDate (updatedDate: DateTimeOffset) (item: Types.Item) =
    { item with Types.Item.UpdatedDate = Some updatedDate }

let withLink (link: string) (item: Types.Item) =
    { item with Types.Item.Link = Some link }

let withDescription (description: string) (item: SyndicationItem) =
    item.Description <- description
    item

let withDescription2 (description: string) (feed: Types.Feed) =
    { feed with Types.Feed.Description = Some description }

let withItem (item: Types.Item) (feed: Types.Feed) =
    { feed with Types.Feed.Items = feed.Items @ [item] }

let feed (title: string) =  { Types.Feed.Title = title; Types.Feed.Items = []; Types.Feed.Description = None }

let toAtom (feed: Types.Feed) = 
    //let sw = new StringWriterWithEncoding(System.Text.Encoding.UTF8);
    //use xmlWriter = XmlWriter.Create(sw, XmlWriterSettings(Async=true))
    //let writer = RssFeedWriter(xmlWriter)
    //let ExampleNs = "http://contoso.com/syndication/feed/examples";
    //let attributes = [| new SyndicationAttribute("xmlns:example", ExampleNs) :> ISyndicationAttribute |]
    //let formatter = new RssFormatter(attributes, xmlWriter.Settings);
    //writer.WriteTitle(feed.Title).Wait()
    //if feed.Description.IsSome then do
    //    writer.WriteDescription(feed.Description.Value).Wait()
    //for item in feed.Items do
    //    let feedItem = SyndicationItem()
    //    feedItem.Title <- item.Title
    //    // Not setting a guid is not great
    //    if item.Id.IsSome then do
    //        feedItem.Id <- item.Id.Value

    //    let content = new SyndicationContent(formatter.CreateContent(feedItem));

    //    if item.Content.IsSome then do
    //        content.AddField(SyndicationContent("encoded", item.Content.Value))
    //    writer.Write(content).Wait()
    //(writer.Flush()).Wait()
    //xmlWriter.Flush()
    //xmlWriter.Dispose()
    //failwith (sw.ToString())
    //sw.ToString()

    let sw = new StringWriterWithEncoding(System.Text.Encoding.UTF8);
    use xmlWriter = XmlWriter.Create(sw, XmlWriterSettings(Async=true))
    
    let writer = AtomFeedWriter(xmlWriter)

    writer.WriteTitle(feed.Title).Wait()
    writer.Write(SyndicationLink(Uri("http://uriurirui")) :> ISyndicationLink).Wait()

    if feed.Description.IsSome then do
        writer.WriteSubtitle(feed.Description.Value).Wait()
    writer.WriteId(Guid.NewGuid().ToString()).Wait()
    writer.WriteUpdated(DateTimeOffset.Now).Wait()

    for item in feed.Items do
        let feedItem = AtomEntry()
        feedItem.Title <- item.Title
        // Not setting a guid is not great
        if item.Id.IsSome then do
            feedItem.Id <- item.Id.Value
        else do
            feedItem.Id <- Guid.NewGuid().ToString()
        if item.Content.IsSome then do
            feedItem.Description <- item.Content.Value
        if item.PublishingDate.IsSome then do
            feedItem.Published <- item.PublishingDate.Value
            
        if item.Link.IsSome then do
            feedItem.AddLink(SyndicationLink(Uri(item.Link.Value)))
        else 
            feedItem.AddLink(SyndicationLink(Uri("http://builder_link")))
        feedItem.LastUpdated <- DateTimeOffset.Now
        if item.UpdatedDate.IsSome then do
            feedItem.LastUpdated <- item.UpdatedDate.Value
        else do
            feedItem.LastUpdated <- DateTimeOffset.Now
        feedItem.AddContributor(SyndicationPerson("temp", "tempemail"))
        writer.Write(feedItem).Wait()

    (writer.Flush()).Wait()
    xmlWriter.Flush()
    xmlWriter.Dispose()
    //failwith (sw.ToString())

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
