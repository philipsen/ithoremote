module Itho 

open System.Threading;
open System.Threading.Tasks;
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open uPLibrary.Networking.M2Mqtt
open System.Text

module remoteDefinitions = 
    let remotes = [ ("wmt6/main", "52:50:b9") ;
                    ("wmt6/second", "74:f3:af") ;
                    ("wmt40/main", "52:4c:6d") ;
                    ("wmt40/second", "74:f3:af") ;
                    ("wmt28/main", "52:4d:45") ;
                    ("wmt28/second", "74:f3:af") ] |> Map.ofList
    
    let remoteCommands = [
                    ("eco", "22:f8:3:0:1:2");
                    ("comfort", "22:f8:3:0:2:2");
                    ("cook1", "22:f3:5:0:2:1e:2:3");
                    ("cook2", "22:f3:5:0:2:3c:2:3");
                    ("s_timer1", "22:f3:3:63:80:1");
                    ("s_timer2", "22:f3:3:63:80:2");
                    ("s_timer3", "22:f3:3:63:80:3");
                    ("s_auto", "22:f1:3:63:3:4")] |> Map.ofList

    let findValue remotes bytes =
        let p = remotes |> Map.tryPick (fun k v -> if v = bytes then Some(k) else None)
        match p with
        | Some key -> key
        | None -> "not found"

    let remote bytes =
        findValue remotes bytes

    let command bytes =
        findValue remoteCommands bytes
    
    let commandBytes command =
      remoteCommands.TryFind command

    let remoteBytes remote = 
      remotes.TryFind remote

type Mqtt() = 
  let node = MqttClient(brokerHostName="167.99.32.103")
  do node.Connect("fsharp_recv2", "itho", "aapnootmies") |> ignore

  member this.SendBytes (name:string, remote:string, command:string) : unit =
    let topic = "itho/" + name + "/send"
    let cb = remoteDefinitions.commandBytes command
    let rb = name + "/" + remote |> remoteDefinitions.remoteBytes
    let payload = rb.Value + "/" + cb.Value |> Encoding.ASCII.GetBytes
    node.Publish(topic, payload) |> ignore

  member this.SetMessageLevel (name, kind, onOff): unit =
    let topic = name + "/set/"
    let payload = kind + "/" + onOff |> Encoding.ASCII.GetBytes
    node.Publish(topic, payload) |> ignore


let mqtt = Mqtt()

type IClientApi = 
  abstract member Message :string -> System.Threading.Tasks.Task

type IthoHub () =
  inherit Hub<IClientApi> ()

type IthoService (hubContext :IHubContext<IthoHub, IClientApi>) =
  inherit BackgroundService ()
  
  member this.HubContext :IHubContext<IthoHub, IClientApi> = hubContext

  override this.ExecuteAsync (stoppingToken :CancellationToken) =
    let pingTimer = new System.Timers.Timer(15000.0)
    pingTimer.Elapsed.Add(fun _ -> 
        printfn "ping"
        this.HubContext.Clients.All.Message("aap") |> ignore)
    pingTimer.Start()
    Task.CompletedTask

type SendBytes = string * string * string -> unit

type SetMessageLevel = string * string * string -> unit

type IServiceCollection with
  member this.AddSendBytes () =
    this.AddSingleton<SendBytes>(mqtt.SendBytes) |> ignore
    this.AddSingleton<SetMessageLevel>(mqtt.SetMessageLevel) |> ignore
    