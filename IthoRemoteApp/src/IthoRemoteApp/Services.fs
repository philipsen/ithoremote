namespace IthoRemoteApp
    
open System
open System.Text
open uPLibrary.Networking.M2Mqtt.Messages
open Mqtt

open Log
open DomainTypes

module HouseService =

    type HandheldAddres = int list
    
    type HandheldType = Main | Secondary

    type HandheldRemote = {
        name: string
        address: HandheldAddres
        kind: HandheldType
    }

    type HouseId = Wmt6 | Wmt10 | Wmt40

    let houseIdToString id =
        match id with
        | Wmt6 -> "wmt6"
        | Wmt10 -> "wmt10"
        | _ -> ""

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
                  { name = "fake" ;   address = [ 0x00 ; 0x00 ; 0x09 ] ; kind = Secondary }
                ]
                transponder = "wmt6test"
            }            
            { 
                name = Wmt10 ; 
                remotes = [
                  { name = "main"   ; address = [ 0x00 ; 0x0 ; 0x0 ] ; kind = Main }
                ]
                transponder = "wmt6test"
            }
            { name = Wmt40 ; remotes = [
                  { name = "main"   ; address = [ 0xff ; 0xff ; 0xff ] ; kind = Main }
                  { name = "second" ; address = [ 0x74 ; 0xF3 ; 0xAF ] ; kind = Secondary }                
            ] ; transponder = "wmt40" }
        ]

    let getHouseForHandheldRemote transponder address =
        let tranponderMatch house = house.transponder = transponder
        let addressMatch house = house.remotes |> List.exists (fun r -> r.address = address)
        let remotes = 
            let house = allHouses |> List.filter tranponderMatch |> List.tryFind addressMatch
            match house with 
            | Some house -> 
                let remote = house.remotes |> List.find (fun remote -> remote.address = address)
                Some house, Some remote
            | _ -> 
                sprintf "unhandled remote %A" address |> Error
                (None, None)
        remotes

    let getHouse house = 
        allHouses |> List.find (fun e -> e.name = house)

    let getHouseRemote house remote =
        let house = getHouse house
        house.remotes |> List.find (fun r -> r.name = remote)

    let stringTohouse house = 
        match house with
        | "wmt6" -> Wmt6
        | "wmt10" -> Wmt10
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
        Domain.HouseAggregate.createIthoTransmitRequestEvents house remote command
        let house = stringTohouse house
        let transponder = getTransponderForHouse house
        let remote = getHouseRemote house remote
        let counter = Random().Next() % 256
        let command = getCommandBytes remote.kind command
        let c = [ 0x16 ] @ remote.address @ [ counter ] @ command
        TransponderService.sendRawCommand transponder c 

module HandheldRemoteService =

    let eventFromRemote sender (packet: string) =
      printf "handheld remote: %s %s\n" sender packet
      let bytes = packet.Split ":"
      match bytes with
      | [| |] -> failwithf "no match %A\n" bytes
      | _ ->
        let rssi = bytes.[bytes.Length-1] |> int
        let message = bytes.[0 .. bytes.Length-2]
        let parseHex str = Int32.Parse (str,  Globalization.NumberStyles.HexNumber)

        match message |> Array.toList |> List.map parseHex with
        | 0x16 :: a1 :: a2 :: a3 :: rest ->
          let message = {
            rssi = rssi
            transponder = sender
            id = [ a1; a2; a3 ]
            time = DateTime.Now
          }
          ClientMessageService.sendToClients ("handheld", (message |> Json.serialize))
          let remotes = HouseService.getHouseForHandheldRemote message.transponder message.id
          match remotes with
          | (Some house, Some remote)  ->
            let command = rest.[1 .. (rest.Length-2)]
            let command = (HouseService.AllCommands |> List.find (fun r -> r.kind = remote.kind)).commands 
                          |> List.find (fun c -> c.bytes = command)
            // printf "got hr = %A %A %s %s\n" house.name remote.kind remote.name command.name
            Domain.HouseAggregate.createIthoTransmitRequestEvents (HouseService.houseIdToString house.name) remote.name command.name
          | _ -> sprintf "handheld for unknown house / remote" |> log.Warning

        | _ -> failwithf "no match %A\n" bytes        

module ControlBoxService = 

    let (|FanspeedPacket|) packetIdentifier =
      match packetIdentifier with
      | [0x14; 0x51] -> FanspeedPacket
      | _ -> ()

    let (|Wmt6|Wmt10|Other|) controlBoxAddress =
      match controlBoxAddress with
      | [0x15; 0x28] -> Wmt6
      | [0x10; 0x45] -> Wmt10
      | _ -> Other

    let eventFromControlBoxPacket sender (packet: string) =
      let packet = packet.Split ":"
      match packet.Length with
      | 17 -> 
          let rssi = packet |> Array.last |> int
          let packet = packet.[.. packet.Length-2] |> Array.toList |> List.map ((sprintf "0x%s") >> int)
          match (packet.[0..1]) with
          | FanspeedPacket -> 
            let id = packet.[2..3]
            let house = match id with
                        | Wmt6 -> Some "wmt6"
                        | Wmt10 -> Some "wmt10"
                        | _ -> None
            let fanspeed = packet.[9] / 2
            {
              sender = sender
              id = id
              house = house
              rssi = rssi
              fanspeed = fanspeed
              unknown = packet.[13]
            } |> EventStore.addEvent
            // printf "fanspeed address = %A house = %A speed = %A\n" id house fanspeed
            match house with
            | Some house -> Domain.HouseAggregate.createIthoFanSpeedEvent house (fanspeed.ToString())
            | _ -> ()

        | _ -> sprintf "unexpected packet length %d (p = %A)" packet.Length packet |> Information


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

    let wmt10Buttons = [      
        { label = "Eco" ;              remoteId = "main" ;  remoteCommand = "eco"}
        { label = "Comfort" ;          remoteId = "main";   remoteCommand = "comfort"}
        { label = "Keuken 30 min";     remoteId = "main";   remoteCommand = "cook1"}
        { label = "Keuken 60 min";     remoteId = "main";   remoteCommand = "cook2"}
        { label = "Maximaal 30 min";   remoteId = "main";   remoteCommand = "timer1"}
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
        | "wmt10" -> wmt10Buttons
        | "wmt40" -> wmt40Buttons
        | _  -> failwith "no buttons defined"


module MqttService =
  let msgReceived (mqttEvent:MqttMsgPublishEventArgs) =
      let message = Encoding.ASCII.GetString mqttEvent.Message
      sprintf "mqtt: received %s -> %s" mqttEvent.Topic message |> Information

      match mqttEvent.Topic with
      | ControlBox transponder -> 
          ControlBoxService.eventFromControlBoxPacket transponder message
      | HandheldRemote transponder -> 
          HandheldRemoteService.eventFromRemote transponder message
      | _ -> ()
