namespace IthoRemoteApp

module TransponderService =
    let sendCommand transponder remote command =
        let topic = sprintf "itho/%s/command/transmit" transponder
        let payload = sprintf "%s/%s" remote command |> System.Text.Encoding.ASCII.GetBytes
        Mqtt.publish topic payload

    let sendRawCommand transponder command =
        let topic = sprintf "itho/%s/command/transmitraw" transponder
        let payload = command |> List.map (fun i -> sprintf "%02x" i ) |> String.concat ":"
        // printf "c = %A\n" payload
        let payload = payload |> System.Text.Encoding.ASCII.GetBytes
        Mqtt.publish topic payload
