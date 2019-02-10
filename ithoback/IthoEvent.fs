namespace IthoEvent

open MongoDB.Bson

type IthoEvent = {
    Id: BsonObjectId
    kind: string
    house: string
    remote: string
    sender: string
    command: string
    time: BsonDateTime
    __v: int
}

type EventCriteria = 
    | ByName of string
    | All

type EventFind = EventCriteria -> IthoEvent[]