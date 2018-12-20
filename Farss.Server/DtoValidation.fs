module DtoValidation

module Nullable =
    open System
    let value (name: string) (v: Nullable<'a>) =
        if not v.HasValue then
            Error name
        else
            Ok v.Value

module Option =
    open System
    let value (name: string) (v: Option<'a>) =
        match v with
        | Some v -> Ok v
        | None -> Error name
