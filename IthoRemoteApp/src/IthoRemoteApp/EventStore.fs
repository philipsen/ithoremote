module MyEventStore

open EventStore.ClientAPI
open System
open System.Net
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.DependencyInjection

open IthoRemoteApp.Signalr

module log = 
    let log = Serilog.Log.Logger
    let Information =
        log.Information
open log


module Json =
  open Newtonsoft.Json

  let serialize obj =
    JsonConvert.SerializeObject obj



// let connSettings = "ConnectTo=tcp://itho:RoAb5!&19h6F@167.99.32.103:1113"
// let conn = EventStoreConnection.Create (connSettings)
// (conn.ConnectAsync()).Wait()


let streamName = "newstream"
let status = "$projections-states-result"
// let metaData = 
//     StreamMetadata.Build()
//      .SetMaxCount(50L)
//      .Build()

// // printf "md = %A\n" metaData.MaxCount

let uc = SystemData.UserCredentials("wim", "ijs^4@#Q8U1t")
printf "uc = %A\n" uc
// let r = conn.SetStreamMetadataAsync(streamName, ExpectedVersion.Any |> int64, metaData, uc)

let metadata = "{ }"B
let connection =
        ConnectionSettings.configureStart()
        |> ConnectionSettings.configureEnd (IPEndPoint.Parse "167.99.32.103:1113")

let getData (e: ResolvedEvent) = 
    e.Event.Data

let dropped a b c =
    printf "dropped\n"

let addEvent data =
    let s = Json.serialize data |> Text.Encoding.ASCII.GetBytes
    let eventPayload = 
        EventData(Guid.NewGuid(), data.GetType().Name, true, s, metadata)
        |> wrapEventData
    Conn.append connection uc streamName ExpectedVersionUnion.Any [ eventPayload ]

let addEventDelayed delay event =
    async {   
        do! Async.Sleep (delay * 1000 * 60)
        addEvent event
    } |> Async.Start

let storeEvent event =
    match event with 
    | Some event -> addEvent event
    | _ -> ()

type EventStoreConnection (sp: IServiceProvider) =
    let _hub = sp.GetService<IHubContext<IthoHub>>()
        
    let handler a b = 
        let d = getData b  |> System.Text.Encoding.ASCII.GetString
        sprintf "new status %A" d |> Information
        _hub.Clients.All.SendAsync("state", d) |> Async.AwaitTask |> Async.RunSynchronously

    do 
        "EventStoreConnection ctor" |> Information 
        Conn.connect(connection) |> Async.RunSynchronously
        Conn.catchUp connection status ResolveLinks handler None uc |> ignore


