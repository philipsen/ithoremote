namespace IthoRemoteApp

module HouseService =

    type House = {
        name: string
        ip: string
    }

    type HouseAll = unit -> House list

    let allHouses() = 
        [
            { name = "wmt6"; ip = "none" }
            { name = "wmt10"; ip = "none" }
            { name = "wmt20"; ip = "none" }
            { name = "wmt40"; ip = "none" }
            { name = "wmt6test"; ip = "none" }
        ]

    let houseGetStatus house = 
        async {
            let! r = EventStore.houseGetStatus house
            match r with
            | Some r -> return Some (System.Text.Encoding.ASCII.GetString r.Event.Data)
            | _ -> return None
        }

module TransponderService =
    let sendCommand (transponder, remote, command) =
        let topic = sprintf "itho/%s/command/transmit" transponder
        let payload = sprintf "%s/%s" remote command |> System.Text.Encoding.ASCII.GetBytes
        Mqtt.publish topic payload
    