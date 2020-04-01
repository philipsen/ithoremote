namespace IthoRemoteApp
open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages
open Microsoft.Extensions.Configuration
open Log

module Mqtt =

    let (|ControlBox|HandheldRemote|Unknown|) (topic: string) = 
        match topic.Split("/") with
        | [|"itho"; transponder; "received"; "allcb"|] -> ControlBox transponder
        | [|"itho"; transponder; "received"; "allremotes"|] -> HandheldRemote transponder
        | _ -> Unknown

    let mutable node = null

    let InitializeMqtt(settings: IConfiguration) msgReceived =
        node <- MqttClient(settings.["MqttHost"])
        node.Connect("fs_rec", settings.["MqttUser"], settings.["MqttPassword"]) |> ignore
        sprintf "mtqq connection: %A" node.IsConnected |> Information
        let topics = [| "#" |]
        let qos = [| MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]
        node.Subscribe(topics, qos) |> ignore
        node.MqttMsgPublishReceived.Add msgReceived

    let publish topic payload =
        node.Publish(topic, payload) |> ignore




