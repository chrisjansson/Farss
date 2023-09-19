module ApiUrls

open System

[<Literal>]
let SubscribeToFeed = "/api/feeds"

[<Literal>]
let PreviewSubscribeToFeed = "/api/previewsubscribe"

[<Literal>]
let GetSubscriptions = "/api/feeds"

[<Literal>]
let DeleteSubscription = "/api/subscription/delete"

[<Literal>]
let PollSubscriptions = "/api/poll"

[<Literal>]
let GetArticles = "/api/articles"

[<Literal>]
let SetArticleReadStatus = "/api/article/setreadstatus"

let GetFileRoute = "/api/file/{id}"

let GetFile (id: Guid) = sprintf "/api/file/%A" id
