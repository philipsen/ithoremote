module IthoRemoteApp.ClientMessageService

open Microsoft.AspNetCore.SignalR
open Signalr

type Common() =
    static let mutable hub: IHubContext<IthoHub> = null
    static member Hub
        with get () = hub
        and set v = hub <- v


let sendToClients (topic, message) =
    let sendToClientsWithHub (hub: IHubContext<IthoHub>) (topic, message: string) = 
        hub.Clients.All.SendAsync (topic, message) |> Async.AwaitTask |> Async.Start
    sendToClientsWithHub Common.Hub (topic, message)

