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
            // POST >=> route "/houses" >=>
            //     fun next context ->
            //         task {
            //             let save = context.GetService<HouseSave>()
            //             let! house = context.BindJsonAsync<House>()
            //             return! json (save house) next context
            //         }
            // POST >=> route "/msg" >=>
            //     fun next context ->
            //         task {
            //             let logger = context.Logger()
            //             logger.Information "send message to hub"
            //             let hub = context.GetService<IHubContext<IthoHub>>()
            //             let! a = context.ReadBodyFromRequestAsync()
            //             hub.Clients.All.SendAsync("aap", a) |> Async.AwaitTask |> ignore
            //             return! json (a) next context 
            //         }
            GET >=> route "/houses" >=>
                fun next context ->
                    printf "here2"
                    let find = context.GetService<HouseAll>()
                    let houses = find()
                    json houses next context
            
            // GET >=> routef "/house/status/%s" (fun id ->
            //     fun next context ->
            //         let logger = context.Logger()
            //         logger.Information("Get status {@string}", id)
            //         let getStatus = context.GetService<HouseFindState>()
            //         let state = getStatus id
            //         json state next context
            // )

            PUT >=> routef "/house/command/%s/%s" (fun (house, remote) -> 
                fun next context ->
                    let logger = context.Logger()
                    let mqtt = context.GetService<SendMqttCommand>()
                    task {
                       let! command = context.ReadBodyFromRequestAsync()
                       logger.Information("got command to send: {0} {1} {2}", house, remote, command)
                       mqtt (house, remote, command)
                       return! text "" next context
                    }
            )
        ]    