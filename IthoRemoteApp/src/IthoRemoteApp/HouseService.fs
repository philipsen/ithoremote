module IthoRemoteApp.HouseService

type House = {
    name: string
    ip: string
}

type HouseAll = unit -> House list

let allHouses() = 
    [
        { name = "wmt6"; ip = "none" }
        { name = "wmt10"; ip = "none" }
        { name = "wmt20"; ip = "none" }
        { name = "wmt40"; ip = "none" }
        { name = "wmt6test"; ip = "none" }
    ]
