namespace HttpHouse

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open Giraffe
open Giraffe.SerilogExtensions

open House
open HouseStatusFactory

module HttpHouse =
    let handlers : HttpFunc -> HttpContext -> HttpFuncResult =
        choose [
            POST >=> route "/houses" >=>
                fun next context ->
                    task {
                        let save = context.GetService<HouseSave>()
                        let! house = context.BindJsonAsync<House>()
                        return! json (save house) next context
                    }
            GET >=> route "/houses" >=>
                fun next context ->
                    let find = context.GetService<HouseFind>()
                    let houses = find HouseCriteria.All
                    json houses next context
            
            GET >=> routef "/house/status/%s" (fun id ->
                fun next context ->
                    let logger = context.Logger()
                    logger.Information("Get status {@string}", id)
                    let getStatus = context.GetService<HouseFindState>()
                    let state = getStatus id
                    json state next context
            )

            GET >=> routef "/house/%s" (fun id ->
                fun next context ->
                    let find = context.GetService<HouseFindByName>()
                    let houses = find id
                    json houses next context
            )

        ]    
