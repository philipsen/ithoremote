module IthoRemoteApp.Signalr
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging

open Serilog

type IthoHub () =
    inherit Hub ()
    let log = LoggerConfiguration().WriteTo.Console().CreateLogger()

    override this.OnConnectedAsync() =
        base.OnConnectedAsync().Wait()
        let r = base.Clients.All.SendAsync ("state", IthoRemoteApp.DomainTypes.currentState)
        r.Wait()
        async{
            let id = this.Context.UserIdentifier
            let n = this.Context.ConnectionId
            sprintf "a new client connected %A" n |>  log.Information
        } |> Async.StartAsTask :> _
        
 