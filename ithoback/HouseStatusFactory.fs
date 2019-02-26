
module HouseStatusFactory

open System
open Serilog
open Microsoft.Extensions.DependencyInjection

open IthoEvent
open EventsMongoDB
open Domain

let log = LoggerConfiguration().WriteTo.Console().CreateLogger()
log.Information "Load HouseStatusFactory"

module CmdArgs =    
    type CommandTask = { 
        Command : string  
        Kind: string
        OccurredOn: DateTime 
    }

type Command = 
    | MakeCommandHappen of CmdArgs.CommandTask

type Event =
    | CommandHappend of CmdArgs.CommandTask

type State = {  
    ventilation: Target
    ventilationBaseState: VentilationBaseState
    endTimeCommand: DateTime
}
    with static member Init = { 
            ventilation = Off 
            ventilationBaseState = VentilationBaseState.Comfort
            endTimeCommand = DateTime.Now
        }

module Aggregate =
    type Aggregate<'state, 'command, 'event> = {
        Init : 'state
        Apply: 'state -> 'event -> 'state
        Execute: 'state -> 'command -> 'event list
    }

    let execute state command = 
        match command with
        | MakeCommandHappen args -> args.Command |> (fun _ -> CommandHappend args)

    let apply (state: State) event =
        match event with
        | CommandHappend args -> 
            let nv = 
                match args.Command with
                | "cook1" -> Keuken
                | "cook2" -> Keuken
                | "eco" -> Eco
                | "comfort" -> Comfort
                | "s_timer1" 
                | "s_timer2" -> Badkamer
                | _ -> Off

            let nb = match args.Command with 
                     | "eco" -> VentilationBaseState.Eco
                     | "comfort" -> VentilationBaseState.Comfort
                     | _ -> state.ventilationBaseState

            let minutes = 
                match args.Command with
                | "cook1" -> 30.0
                | "cook2" -> 60.0
                | "s_timer1" -> 10.0
                | "s_timer2" -> 20.2
                | _ -> 0.0
            let newEnd = args.OccurredOn.AddMinutes minutes

            { state with ventilation = nv; ventilationBaseState = nb ; endTimeCommand = newEnd }
                
    let aggregate = {
        Init = State.Init
        Execute = fun s c -> execute s c |> List.singleton
        Apply = apply
    }

module Mapping =
    let toDomainEvent (ev: IthoEvent) =
        CommandHappend {
            Command = ev.command
            Kind = ev.kind
            OccurredOn = ev.time.ToUniversalTime()
        }


let getEvents (eventCollection) (house: string) : IthoEvent[] = 
    house |> EventCriteria.ByName |> find eventCollection

let getState (eventCollection) (house) =
    getEvents eventCollection house
        |> Seq.map Mapping.toDomainEvent 
        |> Seq.fold Aggregate.aggregate.Apply State.Init

type HouseFindState = string -> State

type IServiceCollection with
    member this.AddStateGet eventCollection =
        this.AddSingleton<HouseFindState>(getState eventCollection) |> ignore
