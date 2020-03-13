module IthoRemoteApp.Domain

open System
open DomainTypes

let createIthoFanSpeedEvent house (msg: string) =
  {
    house = house
    speed = (msg |> int)
  } |> MyEventStore.addEvent

let parseControlBoxPacket (p: string) =
    let packet = p.Split ":"
    let rssi = packet |> Array.last |> int
    let pll = packet.[.. packet.Length-2] |> Array.toList |> List.map (fun a -> "0x" + a |> int)
    rssi, pll

let eventFromControlBoxPacket sender (p: string) =
  let rssi, pll = parseControlBoxPacket p
  let id = pll.[2..3]
  let house = match id with
              | [0x15; 0x28] -> Some "wmt6"
              | _ -> None
  match (pll.[0..1]) with
  | [0x14; 0x51] -> 
    let p = {
      sender = sender
      id = id
      house = house
      rssi = rssi
      fanspeed = pll.[9] / 2
      unknown = pll.[13]
    }
    p |> MyEventStore.addEvent
  | _ -> ()

let delayForCommand command =
  match command with
  | "cook1" -> Some 30
  | "cook2" -> Some 60
  | "timer1" -> Some 60
  | "timer2" -> Some 120
  | "timer3" -> Some 180
  | "s_timer1" -> Some 10
  | "s_timer2" -> Some 20
  | "s_timer3" -> Some 30
  | _ -> None

let createIthoTransmitRequestEvents house remote command =
  let correlationId = Guid.NewGuid()
  {
    house = house
    remote = remote
    command = command
    correlationId = correlationId
  } |> MyEventStore.addEvent

  let delay = delayForCommand command
  match delay with
  | Some delay ->
      {
        house = house
        remote = remote
        cancelCommand = command
        correlationId = correlationId
      } |> MyEventStore.addEventDelayed delay
  | _ -> ()