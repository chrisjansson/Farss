[<RequireQualifiedAccess>]
module GetFileWorkflow

open System
open Dto
open Persistence

type GetFileWorkflow = Dto.GetFileDto -> Result<FileDto, WorkflowError>
type Factory = FileRepository -> GetFileWorkflow

let impl: Factory =
    fun fr command ->
        let getFile (fileId: Guid) =
            fr.get fileId

        command
            |> (fun c -> getFile c.Id)
            |> (fun f -> { FileDto.Id = f.Id; Data = f.Data; FileName = f.FileName })
            |> Ok
            