module MyEventStore

open EventStore.ClientAPI
open System
open System.Net
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.DependencyInjection

open IthoRemoteApp.Domain
open IthoRemoteApp.Signalr

module log = 
    let log = Serilog.Log.Logger
    let Information =
        log.Information
open log


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
        do! Async.Sleep (delay * 1000)
        addEvent event
    } |> Async.Start

let storeEvent event =
    match event with 
    | Some event -> addEvent event
    | _ -> ()


let createIthoTransmitRequestEvents house remote command =
  let correlationId = Guid.NewGuid()
  let startEvent = {
    house = house
    remote = remote
    command = command
    correlationId = correlationId
  } 
  startEvent |> addEvent

  let delay = match command with
              | "cook1" -> Some 30
              | "cook2" -> Some 60
              | "timer1" -> Some 60
              | "timer2" -> Some 120
              | "timer3" -> Some 180
              | "s_timer1" -> Some 10
              | "s_timer2" -> Some 20
              | "s_timer3" -> Some 30
              | _ -> None
 
  match delay with
  | Some delay ->
      let event = {
        house = house
        remote = remote
        cancelCommand = command
        correlationId = correlationId
      }
      event |> addEventDelayed delay
  | _ -> ()
  

type EventStoreConnection (sp: IServiceProvider) =
    let _hub = sp.GetService<IHubContext<IthoHub>>()
        
    let handler a b = 
        let d = getData b  |> System.Text.Encoding.ASCII.GetString
        sprintf "new status %A\n" d |> Information
        _hub.Clients.All.SendAsync("state", d) |> Async.AwaitTask |> Async.RunSynchronously

    do 
        "EventStoreConnection ctor" |> Information 
        Conn.connect(connection) |> Async.RunSynchronously
        Conn.catchUp connection status ResolveLinks handler None uc |> ignore


