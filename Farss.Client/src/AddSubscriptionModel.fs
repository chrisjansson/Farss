module AddSubscriptionModel

type Model = 
    | EnterFeedUrl of EnterFeedUrlModel
    | LoadingPreview of url: string
    | PreviewSubscription of url: string * title: string
    | PreviewFeedFailed
and EnterFeedUrlModel = { Url: string }

type Message =
    | EditUrl of string
    | PreviewSubscription
    | SubscriptionPreviewReceived of Result<Dto.PreviewSubscribeToFeedResponseDto, string>
    | Ignore