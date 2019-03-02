namespace Itho 

open System.Threading;
open System.Threading.Tasks;
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Hosting


type IClientApi = 
  abstract member Message :string -> System.Threading.Tasks.Task

type IthoHub () =
  inherit Hub<IClientApi> ()

type IthoService (hubContext :IHubContext<IthoHub, IClientApi>) =
  inherit BackgroundService ()
  
  member this.HubContext :IHubContext<IthoHub, IClientApi> = hubContext

  override this.ExecuteAsync (stoppingToken :CancellationToken) =
    let pingTimer = new System.Timers.Timer(1000.0)
    pingTimer.Elapsed.Add(fun _ -> 
        printfn "ping"
        this.HubContext.Clients.All.Message("aap") |> ignore)
    pingTimer.Start()
    Task.CompletedTask