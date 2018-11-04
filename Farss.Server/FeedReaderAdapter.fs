module FeedReaderAdapter

open CodeHollow.FeedReader

type FeedReaderAdapter = 
    {
        getFromUrl: string -> Feed
    }
