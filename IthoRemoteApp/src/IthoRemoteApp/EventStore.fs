﻿module MyEventStore

open EventStore.ClientAPI
open System
open System.Net
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.DependencyInjection

open IthoRemoteApp.Json
open IthoRemoteApp.Signalr

module log = 
    let log = Serilog.Log.Logger
    let Information =
        log.Information
    let Fatal = 
        log.Fatal
open log


let streamName = "newstream"
let status = "$projections-states-result"

// // printf "md = %A\n" metaData.MaxCount
// let uc = SystemData.UserCredentials("itho", "YZ9fuf7%0I1z")
let uc = SystemData.UserCredentials("wim", "ijs^4@#Q8U1t")
//printf "uc = %A\n" uc
// let r = conn.SetStreamMetadataAsync(streamName, ExpectedVersion.Any |> int64, metaData, uc)

let metadata = "{ }"B
let connection =
        ConnectionSettings.configureStart()
        //|> ConnectionSettings.enableVerboseLogging
        |> ConnectionSettings.keepReconnecting
        |> ConnectionSettings.useConsoleLogger
        |> ConnectionSettings.configureEnd (IPEndPoint.Parse "167.99.32.103:1113")

let getData (e: ResolvedEvent) = 
    e.Event.Data

let getLastStateEvent () =
    Conn.readEvent connection "$projections-states-result" (EventVersion.LastInStream) ResolveLinksStrategy.DontResolveLinks uc
    |> Async.RunSynchronously
    

let addEvent data =
    let s = serialize data |> Text.Encoding.ASCII.GetBytes
    let eventPayload = 
        EventData(Guid.NewGuid(), data.GetType().Name, true, s, metadata)
        |> wrapEventData
    async {
        Conn.append connection uc streamName Any [ eventPayload ] |> ignore
    } |> Async.Start |> ignore

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
         .SetMaxCount(15L)
         .Build()
    Conn.setMetadata connection "$projections-fanStates-result" Any metaData uc |> Async.RunSynchronously
    Conn.setMetadata connection "newstream" Any metaData uc |> Async.RunSynchronously
    Conn.setMetadata connection "$projections-states-result" Any metaData uc |> Async.RunSynchronously

type EventStoreConnection (sp: IServiceProvider) =
    let _hub = sp.GetService<IHubContext<IthoHub>>()

    let handler a b = 
        let d = getData b  |> System.Text.Encoding.ASCII.GetString
        IthoRemoteApp.DomainTypes.currentState <- d
        sprintf "new status %A" d |> Information
        _hub.Clients.All.SendAsync("state", d) |> Async.AwaitTask |> Async.RunSynchronously

    let rec dropped 
        (aaa:EventStoreSubscription) 
        (bbb:SubscriptionDropReason) 
        (ccc:exn) =
        try
            sprintf "dropped connection %A %A %A\n" aaa bbb ccc.GetType   |> Information
            Async.Sleep(10000) |> Async.RunSynchronously
            Information "try reconnect"
            let s = Conn.catchUp connection status ResolveLinks handler (Some dropped) uc
            let cts = new Threading.CancellationTokenSource()
            let s1 = Async.RunSynchronously (s, 1000, cts.Token)
            printf "s = %A\n" s1
        with
        | e -> 
            e.GetType().Name + ": " + e.Message |> sprintf "Unable to renew subscription %A"  |> Fatal
            exit 1
        ()


    do 
        "EventStoreConnection ctor" |> Information
        try
            Conn.connect(connection) |> Async.RunSynchronously
            configureMetaData connection
            let s = Conn.catchUp connection status ResolveLinks handler (Some dropped) uc
            let sub = s |> Async.RunSynchronously
            //printf "sub = %A\n" sub
            ()
        with
            | :? Exceptions.ConnectionClosedException as ex ->
                failwithf "Connection was closed"

