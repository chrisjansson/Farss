module UpdateIconsHandler

open System.Threading.Tasks
open Domain
open Persistence
open FeedReaderAdapter
open Microsoft.Extensions.DependencyInjection
open System

let private updateFeedIconForSubscriptionHandler (serviceProvider: IServiceProvider): SubscriptionId -> Task<_> =
    fun id ->
        task {
            //todo: uow
            use scope = serviceProvider.CreateScope()
            let services = scope.ServiceProvider
            let adapter = services.GetService<FeedReaderAdapter>()
            let subscriptionRepository = services.GetService<SubscriptionRepository>()
            let fileRepository = services.GetService<FileRepository>()
            return! SubscribeToFeedWorkflow.updateFeedIcon adapter subscriptionRepository fileRepository id
        }

let updateFeedIconsHandler (serviceProvider: IServiceScopeFactory) =
    let run () =
        task {
            use scope = serviceProvider.CreateScope()
            let services = scope.ServiceProvider
            let subscriptionRepository = services.GetService<SubscriptionRepository>()
            let subs = subscriptionRepository.getAll () |> List.map (fun x -> x.Id)

            for s in subs do
                let! _ = updateFeedIconForSubscriptionHandler services s
                ()

            return ()
        }

    run
