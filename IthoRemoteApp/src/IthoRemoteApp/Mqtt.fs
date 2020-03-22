module IthoRemoteApp.Mqtt
open System.Text

open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages
open Log
open Microsoft.Extensions.Configuration

let (|ControlBox|HandheldRemote|TransmitRequest|Fanspeed|Unknown|) (topic: string) = 
    match topic.Split("/") with
    | [|"itho"; transponder; "received"; "allcb"|] -> ControlBox transponder
    | [|"itho"; transponder; "received"; "allremotes"|] -> HandheldRemote transponder
    | [|"itho"; transponder; "received"; "handheld"|]
    | [|"itho"; transponder; "command"; "transmit"|] -> TransmitRequest transponder
    | [|"itho"; house; "received"; "fanspeed"|] -> Fanspeed house
    | _ -> Unknown


let msgReceived (mqttEvent:MqttMsgPublishEventArgs) =
    let message = Encoding.ASCII.GetString mqttEvent.Message
    sprintf "mqtt: received %s -> %s" mqttEvent.Topic message |> Information

    match mqttEvent.Topic with
    | ControlBox transponder -> Domain.ControlBoxAggregate.eventFromControlBoxPacket transponder message
    | HandheldRemote transponder -> Domain.HandheldRemoteAggregate.eventFromRemote transponder message
    | TransmitRequest transponder ->
        match message.Split("/") with
        | [| remote ; command |] ->
            Domain.VirtualRemoteAggregate.createIthoTransmitRequestEvents transponder remote command
        | _ -> printf "unknown command ignored %s\n" message
    | Fanspeed house -> Domain.HouseAggregate.createIthoFanSpeedEvent house message
    | _ -> ()

let mutable node = null

let InitializeMqtt(settings: IConfiguration) =
    node <- MqttClient(settings.["MqttHost"])
    node.Connect("fs_rec", settings.["MqttUser"], settings.["MqttPassword"]) |> ignore
    sprintf "mtqq connection: %A" node.IsConnected |> Information
    let topics = [| "#" |]
    let qos = [| MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]
    node.Subscribe(topics, qos) |> ignore
    node.MqttMsgPublishReceived.Add msgReceived

let publish topic payload =
    node.Publish(topic, payload) |> ignore




