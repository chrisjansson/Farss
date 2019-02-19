module AddSubscriptionModel


//Idea, loading as a continuation instead of an explicit number of loading states?
// Loading of next: 'msg -> 'model, 'cmd //For every message evaluate if we should stay or progress to another state

//Another is to simple fold loading into a boolean like error instead of an explicit state?
type Model = AddSubscriptionModal option
and AddSubscriptionModal = 
    | EnterFeedUrl of EnterFeedUrlModel
    | LoadingPreview of EnterFeedUrlModel
    | PreviewSubscription of PreviewSubscriptionModel
    | LoadingSubscribe of PreviewSubscriptionModel

and EnterFeedUrlModel = 
    { 
        Url: string
        Error: string option
    }
and PreviewSubscriptionModel = 
    {
        Url: string
        Title: string
        Error: string option
    }

type Message =
    | EditUrl of string
    | PreviewSubscription
    | SubscriptionPreviewReceived of Result<Dto.PreviewSubscribeToFeedResponseDto, string>
    | SubscribeToFeedReceived of Result<unit, string>
    | Close
    | Subscribe
    | EditTitle of string
    | Ignore