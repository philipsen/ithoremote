module MqttConnection

open Microsoft.Extensions.DependencyInjection
open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages;
open System.Text
open Serilog
open HouseStatusFactory

module log = 
    let log = LoggerConfiguration().WriteTo.Console().CreateLogger()
    let Information =
        log.Information
    Information "Load MqttConnection"

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


module Connection =
    open log
    let processIncoming house sender remoteId remoteCommandId =
        let remote = remoteDefinitions.remote remoteId 
        let command = remoteDefinitions.command remoteCommandId
        sprintf "received %s %s %s %s"  house sender remote command |> Information

        //let clients = GlobalHost.ConnectionManager.GetHubContext<MyFirstHub>().Clients

        //0


    let Startup = 
        Information "Connection, starting"
        let node = MqttClient(brokerHostName="167.99.32.103")

        //Received 
        let msgReceived (e:MqttMsgPublishEventArgs) =
            //printfn "Sub Received Topic: %s" e.Topic
            //printfn "Sub Received Qos: %u" e.QosLevel
            //printfn "Sub Received Retain: %b" e.Retain
            let m = Encoding.ASCII.GetString e.Message
            //printfn "Sub Received Message: %s" (m)
            //sprintf "mqtt received: %s %u %b %s"  e.Topic e.QosLevel e.Retain m |> Information
            let splitTopic = e.Topic.Split '/'
            let splitMessage = m.Split '/'

            match (splitTopic, splitMessage) with
            | [|"itho"; "log"; house|], [|"send"; sender; remoteId; remoteCommand |] -> 
                processIncoming house sender remoteId remoteCommand
            | _ -> ()
            

        node.MqttMsgPublishReceived.Add(msgReceived)

        node.Connect("fsharp_recv", "itho", "aapnootmies") |> ignore
        let topics = [| "#" |]
        let qos = [| MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]
        let sr = node.Subscribe(topics, qos)
        Information ("started: " + sr.ToString())
        node


type StartMqtt = MqttClient

type IServiceCollection with
    member this.AddMqtt() =
        this.AddSingleton<StartMqtt>(Connection.Startup)