module MqttConnection

open Microsoft.Extensions.DependencyInjection
open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages;
open System.Text
open Signalr
open Microsoft.AspNetCore.SignalR
open System
module log = 
    let log = Serilog.Log.Logger
    let Information =
        log.Information
    Information "Load MqttConnection"

open log

module remoteDefinitions = 
    let remotes = [ ("wmt6/main", "52:50:b9") ;
                    ("wmt6/second", "74:f3:af") ;
                    ("wmt40/main", "52:4c:6d") ;
                    ("wmt40/second", "74:f3:af") ;
                    ("wmt28/main", "52:4d:45") ;
                    ("wmt28/second", "74:f3:af") ] |> Map.ofList
    
    let remoteCommands = [
                    ("eco",      "22:f8:3:0:1:2");
                    ("comfort",  "22:f8:3:0:2:2");
                    ("cook1",    "22:f3:5:0:2:1e:2:3");
                    ("cook2",    "22:f3:5:0:2:3c:2:3");
                    ("s_timer1", "22:f3:3:63:80:1");
                    ("s_timer2", "22:f3:3:63:80:2");
                    ("s_timer3", "22:f3:3:63:80:3");
                    ("s_auto",   "22:f1:3:63:3:4")] |> Map.ofList

    let findValue remotes bytes =
        let p = remotes |> Map.tryPick (fun k v -> if v = bytes then Some(k) else None)
        printfn "fv %A %A %A" bytes p remotes.[bytes]
        match p with
        | Some key -> key
        | None -> "not found"

    let remote bytes =
        log.Information ("lookup = " + bytes)
        printfn "remotes = %A" remotes
        printfn "6 m  = %A" remotes.["wmt6/main"]
        printfn "6 m  = %A" remotes.[bytes]
        findValue remotes bytes

    let command bytes =
        findValue remoteCommands bytes



open HouseStatusFactory
open Db
open Domain

type Connection2() =
    let node = MqttClient(brokerHostName="167.99.32.103")

    do
       "Connection ctor2" |> Information
       node.Connect("fsharp_recv", "itho", "aapnootmies") |> ignore
       let cs= node.IsConnected
       Information("is connected " + cs.ToString())
       let topics = [| "#" |]
       let qos = [| MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]
       let sr = node.Subscribe(topics, qos)
       Information ("started: " + sr.ToString())

    member this.RegisterListener(msgReceived) = 
       node.MqttMsgPublishReceived.Add(msgReceived)

    member this.SendCommand(house: string, remote: string, command: string) = 
        // Information("here " + house + " : " + remote)
        // let b: byte[] = [| |]
        // let cs= node.IsConnected
        // Information("is connected " + cs.ToString())
        let commandString = remote + "/" + command
        let res = node.Publish("itho/" + house + "/transmit", System.Text.Encoding.ASCII.GetBytes commandString)
        // Information("res2 = " + res.ToString())
        res

let conn = Connection2() 

type StateUpdate = {
    house: string
    target: Target
    state: HouseStatusFactory.State
}


type Connection (sp: IServiceProvider) =
    let _hub = sp.GetService<IHubContext<IthoHub>>() 

    let processIncoming house sender remoteId remoteCommandId  =
        // let remote = remoteDefinitions.remote remoteId 
        // let command = remoteDefinitions.command remoteCommandId
        let state = getState events house
        // let update = { 
        //     house = house
        //     target = state.ventilation
        //     state = state
        // }
        _hub.Clients.All.SendAsync("state", state) |> Async.AwaitTask |> ignore

    // let node = MqttClient(brokerHostName="167.99.32.103")

    let msgReceived (e:MqttMsgPublishEventArgs) =
        let m = Encoding.ASCII.GetString e.Message
        let splitTopic = e.Topic.Split '/'
        let splitMessage = m.Split '/'
        match (splitTopic, splitMessage) with
        | [|"itho"; "log"; house|], [|"send"; sender; remoteId; remoteCommand |] -> 
            processIncoming house sender remoteId remoteCommand
        | _ -> ()

    do 
        "Connection ctor" |> Information
        conn.RegisterListener msgReceived

let sendCommand (house, remote, command): uint16 = 
    let remoteName = house + "/" + remote
    let remoteBytes =  remoteDefinitions.remotes.[remoteName]
    let commandBytes = remoteDefinitions.remoteCommands.[command]

    log.Information ("send to: " + remoteName + " " + command + " id: " + remoteBytes + " -> " + commandBytes)
    conn.SendCommand (house, remoteBytes, commandBytes)

type SendMqttCommand = string * string * string-> uint16

type IServiceCollection with
  member this.AddMqttService() =
    this.AddSingleton<SendMqttCommand>(sendCommand) |> ignore