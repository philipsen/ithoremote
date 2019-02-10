namespace House
open MongoDB.Bson


type House = {
    Id: BsonObjectId
    name: string
    ip: string
    __v: int
}

type HouseSave = House -> House

type HouseCriteria = 
    | All

type HouseFind = HouseCriteria -> House[]

type HouseFindByName = string -> House


