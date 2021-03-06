﻿module FetchArticlesHostedService

open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open System
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks

let Interval = 10 * 60 * 1000

//todo: Add durability
type FetchArticlesHostedService(serviceProvider: IServiceProvider) =
    inherit BackgroundService()

    override this.ExecuteAsync(cancellationToken) =
        task {
            while (not cancellationToken.IsCancellationRequested) do

                do! using(serviceProvider.CreateScope()) (fun scope -> task {
                    let scopedServiceProvider = scope.ServiceProvider
                    let fetchEntries = FetchArticlesHandler.constructFetchEntriesHandler scopedServiceProvider

                    let! x = fetchEntries ()
                    x |> ignore
                })
                
                do! Task.Delay(Interval, cancellationToken)

            return ()
        }
        
    