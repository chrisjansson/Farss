module DtoValidation

module Nullable =
    open System
    let value (name: string) (v: Nullable<'a>) =
        if not v.HasValue then
            Error name
        else
            Ok v.Value
