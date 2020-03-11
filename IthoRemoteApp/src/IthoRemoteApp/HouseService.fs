module IthoRemoteApp.HouseService

open Microsoft.Extensions.DependencyInjection

open IthoRemoteApp.Models
type HouseAll = unit -> House list


let allHouses() = 
    [
        { name = "wmt6"; ip = "none" }
        { name = "wmt10"; ip = "none" }
        { name = "wmt20"; ip = "none" }
        { name = "wmt40"; ip = "none" }
        { name = "wmt6test"; ip = "none" }
    ]

type IServiceCollection with
    member this.AddHouseService() =
        this.AddSingleton<HouseAll>(allHouses) |> ignore

