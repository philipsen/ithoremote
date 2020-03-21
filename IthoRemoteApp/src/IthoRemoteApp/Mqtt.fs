module IthoRemoteApp.Mqtt
open System.Text

open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages
open Log
open Microsoft.Extensions.Configuration

let node = MqttClient(brokerHostName="167.99.32.103")

let msgReceived (mqttEvent:MqttMsgPublishEventArgs) =
    let message = Encoding.ASCII.GetString mqttEvent.Message
    sprintf "mqtt: received %s -> %s" mqttEvent.Topic message |> Information

    match mqttEvent.Topic.Split("/") with
    | [|"itho"; house; "received"; "allcb"|] -> 
        Domain.eventFromControlBoxPacket house message

    | [|"itho"; house; "received"; "allremotes"|] -> 
        Domain.eventFromRemote house message

    | [|"itho"; house; "received"; "handheld"|]
    | [|"itho"; house; "command"; "transmit"|] ->
        match message.Split("/") with
        | [| remote ; command |] ->
            Domain.createIthoTransmitRequestEvents house remote command
        | _ -> printf "unknown command ignored %s\n" message

    | [|"itho"; house; "received"; "fanspeed"|] -> 
        Domain.HouseAggregate.createIthoFanSpeedEvent house message

    | _ -> ()

let InitializeMqtt(settings: IConfiguration) =
    node.Connect("fs_rec", settings.["MqttUser"], settings.["MqttPassword"]) |> ignore
    sprintf "mtqq connection: %A" node.IsConnected |> Information
    let topics = [| "#" |]
    let qos = [| MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]
    node.Subscribe(topics, qos) |> ignore
    node.MqttMsgPublishReceived.Add msgReceived

let publish topic payload =
    node.Publish(topic, payload) |> ignore




