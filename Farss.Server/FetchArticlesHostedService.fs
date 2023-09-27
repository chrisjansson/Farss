module FetchArticlesHostedService

open System.Threading
open Farss.Server.BackgroundTaskQueue
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

let interval = TimeSpan.FromMinutes(10)

type QueueFetchArticles(taskQueue: IBackgroundTaskQueue) =
    inherit BackgroundService()

    override this.ExecuteAsync(ct) =
        task {
            while (not ct.IsCancellationRequested) do
                do! taskQueue.QueuePollArticles(QueueReason.Poll)
                do! Task.Delay(interval)
        }

type FetchArticlesHostedService
    (serviceProvider: IServiceProvider, logger: ILogger<FetchArticlesHostedService>, taskQueue: IBackgroundTaskQueue) =
    inherit BackgroundService()

    override this.ExecuteAsync(ct) =
        task {

            let mutable lastPoll = None

            while (not ct.IsCancellationRequested) do

                let! queueReason = taskQueue.DequeuePollArticles(ct)

                let now = DateTimeOffset.UtcNow
                let minimumInterval = TimeSpan.FromSeconds 1

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
                    do!
                        task {
                            use scope = serviceProvider.CreateScope()
                            let scopedServiceProvider = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>()

                            try
                                use cts = new CancellationTokenSource()

                                let fetchEntries =
                                    FetchArticlesHandler.constructFetchEntriesHandler scopedServiceProvider

                                do! fetchEntries cts.Token
                            with exn ->
                                logger.LogError(exn, "Error updating feed articles")

                            try
                                logger.LogInformation "Updating subscription icons"

                                let updateIcons =
                                    FetchArticlesHandler.constructUpdateIconsHandler scopedServiceProvider

                                do! updateIcons ()
                                logger.LogInformation "Updated dubscription icons"
                            with exn ->
                                logger.LogError(exn, "Error updating feed icon")
                        }
                else
                    ()

            return ()
        }
