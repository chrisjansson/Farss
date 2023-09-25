module Farss.Server.BackgroundTaskQueue

open System.Threading
open System.Threading.Channels
open System.Threading.Tasks
open Domain


type QueueReason =
    | Poll
    | Trigger

type IBackgroundTaskQueue =
    abstract member QueuePollArticles: QueueReason -> ValueTask
    abstract member DequeuePollArticles: CancellationToken -> ValueTask<QueueReason>
    abstract member DequeuePollArticlesForSubscription: CancellationToken -> ValueTask<SubscriptionId>
    abstract member QueuePollArticlesForSubscription: SubscriptionId * CancellationToken -> ValueTask

type BackgroundTaskQueue() =
    let queue: Channel<_> =
        let options = BoundedChannelOptions(100)
        Channel.CreateBounded<_>(options)
        
    let pollArticlesQueue: Channel<_> =
        let options = BoundedChannelOptions(100)
        Channel.CreateBounded<_>(options)
        
    interface IBackgroundTaskQueue with
        member this.QueuePollArticles(fetchReason) =
            queue.Writer.WriteAsync((fetchReason))
        member this.DequeuePollArticles(ct) =
            queue.Reader.ReadAsync(ct)

        member this.DequeuePollArticlesForSubscription(ct) =
            pollArticlesQueue.Reader.ReadAsync(ct)
            
        member this.QueuePollArticlesForSubscription(subscriptionId, ct) =
            pollArticlesQueue.Writer.WriteAsync(subscriptionId, ct)
        
        
