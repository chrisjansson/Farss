module FeedBuilder 

open System.Xml
open Microsoft.SyndicationFeed
open Microsoft.SyndicationFeed.Rss

type StringWriterWithEncoding(encoding: System.Text.Encoding) =
    inherit System.IO.StringWriter()

    override this.Encoding with get() = encoding

let feed (title: string) = SyndicationItem(Title = title)

let withDescription (description: string) (item: SyndicationItem) =
    item.Description <- description
    item

//let feedItem (atr: string) (feed: SyndicationItem) =


let toRss (feed: SyndicationItem) = 
    let sw = new StringWriterWithEncoding(System.Text.Encoding.UTF8);
    use xmlWriter = XmlWriter.Create(sw, XmlWriterSettings(Async=true))
    let writer = RssFeedWriter(xmlWriter)
    (writer.Write(feed)).Wait()
    (writer.Flush()).Wait()
    xmlWriter.Flush()
    xmlWriter.Dispose()
    failwith <| sw.ToString()
