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
      match delay with
      | 0 -> ()
      | delay ->
        {
          house = house
          remote = remote
          cancelCommand = command
          correlationId = correlationId
        } |> EventStore.addEventDelayed delay

