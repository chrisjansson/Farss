module AddSubscriptionModel

type Model = 
    | EnterFeedUrl of EnterFeedUrlModel
    | LoadingPreview
    | PreviewSubscription
    | PreviewFeedFailed
and EnterFeedUrlModel = { Url: string }

type Message =
    | EditUrl of string
    | PreviewSubscription
    | SubscriptionPreviewReceived of Result<Dto.PreviewSubscribeToFeedResponseDto, string>
    | Ignore