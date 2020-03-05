module IthoRemoteApp.Signalr
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging
open IthoRemoteApp.Models

open Serilog

//log.Information "Load Domain"


type IthoHub () =
    inherit Hub ()
    let log = LoggerConfiguration().WriteTo.Console().CreateLogger()

    // let this.send msg =
    //     this.Clients.All.SendAsync("state", msg) |> Async.AwaitTask |> ignore
       
    override this.OnConnectedAsync() =
        async{
            let id = this.Context.UserIdentifier
            let n = this.Context.ConnectionId
            sprintf "a new client connected %A" n |>  log.Information
        } |> Async.StartAsTask :> _
        
 
        //this.Clients.All.SendAsync("state") |> Async.AwaitTask |> ignore
