module IthoRemoteApp.Domain

open Newtonsoft.Json

type IthoEvent = {
    receiver: string
    payload: int list
    rssi: int
}

type IthoControlBoxEvent = {
  id: int list
  house: Option<string>
  rssi: int
  fanspeed: int
  unknown: int
}

type IthoTransmitEvent = {
  house: string
  remote: string
  command: string
  correlationId: System.Guid
}

type IthoTransmitCancelEvent = {
  house: string
  remote: string
  cancelCommand: string
  correlationId: System.Guid
}

type IthoFanSpeed = {
  house: string
  speed: int
}


type Event =
    | IthoFanSpeed of IthoFanSpeed


type EventHandler() =
    member this.Handle = function
        | IthoFanSpeed event ->
            printf "here\n"

let createIthoFanSpeedEvent house (msg: string) =
  {
    house = house
    speed = (msg |> int)
  }

// let createIthoTransmitRequestEvent house (msg: string) =
//   match msg.Split("/") with
//   | [| remote ; command |] ->
//       Some {
//         house = house
//         remote = remote
//         command = command
//         correlationId = System.Guid.NewGuid()
//       }
//   | _ -> None

let createIthoHandheldEvent house (msg: string) =
  match msg.Split("/") with
  | [| remote ; command |] ->
      Some {
        house = house
        remote = remote
        command = command
        correlationId = System.Guid.NewGuid()
      }
  | _ -> None

let parseControlBoxPacket (p: string) =
    let packet = p.Split ":"
    let rssi = packet |> Array.last |> int
    let pll = packet.[.. packet.Length-2] |> Array.toList |> List.map (fun a -> "0x" + a |> int)
    rssi, pll

let eventFromControlBoxPacket (p: string) =
  let rssi, pll = parseControlBoxPacket p
  let id = pll.[2..3]
  let house = match id with
              | [0x15; 0x28] -> Some "wmt6"
              | _ -> None
  match (pll.[0..1]) with
  | [0x14; 0x51] -> 
    let p = {
      id = id
      house = house
      rssi = rssi
      fanspeed = pll.[9] / 2
      unknown = pll.[13]
    }
    Some p
  | _ -> None


module Json =
  open Newtonsoft.Json

  let serialize obj =
    JsonConvert.SerializeObject obj
