module Farss.Server.BackgroundTaskQueue

open System.Threading
open System.Threading.Channels
open System.Threading.Tasks


type QueueReason =
    | Poll
    | Trigger

type IBackgroundTaskQueue =
    abstract member QueuePollArticles: QueueReason -> ValueTask
    abstract member DequeuePollArticles: CancellationToken -> ValueTask<QueueReason>

type BackgroundTaskQueue() =
    let queue: Channel<_> =
        let options = BoundedChannelOptions(100)
        Channel.CreateBounded<_>(options)
        
    interface IBackgroundTaskQueue with
        member this.QueuePollArticles(fetchReason) =
            queue.Writer.WriteAsync((fetchReason))
        member this.DequeuePollArticles(ct) =
            queue.Reader.ReadAsync(ct)
        