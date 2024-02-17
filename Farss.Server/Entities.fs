module Entities

open System
open Domain

[<AllowNullLiteral>]
type PersistedSubscription() =
    member val Id: Guid = Unchecked.defaultof<_> with get, set
    member val Url: string = Unchecked.defaultof<_> with get, set
    member val Title: string = Unchecked.defaultof<_> with get, set
    member val IconId: Nullable<Guid> = Unchecked.defaultof<_> with get, set
    member val Articles: ResizeArray<PersistedArticle> = ResizeArray<_>() with get, set

and [<AllowNullLiteral>] PersistedSubscriptionLogEntry() =
    member val Id: Guid = Unchecked.defaultof<_> with get, set
    member val SubscriptionId: Guid = Unchecked.defaultof<_> with get, set
    member val Subscription: PersistedSubscription = Unchecked.defaultof<_> with get, set
    member val Success: bool = Unchecked.defaultof<_> with get, set
    member val Message: string = Unchecked.defaultof<_> with get, set
    member val Timestamp: DateTimeOffset = Unchecked.defaultof<_> with get, set

and [<AllowNullLiteral>] PersistedArticle() =
    member val Id: Guid = Unchecked.defaultof<_> with get, set
    member val Title: string = Unchecked.defaultof<_> with get, set
    member val Guid: string = Unchecked.defaultof<_> with get, set
    member val SubscriptionId: Guid = Unchecked.defaultof<_> with get, set
    member val Subscription: PersistedSubscription = Unchecked.defaultof<_> with get, set
    member val Content: string = Unchecked.defaultof<_> with get, set
    member val Summary: string = Unchecked.defaultof<_> with get, set
    member val Source: string = Unchecked.defaultof<_> with get, set
    member val IsRead: bool = Unchecked.defaultof<_> with get, set
    member val Timestamp: DateTimeOffset = Unchecked.defaultof<_> with get, set
    member val Link: string = Unchecked.defaultof<_> with get, set

[<AllowNullLiteral>]
type PersistedFile() =
    member val Id: Guid = Unchecked.defaultof<_> with get, set
    member val FileName: string = Unchecked.defaultof<_> with get, set
    member val FileOwner: FileOwner = Unchecked.defaultof<_> with get, set
    member val Data: byte[] = Unchecked.defaultof<_> with get, set
    member val Hash: byte[] = Unchecked.defaultof<_> with get, set

[<AllowNullLiteral>]
type PersistedHttpCacheEntry() =
    member val Id: Guid = Unchecked.defaultof<_> with get, set
    member val Url: string = Unchecked.defaultof<_> with get, set
    member val Content: string = Unchecked.defaultof<_> with get, set
    member val ETag: string = Unchecked.defaultof<_> with get, set
    member val LastModifiedDate: Nullable<DateTimeOffset> = Unchecked.defaultof<_> with get, set
    member val LastGet: DateTimeOffset = Unchecked.defaultof<_> with get, set

