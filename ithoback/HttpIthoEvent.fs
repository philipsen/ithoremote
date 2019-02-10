namespace HttpIthoEvent

open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open System

open Giraffe.SerilogExtensions
open Serilog
open Serilog.Formatting.Json
open IthoEvent

module HttpEvent =
    let handlers : HttpFunc -> HttpContext -> HttpFuncResult =
        choose [
            GET >=> routef "/events/%s" (fun (id) ->
                fun next context ->
                    let logger = context.Logger()
                    logger.Information("Get events for {@string", id)
                    let find = context.GetService<EventFind>()
                    let events = id |> EventCriteria.ByName |> find
                    json events next context
            )
            GET >=> route "/events" >=>
                fun next context ->
                    let logger = context.Logger()
                    logger.Information("Get all events")
                    let find = context.GetService<EventFind>()
                    let events = find EventCriteria.All
                    json events next context
        ]    
