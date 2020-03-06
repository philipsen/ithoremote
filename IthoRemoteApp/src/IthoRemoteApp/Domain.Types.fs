
module IthoRemoteApp.DomainTypes

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

type HouseState = {
  transmits: int
  fanspeed: int
  baseState: string
  state: string
}
