module DeleteSubscriptionWorkflow

open Persistence
open Dto
open Domain

let convertToWorkflowError e: WorkflowError =
    InvalidParameter [e]
    
let deleteSubscription (repository: SubscriptionRepository) (dto: DeleteSubscriptionDto) =
    let command = DeleteSubscriptionDto.toCommand dto

    let deleteSubscription (command: DeleteSubscriptionCommand) =
        repository.delete command.Id

    command 
        |> Result.map deleteSubscription
        |> Result.mapError convertToWorkflowError