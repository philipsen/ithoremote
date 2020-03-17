namespace IthoRemoteApp
open Microsoft.AspNetCore.SignalR
open IthoRemoteApp.Signalr
open IthoRemoteApp.ClientMessageService

module ClientSignalService =
    
    let sendClientStatus  house (status: string) =
        // hub.Clients.All.SendAsync(("state/" + house), status) |> Async.AwaitTask |> Async.Start
        sendToClients ("state/" + house, status) 

    let sendStatusFanspeed  (status: string) =
        //hub.Clients.All.SendAsync(("fanstates"), status) |> Async.AwaitTask |> Async.Start
        sendToClients ("fanstates", status)