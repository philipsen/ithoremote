module IthoRemoteApp.ClientMessageService

open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.SignalR

open Signalr

type Common() =
    static let mutable hub: IHubContext<IthoHub> = null

    static member Hub
        with get () = hub
        and set v = hub <- v


let sendToClientsWithHub (hub: IHubContext<IthoHub>) (topic, message: string) = 
    hub.Clients.All.SendAsync (topic, message) |> Async.AwaitTask |> Async.Start

let sendToClients (topic, message) =
    sendToClientsWithHub Common.Hub (topic, message)

type SendClientMessage = string * string -> unit

type IServiceCollection with
    member this.AddClientMessageService() =
        this.AddSingleton<SendClientMessage>(sendToClients) |> ignore


