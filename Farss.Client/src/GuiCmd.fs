module GuiCmd

open Elmish
open Model
open System

let loadSubsAndArticles =
    let inner () = 
        ApiClient.getSubscriptions ()
        |> PromiseResult.bind(fun r -> ApiClient.getArticles () |> PromiseResult.map (fun r2 -> r, r2))

    Cmd.ofPromiseResult inner () Msg.Loaded Msg.LoadingError

let deleteSubscription (id: Guid) =
    let dto: Dto.DeleteSubscriptionDto = { Id = Some id }
    Cmd.ofPromiseResult ApiClient.deleteSubscription dto (fun _ -> SubscriptionDeleted) SubscriptionDeleteFailed

let alert (message: string) =
    Cmd.ofSub (fun _ -> Fable.Import.Browser.window.alert message)