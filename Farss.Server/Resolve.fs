module Resolve

open System
open Microsoft.FSharp.Reflection


let services (func: 'a -> 'b) (serviceProvider: IServiceProvider): 'b = 
    let argType = typeof<'a>
    
    let diArguments =
        if FSharpType.IsTuple argType then
            FSharpType.GetTupleElements argType
            |> Array.map serviceProvider.GetService
            |> (fun args -> FSharpValue.MakeTuple(args, argType))
        else
            serviceProvider.GetService(argType)
            
    let methodInfo =
        func.GetType().GetMethods()
        |> Array.filter (fun x -> x.Name = "Invoke" && x.GetParameters().Length = 1)
        |> Array.exactlyOne
        
    methodInfo.Invoke(func, [| diArguments |]) :?> 'b
                               
