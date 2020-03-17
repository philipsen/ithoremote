module IthoRemoteApp.Signalr
open Microsoft.AspNetCore.SignalR

open Serilog

type IthoHub () =
    inherit Hub ()
    let log = LoggerConfiguration().WriteTo.Console().CreateLogger()
        
    override this.OnConnectedAsync() =
        base.OnConnectedAsync().Wait()
        async{
            sprintf "a new client connected %A" ( this.Context.ConnectionId) |>  log.Information
        } |> Async.StartAsTask :> _
