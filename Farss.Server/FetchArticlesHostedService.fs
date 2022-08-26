module FetchArticlesHostedService

open Farss.Server.BackgroundTaskQueue
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open System
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks
open Microsoft.Extensions.Logging

let interval = TimeSpan.FromMinutes(10)

type QueueFetchArticles(
    taskQueue: IBackgroundTaskQueue) =
    inherit BackgroundService()

    override this.ExecuteAsync(ct) =
        task {
            while (not ct.IsCancellationRequested) do
                do! taskQueue.QueuePollArticles(QueueReason.Poll)
                do! Task.Delay(interval)
        }
        
type FetchArticlesHostedService(
    serviceProvider: IServiceProvider,
    logger: ILogger<FetchArticlesHostedService>,
    taskQueue: IBackgroundTaskQueue) =
    inherit BackgroundService()

    override this.ExecuteAsync(ct) =
        task {
            
            let mutable lastPoll = None
            
            while (not ct.IsCancellationRequested) do
                
                let! queueReason = taskQueue.DequeuePollArticles(ct)
                
                let now = DateTimeOffset.UtcNow
                let minimumInterval = TimeSpan.FromSeconds 30
                
                let shouldPoll =
                    match queueReason with
                    | QueueReason.Poll ->
                        match lastPoll with
                        | Some lp when ((now - lp) > minimumInterval) -> true
                        | None -> true
                        | _ -> false
                    | QueueReason.Trigger -> true
                    
                lastPoll <- Some now
                
                if shouldPoll then
                    do! using(serviceProvider.CreateScope()) (fun scope -> task {
                        let scopedServiceProvider = scope.ServiceProvider
                        let fetchEntries = FetchArticlesHandler.constructFetchEntriesHandler scopedServiceProvider

                        let! x = fetchEntries ()
                        x |> ignore
                    })
                else
                    ()

            return ()
        }
        
    