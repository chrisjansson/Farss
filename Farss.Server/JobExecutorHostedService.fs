module JobExecutorHostedService

open Farss.Server.BackgroundTaskQueue
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

type JobExecutorHostedService
    (
        serviceScopeFactory: IServiceScopeFactory,
        taskQueue: IBackgroundTaskQueue,
        logger: ILogger<JobExecutorHostedService>
    ) =

    inherit BackgroundService()

    override this.ExecuteAsync(ct) =
        task {
            while (not ct.IsCancellationRequested) do
                let! subscriptionId = taskQueue.DequeuePollArticlesForSubscription(ct)

                let work () =
                    task {
                        //TODO: UoW
                        use scope = serviceScopeFactory.CreateAsyncScope()
                        let job = FetchArticlesHandler.runFetchArticlesForSubscription scope.ServiceProvider
                        let! _ = job subscriptionId
                        return ()
                    }

                try
                    do! work ()
                with exn ->
                    logger.LogError(exn, "Error running job")
        }
