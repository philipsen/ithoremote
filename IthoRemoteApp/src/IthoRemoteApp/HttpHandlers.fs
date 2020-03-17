namespace IthoRemoteApp


open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open Giraffe
open Giraffe.SerilogExtensions

open HouseService
open Mqtt

module HttpHandlers =

    let handlers : HttpFunc -> HttpContext -> HttpFuncResult =
        choose [

            GET >=> route "/houses" >=>
                fun next context ->
                    let houses = allHouses()
                    json houses next context
            GET >=> routef "/house/status/%s" (fun (house) ->
                fun next context ->
                    task {
                        let! r = MyEventStore.houseGetStatus house
                        return! match r with 
                                | Some r ->
                                    let d = System.Text.Encoding.ASCII.GetString r.Event.Data
                                    text d next context
                                | _ -> failwithf "house state not found"
                    }
            )
            PUT >=> routef "/house/command/%s/%s" (fun (house, remote) -> 
                fun next context ->
                    let logger = context.Logger()
                    // let mqtt = context.GetService<SendMqttCommand>()
                    task {
                       let! command = context.ReadBodyFromRequestAsync()
                       logger.Information("got command to send: {0} {1} {2}", house, remote, command)
                       sendCommand (house, remote, command)
                       return! text "" next context
                    }
            )
        ]    