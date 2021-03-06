﻿module IthoRemoteApp.EventStore

open EventStore.ClientAPI
open System
open System.Net

open IthoRemoteApp.Json
open IthoRemoteApp.ClientMessageService

open Log

let streamName = "newstream"
let status = "$projections-states-result"
let uc = SystemData.UserCredentials("wim", "ijs^4@#Q8U1t")

let metadata = "{ }"B
let connection =
        ConnectionSettings.configureStart()
        |> ConnectionSettings.keepReconnecting
        // |> ConnectionSettings.useConsoleLogger
        |> ConnectionSettings.configureEnd (IPEndPoint.Parse "167.99.32.103:1113")

let getLastStateEvent () =
    Conn.readEvent connection "$projections-states-result" LastInStream DontResolveLinks uc
    |> Async.RunSynchronously
    
let addEvent data =
    let s = serialize data |> Text.Encoding.ASCII.GetBytes
    let eventPayload = 
        EventData(Guid.NewGuid(), data.GetType().Name, true, s, metadata)
        |> wrapEventData
    async {
        Conn.append connection uc streamName Any [ eventPayload ] |> ignore
    } |> Async.Start

let addEventDelayed delay event =
    async {   
        do! Async.Sleep (delay * 1000 * 60)
        addEvent event
    } |> Async.Start

let storeEvent event =
    match event with 
    | Some event -> 
        addEvent event
    | _ -> ()

let configureMetaData connection =
    Information "configureMetaData"
    let metaData = 
        StreamMetadata.Build()
         .SetMaxCount(150L)
         .Build()
    [
        "newstream"
        "status"
        "$projections-fanStates-result"
        "$projections-states-wmt6test-result"
        "$projections-states-wmt10-result"
        "$stats-0.0.0.0:2113"
    ] |> List.iter (fun streamName -> 
        Conn.setMetadata connection streamName Any metaData uc |> Async.RunSynchronously
    )
    
// let configureProjections =
//     Information "config projections"
//     //let fs = IO.File.ReadAllText "../js/fanStates.js"
//     //sprintf "got %s\n\n" fs |> Information
//     let log2 = Common.Log.ConsoleLogger()
//     let ctx = {
//         logger = log2
//         ep = IPEndPoint.Parse "167.99.32.103:2113"
//         timeout = TimeSpan.FromMinutes 10.0
//         creds = uc
//     }
//     //Projections.getStatistics ctx "fanStates" |> Async.RunSynchronously |> sprintf "proj = %A" |> Information
//     //Projections.listAll ctx |> Async.RunSynchronously |> serialize |> sprintf "all %A" |> Information
//     //Projections.getState ctx "states"  |> Async.RunSynchronously |> serialize |> sprintf "states =  %A" |> Information
//     // Projections.updateQuery ctx "bla" fs |> Async.RunSynchronously
//     let projections = [
//         "fanStates"
//         "states"
//     ]

//     ()

let houseGetStatus house = 
    async {
        let! s = Conn.readEvent connection ("$projections-states-"+ house + "-result") LastInStream DontResolveLinks uc
        return s.Event    
    }

let handlerFanstateUpdate _ (event: ResolvedEvent) =
    let status = event.Event.Data  |> System.Text.Encoding.ASCII.GetString
    sendToClients ("fanstates", status)
    
let handlerStatus _ (event: ResolvedEvent) = 
    let house = match event.Event.StreamId.Split "-" with
                | [| "$projections"; "states"; house; "result" |] -> house
                | _ -> ""
     
    let status = event.Event.Data  |> System.Text.Encoding.ASCII.GetString
    sprintf "new status2 %s -> %A" house status |> Information
    sendToClients ("state/" + house, status) 

let rec dropped subscription reason ex =
    sprintf "dropped connection %A %A %A\n" subscription reason ex.GetType   |> Fatal
    exit 1

let initSubsription() =
    "initSubsription" |> Information
    Conn.catchUp connection "status" ResolveLinks handlerStatus (Some dropped) uc |> Async.RunSynchronously |> ignore
    Conn.catchUp connection "$projections-fanStates-result" DontResolveLinks handlerFanstateUpdate (Some dropped) uc
    |> Async.RunSynchronously |> ignore

let onCloseTask s e =
    sprintf "on close" |> Information

let onRecoTask s e =
    sprintf "on reco" |> Information

// let onErrorTask s e =
//     sprintf "on error \n\n%A \n\n%A" (serialize s) (serialize e) |> Information
let onConnTask s e =
    sprintf "on conn sleep\n\n%A \n\n%A" s e |> Information
//     // Async.Sleep(10000) |> Async.RunSynchronously
//     // sprintf "sleep done" |> Information
//     // initSubsription()

let ehh1 = EventHandler<ClientClosedEventArgs> onCloseTask
// let ehh2 = EventHandler<ClientReconnectingEventArgs> onRecoTask
// let ehh3 = EventHandler<ClientConnectionEventArgs> onConnTask
// let ehh4 = EventHandler<ClientErrorEventArgs> onErrorTask

let InitializeEventStoreConnection() =
    sprintf "EventStoreConnection ctor" |> Information
    //configureProjections
    Conn.connect(connection) |> Async.RunSynchronously
    configureMetaData connection
    
    connection.Closed.AddHandler (EventHandler<ClientClosedEventArgs> onCloseTask)
    // connection.Reconnecting.AddHandler ehh2
    // connection.Connected.AddHandler ehh3
    // connection.ErrorOccurred.AddHandler ehh4
    initSubsription()
