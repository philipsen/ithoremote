module MyEventStore

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

let getLastStateEvent () =
    Conn.readEvent connection "$projections-states-result" (EventVersion.LastInStream) ResolveLinksStrategy.DontResolveLinks uc
    |> Async.RunSynchronously
    

let addEvent data =
    let s = serialize data |> Text.Encoding.ASCII.GetBytes
    let eventPayload = 
        EventData(Guid.NewGuid(), data.GetType().Name, true, s, metadata)
        |> wrapEventData
    async {
        Conn.append connection uc streamName ExpectedVersionUnion.Any [ eventPayload ] |> ignore
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

type EventStoreConnection (sp: IServiceProvider) =
    let _hub = sp.GetService<IHubContext<IthoHub>>()

    let handler a b = 
        let d = getData b  |> System.Text.Encoding.ASCII.GetString
        sprintf "new status %A" d |> Information
        //_hub.Clients.All.SendAsync("state", d) |> Async.AwaitTask |> Async.RunSynchronously

    let rec dropped 
        (aaa:EventStoreSubscription) 
        (bbb:SubscriptionDropReason) 
        (ccc:exn) =
        sprintf "dropped connection %A %A %A" aaa bbb 1   |> Information
        printf "start new\n"
        let s = Conn.catchUp connection status ResolveLinks handler (Some dropped) uc
        let s1 = s |> Async.RunSynchronously
        printf "s = %A" s1
        ()

    do 
        "EventStoreConnection ctor" |> Information
        Conn.connect(connection) |> Async.RunSynchronously
        let s = Conn.catchUp connection status ResolveLinks handler (Some dropped) uc
        s |> Async.RunSynchronously |> ignore
