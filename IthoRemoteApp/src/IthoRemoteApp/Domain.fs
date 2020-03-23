namespace IthoRemoteApp

open System
open DomainTypes

module Domain =

  module HouseAggregate = 
    let createIthoFanSpeedEvent house (msg: string) =
      // printf "create fs %s %s\n" house msg
      {
        house = house
        speed = (msg |> int)
      } |> EventStore.addEvent

  module HandheldRemoteAggregate =
    let eventFromRemote sender (packet: string) =
      printf "handheld remote: %s %s\n" sender packet
      let parseHex str = Int32.Parse (str,  Globalization.NumberStyles.HexNumber)
      let bytes = packet.Split ":"
      match bytes with
      | [| |] -> failwithf "no match %A\n" bytes
      | _ ->
        let rssi = bytes.[bytes.Length-1] |> int
        match bytes.[0 .. bytes.Length-2] |> Array.toList |> List.map parseHex with
        | 0x16 :: a1 :: a2 :: a3 :: rest ->
          // printf "got match %d %d %d %A\n" a1 a2 a3 rest
          let message = {
            rssi = rssi
            transponder = sender
            id = [ a1; a2; a3 ]
            time = DateTime.Now
          }
          ClientMessageService.sendToClients ("handheld", (message |> Json.serialize))
        | _ -> failwithf "no match %A\n" bytes

  module ControlBoxAggregate = 

    let (|FanspeedPacket|) packet =
      match packet with
      | [0x14; 0x51] -> FanspeedPacket
      | _ -> ()

    let (|Wmt6|Other|) address =
      match address with
      | [0x15; 0x28] -> Wmt6
      | _ -> Other

    let eventFromControlBoxPacket sender (packet: string) =
      let packet = packet.Split ":"
      let rssi = packet |> Array.last |> int
      let packet = packet.[.. packet.Length-2] |> Array.toList |> List.map ((sprintf "0x%s") >> int)
      
      let id = packet.[2..3]
      let house = match id with
                  | Wmt6 -> Some "wmt6"
                  | _ -> None
      let fanspeed = packet.[9] / 2
      match (packet.[0..1]) with
      | FanspeedPacket -> 
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
      | Some house -> HouseAggregate.createIthoFanSpeedEvent house (fanspeed.ToString())
      | _ -> ()

  module VirtualRemoteAggregate = 

    let delayForCommand command =
      match command with
      | "cook1" ->  30
      | "cook2" ->  60
      | "timer1" ->  60
      | "timer2" ->  120
      | "timer3" ->  180
      | "s_timer1" ->  10
      | "s_timer2" ->  20
      | "s_timer3" ->  30
      | _ -> 0

    let createIthoTransmitRequestEvents house remote command =
      let correlationId = Guid.NewGuid()
      {
        house = house
        remote = remote
        command = command
        correlationId = correlationId
      } |> EventStore.addEvent

      let delay = delayForCommand command
      {
        house = house
        remote = remote
        cancelCommand = command
        correlationId = correlationId
      } |> EventStore.addEventDelayed delay
