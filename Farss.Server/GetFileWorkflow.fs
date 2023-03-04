[<RequireQualifiedAccess>]
module GetFileWorkflow

open System
open Persistence

type GetFileWorkflow = Dto.GetFileDto -> Result<FileDto, WorkflowError>
type Factory = FileRepository -> GetFileWorkflow

let impl: Factory =
    fun fr command ->
        let getFile (fileId: Guid) =
            fr.get fileId

        command
            |> getFile c.Id
            