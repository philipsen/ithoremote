module IthoRemoteApp.Mqtt
open System
open System.Text

open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages
open Microsoft.Extensions.DependencyInjection

module log = 
    let log = Serilog.Log.Logger
    let Information =
        log.Information
open log

let node = MqttClient(brokerHostName="167.99.32.103")

let msgReceived (e:MqttMsgPublishEventArgs) =
    let m = Encoding.ASCII.GetString e.Message
    //sprintf "mqtt: received %s -> %s" e.Topic m |> Information
    match e.Topic.Split("/") with
    | [|"itho"; house; "received"; "allcb"|] -> 
        Domain.eventFromControlBoxPacket house m
    | [|"itho"; house; "received"; "allremotes"|] -> 
        Domain.eventFromRemote house m
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
    sprintf "mtqq connection: %A" node.IsConnected |> Information
    let topics = [| "#" |]
    let qos = [| MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]
    let sr = node.Subscribe(topics, qos)
    0

let InitializeMqtt() =
    "MqttConnection ctor" |> Information
    connectNode node |> ignore
    node.MqttMsgPublishReceived.Add msgReceived

let sendCommand (house, remote, command) =
    let topic = sprintf "itho/%s/command/transmit" house
    let payload = sprintf "%s/%s" remote command
    node.Publish(topic, System.Text.Encoding.ASCII.GetBytes payload) |> ignore




