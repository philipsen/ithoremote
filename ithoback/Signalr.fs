module Signalr
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging
open System.Threading.Tasks
open Serilog

// type JsonBlob = JsonProvider<"Blob.json">
// type Message = 
//     | GetBlobs of AsyncReplyChannel<JsonBlob.Root list>
//     | PostBlob of JsonBlob.Root
// type Message = {
//     text: string
// }
type IGiraffeHubClient =
    interface
        abstract Send: msg: string -> Task
    end

type IGiraffeHubServer =
    interface
        abstract Broadcast: msg:string -> unit
        abstract Broadcast: msg:int -> unit
    end

//[<HubName("myFirstHub")>]
type IthoHub (logger : ILogger<IthoHub>) =
    inherit Hub ()

    // member x.MessageToTheServer message =
    //     printfn "Someone sent us a message! - %s" message

    let _logger = logger    

    interface IGiraffeHubServer with 
        member this.Broadcast( msg:string ) =
            logger.LogDebug "broadcast all"
            this.Clients.All.SendAsync(msg) |>Async.AwaitTask |> ignore
        member this.Broadcast( msg:int ) =
            this.Clients.All.SendAsync(msg.ToString()) |>Async.AwaitTask |> ignore
    override this.OnConnectedAsync() =
        // printf "connected -{%A}-\n" logger
        logger.LogDebug "aap"
        async{
            _logger.LogInformation( "Connected" + this.Context.UserIdentifier )
            } |> Async.StartAsTask :> _

// type Server (host:string) =
//     let startup (a:IAppBuilder) =
//         a.UseCors(CorsOptions.AllowAll) |> ignore
//         a.MapSignalR() |> ignore

//     let clients = GlobalHost.ConnectionManager.GetHubContext<MyFirstHub>().Clients

//     do
//         WebApp.Start(host, startup) |> ignore
//         printfn "Signalr server running on %s" host

//     member x.Send message = clients.All?messageToTheClient message        


