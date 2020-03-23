namespace IthoRemoteApp
    
module TransponderService =
    let sendCommand transponder remote command =
        let topic = sprintf "itho/%s/command/transmit" transponder
        let payload = sprintf "%s/%s" remote command |> System.Text.Encoding.ASCII.GetBytes
        Mqtt.publish topic payload

    let sendRawCommand transponder command =
        let topic = sprintf "itho/%s/command/transmitraw" transponder
        let payload = command |> List.map (fun i -> sprintf "%02x" i ) |> String.concat ":"
        printf "c = %A\n" payload
        let payload = payload |> System.Text.Encoding.ASCII.GetBytes
        Mqtt.publish topic payload


module HouseService =

    type HandheldAddres = int list
    
    type HandheldType = Main | Secondary

    type HandheldRemote = {
        name: string
        address: HandheldAddres
        kind: HandheldType
    }

    type HouseId = Wmt6 | Wmt40

    type House = {
        name: HouseId
        remotes: HandheldRemote list
        transponder: string
    }

    let allHouses = 
        [
            { 
                name = Wmt6 ; 
                remotes = [
                  { name = "main"   ; address = [ 0x52 ; 0x50 ; 0xB9 ] ; kind = Main }
                  { name = "second" ; address = [ 0x74 ; 0xF3 ; 0xAF ] ; kind = Secondary }
                ]
                transponder = "wmt6test"
            }
            { name = Wmt40 ; remotes = [] ; transponder = "wmt40" }
        ]

    let getHouse house = 
        allHouses |> List.find (fun e -> e.name = house)

    let getHouseRemote house remote =
        let house = getHouse house
        house.remotes |> List.find (fun r -> r.name = remote)

    let stringTohouse house = 
        match house with
        | "wmt6" -> Wmt6
        | "wmt40" -> Wmt40
        | _ -> failwith "no transponder for this house"

    let getTransponderForHouse house =
        let house = getHouse house
        house.transponder

    let getStatus house = 
        async {
            let! r = EventStore.houseGetStatus house
            match r with
            | Some r -> return Some (System.Text.Encoding.ASCII.GetString r.Event.Data)
            | _ -> return Some "{\"transmits\":0,\"fanspeed\":0,\"baseState\":\"eco\",\"state\":\"eco\"}"
        }

    type HandheldCommand = {
        name: string
        bytes: int list
    }

    type RemoteCommands = {
        kind: HandheldType
        commands: HandheldCommand list
    }

    let AllCommands = [
        {
            kind = Main
            commands = [
                { name = "eco";     bytes = [ 0x22; 0xf8; 0x03; 0x00; 0x01; 0x02 ]}
                { name = "comfort"; bytes = [ 0x22; 0xf8; 0x03; 0x00; 0x02; 0x02 ]}
                { name = "cook1";   bytes = [ 0x22; 0xf3; 0x05; 0x00; 0x02; 0x1e; 0x02; 0x03 ]}
                { name = "cook2";   bytes = [ 0x22; 0xf3; 0x05; 0x00; 0x02; 0x3c; 0x02; 0x03 ]}
                { name = "time1";   bytes = [ 0x22; 0xF3; 0x05; 0x00; 0x42; 0x03; 0x03; 0x03 ]}
                { name = "time2";   bytes = [ 0x22; 0xF3; 0x05; 0x00; 0x42; 0x06; 0x03; 0x03 ]}
                { name = "time3";   bytes = [ 0x22; 0xF3; 0x05; 0x00; 0x42; 0x09; 0x03; 0x03 ]}
            ]
        }
        {
            kind = Secondary
            commands = [
                { name = "s_timer1";bytes = [ 0x22; 0xf3; 0x03; 0x63; 0x80; 0x01 ]}
                { name = "s_timer2";bytes = [ 0x22; 0xf3; 0x03; 0x63; 0x80; 0x02 ]}
                { name = "s_timer3";bytes = [ 0x22; 0xf3; 0x03; 0x63; 0x80; 0x03 ]}
            ]
        }
    ]

    let getCommandBytes kind command =
        let remote = AllCommands |> List.find (fun r -> r.kind = kind)
        let command = remote.commands |> List.find (fun c -> c.name = command)
        command.bytes

    let sendCommand house remote command =
        Domain.VirtualRemoteAggregate.createIthoTransmitRequestEvents house remote command

        let house = stringTohouse house
        let transponder:string = getTransponderForHouse house
        let remote = getHouseRemote house remote
        let counter = System.Random().Next() % 256
        let command = getCommandBytes remote.kind command
        let c = [ 0x16 ] @ remote.address @ [ counter ] @ command
        printf "c = %A\n" c
        TransponderService.sendRawCommand transponder c 

module ButtonService =         
    type IthoButton = {
        label: string
        remoteId: string
        remoteCommand: string
    }

    let wmt6Buttons = [      
        { label = "Eco" ;              remoteId = "main" ;  remoteCommand = "eco"}
        { label = "Comfort" ;          remoteId = "main";   remoteCommand = "comfort"}
        { label = "Keuken 30 min";     remoteId = "main";   remoteCommand = "cook1"}
        { label = "Maximaal 30 min";   remoteId = "main";   remoteCommand = "timer1"}
        { label = "WC beneden 10 min"; remoteId = "second"; remoteCommand = "s_timer1"}
        { label = "WC beneden 20 min"; remoteId = "second"; remoteCommand = "s_timer2"}
        { label = "WC auto";           remoteId = "second"; remoteCommand = "s_auto"}
    ]

    let wmt40Buttons = [
        { label = "Eco" ;           remoteId = "main" ; remoteCommand = "eco"}
        { label = "Comfort" ;       remoteId = "main" ; remoteCommand = "comfort"}
        { label = "Keuken 30 min" ; remoteId = "main" ; remoteCommand = "cook1"}
        { label = "Keuken 60 min" ; remoteId = "main" ; remoteCommand = "cook2"}
        { label = "Douche 10 min" ; remoteId = "second" ; remoteCommand = "s_timer1"}
        { label = "Douche 20 min" ; remoteId = "second" ; remoteCommand = "s_timer2"}
    ]

    let getButtons house =
        match house with
        | "wmt6" -> wmt6Buttons
        | "wmt40" -> wmt40Buttons
        | _  -> failwith "no buttons defined"
