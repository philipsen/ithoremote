namespace IthoRemoteApp
open Microsoft.AspNetCore.SignalR
open IthoRemoteApp.Signalr

module ClientSignalService =
    
    let sendClientStatus (hub: IHubContext<IthoHub>) house (status: string) =
        hub.Clients.All.SendAsync(("state/" + house), status) |> Async.AwaitTask |> Async.Start

    let sendStatusFanspeed (hub: IHubContext<IthoHub>) (status: string) =
        hub.Clients.All.SendAsync(("fanstates"), status) |> Async.AwaitTask |> Async.Start