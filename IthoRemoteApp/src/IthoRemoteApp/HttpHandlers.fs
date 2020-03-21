namespace IthoRemoteApp

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open Giraffe
open Giraffe.SerilogExtensions

module HttpHandlers =

    let handlers : HttpFunc -> HttpContext -> HttpFuncResult =
        choose [

            GET >=> route "/houses" >=>
                fun next context ->
                    let houses = HouseService.allHouses()
                    json houses next context
            GET >=> routef "/house/status/%s" (fun (house) ->
                fun next context ->
                    task {
                        let! r = HouseService.houseGetStatus house
                        return! match r with 
                                | Some d-> text d next context
                                | _ -> failwithf "house state not found"
                    }
            )
            PUT >=> routef "/house/command/%s/%s" (fun (transponder, remote) -> 
                fun next context ->
                    task {
                       let! command = context.ReadBodyFromRequestAsync()
                       TransponderService.sendCommand (transponder, remote, command)
                       return! text "" next context
                    }
            )
        ]    