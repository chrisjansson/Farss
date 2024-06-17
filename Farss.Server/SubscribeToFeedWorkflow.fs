module SubscribeToFeedWorkflow

open System
open System.Security.Cryptography
open System.Threading.Tasks
open Persistence
open FeedReaderAdapter
open Dto

let private convertToWorkflowError r : Result<_, WorkflowError> =
    match r with
    | Ok r -> Ok r
    | Error(BaseDiscoveryError.FetchError e) -> BadRequest(e.Message, Some e) |> Error

let previewSubscribeToFeed
    (feedReader: FeedReaderAdapter)
    (query: PreviewSubscribeToFeedQueryDto)
    : Task<Result<Result<PreviewSubscribeToFeedResponseDto, FeedError> list, WorkflowError>> =
    let aggregateResults
        (results: Result<DiscoveredFeed, _> list)
        : Result<PreviewSubscribeToFeedResponseDto, FeedError> list =
        [
            for r in results do
                match r with
                | Ok feed ->
                    Ok {
                        Title = feed.Title
                        Url = feed.Url
                        Type =
                            match feed.FeedType with
                            | FeedReaderAdapter.FeedType.Atom -> FeedType.Atom
                            | FeedReaderAdapter.FeedType.Rss -> FeedType.Rss
                        Protocol =
                            match feed.Protocol with
                            | FeedReaderAdapter.Protocol.Http -> Http
                            | FeedReaderAdapter.Protocol.Https -> Https
                        Icon = feed.Icon
                    }
                | Error feedError ->
                    match feedError with
                    | FeedReaderAdapter.FeedError.FetchError e -> Error(FetchError (string e))
                    | FeedReaderAdapter.FeedError.ParseError e -> Error(ParseError (string e))

        ]

    feedReader.discoverFeeds query.Url
    |> TaskResult.map aggregateResults
    |> Task.map convertToWorkflowError

type SubscribeToFeedError =
    | FeedError of FeedReaderAdapter.FeedError
    | SubscriptionError of string list

let private convertToWorkflowError2 r : Result<_, WorkflowError> =
    match r with
    | Ok r -> Ok r
    | Error(FeedError(FeedReaderAdapter.FetchError e)) -> BadRequest(e.Message, Some e) |> Error
    | Error(FeedError(FeedReaderAdapter.ParseError e)) -> BadRequest(e.Message, Some e) |> Error
    | Error(SubscriptionError errors) -> BadRequest(sprintf "%A" errors, None) |> Error

let subscribeToFeed (feedReader: FeedReaderAdapter) (repository: SubscriptionRepository) (command: SubscribeToFeedDto) =
    let getFromUrl url =
        feedReader.getFromUrl url
        |> Task.map (Result.mapError SubscribeToFeedError.FeedError)

    let createSubscription (command: SubscribeToFeedDto) _ =
        result {
            let! title =
                Domain.SubscriptionTitle.create command.Title
                |> Result.mapError (fun e -> SubscriptionError [ e ])

            return Domain.Subscription.create command.Url title
        }

    let saveSubscription = repository.save

    command.Url
    |> getFromUrl
    |> TaskResult.bind (createSubscription command)
    |> TaskResult.tee saveSubscription
    |> Task.map convertToWorkflowError2

let updateFeedIcon
    (feedReader: FeedReaderAdapter)
    (repository: BackendSubscriptionRepository)
    (fileRepository: FileRepository)
    (subscriptionId: Domain.SubscriptionId)
    =
    let getFeedForSubscription (subscription: Domain.TenantedSubscription) =
        feedReader.getFromUrl subscription.Url
        |> TaskResult.map (fun f -> subscription, f)

    let updateIcon (subscription: Domain.TenantedSubscription, feed: Feed) =
        let hashData (iconData: byte[]) = SHA256.HashData(iconData)

        let createFileAndSaveFile (s, icon, iconHash) =
            let inner (fn, data, dataHash) : Domain.File = {
                Id = Guid.NewGuid()
                FileName = fn
                FileOwner = Domain.FileOwner.Feed
                Hash = dataHash
                Data = data
            }

            let icon = inner (s, icon, iconHash)
            fileRepository.save icon
            icon

        let changeIcon (subscription: Domain.TenantedSubscription, file: Domain.File option) = {
            subscription with
                Icon = file |> Option.map (fun file -> file.Id)
        }

        let feedIcon =
            feed.Icon |> Option.map (fun (name, data) -> (name, data, hashData data))

        let getOrSaveIcon (existingIcon: Guid option, newIcon: (string * byte[] * byte[]) option) =
            let shouldReplaceIcon =
                match (existingIcon, newIcon) with
                | None, None -> false
                | Some iconId, Some(_, _, newIconHash) ->
                    let icon = fileRepository.get iconId
                    icon.Hash <> newIconHash
                | _ -> true

            if shouldReplaceIcon && existingIcon.IsSome then
                fileRepository.delete existingIcon.Value

            match newIcon with
            | Some icon ->
                let savedIcon = createFileAndSaveFile icon
                Some savedIcon
            | _ -> None

        subscription
        |> (fun s -> s, getOrSaveIcon (s.Icon, feedIcon))
        |> changeIcon


    subscriptionId
    |> repository.get
    |> getFeedForSubscription
    |> TaskResult.map updateIcon
    |> TaskResult.tee repository.save
