namespace IthoRemoteApp

module HouseService =

    type HandheldAddres = int list
    
    type HandheldType = Main | Secondary

    type HandheldRemote = {
        name: string
        address: HandheldAddres
        kind: HandheldType
    }

    type House = {
        name: string
        remotes: HandheldRemote list
    }

    type HouseAll = unit -> House list

    let allHouses = 
        [
            { name = "wmt6" ; remotes = [] }
            { name = "wmt10" ; remotes = []  }
            { name = "wmt20" ; remotes = []  }
            { name = "wmt40" ; remotes = []  }
            { name = "wmt6test" ; remotes = [
                { name = "main" ; address = [ ] ; kind = Main }
            ]  
            }
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
    