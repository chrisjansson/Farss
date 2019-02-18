module AddSubscriptionModel

type Model = 
    | EnterFeedUrl of EnterFeedUrlModel
    | LoadingPreview of url: string
    | PreviewSubscription of url: string * title: string
    //| PreviewFeedFailed of url: string * error: string    
and EnterFeedUrlModel = 
    { 
        Url: string
        Error: string option
    }

type Message =
    | EditUrl of string
    | PreviewSubscription
    | SubscriptionPreviewReceived of Result<Dto.PreviewSubscribeToFeedResponseDto, string>
    | Ignore