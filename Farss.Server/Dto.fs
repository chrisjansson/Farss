module Dto

open System

type SubscriptionDto =
    {
        Id: Guid
        Url: string
    }

module SubscriptionDto = 
    let toDto (feed: Domain.Feed): SubscriptionDto = { Id = feed.Id; Url = feed.Url }

type DeleteSubscriptionDto = { Id: Nullable<Guid> }

module DeleteSubscriptionDto =
    open Domain

    let toCommand (dto: DeleteSubscriptionDto): Result<DeleteSubscriptionCommand, unit> =
        if dto.Id.HasValue then
            { DeleteSubscriptionCommand.Id = dto.Id.Value } |> Ok
        else 
            Error ()
        


        //todo: domain dto workflow in what order, what part of workflow is part of domain etc?