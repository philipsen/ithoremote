module IthoRemoteApp.Mqtt
open System
open System.Text

open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages

open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.DependencyInjection

open IthoRemoteApp.Models
open IthoRemoteApp.Signalr

module log = 
    let log = Serilog.Log.Logger
    let Information =
        log.Information
open log

type MqttConnection (sp: IServiceProvider) =
    let _hub = sp.GetService<IHubContext<IthoHub>>()
    let node = MqttClient(brokerHostName="167.99.32.103")

    let msgReceived (e:MqttMsgPublishEventArgs) =
        let m = Encoding.ASCII.GetString e.Message
        sprintf "mqtt: received %s -> %s" e.Topic m |> Information
        // _hub.Clients.All.SendAsync("state", state) |> Async.AwaitTask |> Async.RunSynchronously
        match e.Topic.Split("/") with
        | [|"itho"; "wmt6test"; "received"; "allcb"|] -> 
            Domain.eventFromControlBoxPacket m
        | [|"itho"; house; "received"; "handheld"|]
        | [|"itho"; house; "command"; "transmit"|] ->
            match m.Split("/") with
            | [| remote ; command |] ->
                Domain.createIthoTransmitRequestEvents house remote command
            | _ -> printf "unknown command ignored %s\n" m
        | [|"itho"; house; "received"; "fanspeed"|] -> 
            Domain.createIthoFanSpeedEvent house m
        | _ -> ()

    let connectNode (node: MqttClient) =
        node.Connect("fs_rec", "itho", "aapnootmies") |> ignore
        printf "mtqq connection: %A\n" node.IsConnected
        let topics = [| "#" |]
        let qos = [| MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]
        let sr = node.Subscribe(topics, qos)
        0

    do 
        "MqttConnection ctor" |> Information
        //conn.RegisterListener msgReceived
        connectNode node |> ignore
        node.MqttMsgPublishReceived.Add msgReceived



