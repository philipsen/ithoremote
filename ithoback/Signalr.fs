module Signalr
open Microsoft.AspNetCore.SignalR



//[<HubName("myFirstHub")>]
type IthoHub () =
    inherit Hub ()

    member x.MessageToTheServer message =
        printfn "Someone sent us a message! - %s" message

// type Server (host:string) =
//     let startup (a:IAppBuilder) =
//         a.UseCors(CorsOptions.AllowAll) |> ignore
//         a.MapSignalR() |> ignore

//     let clients = GlobalHost.ConnectionManager.GetHubContext<MyFirstHub>().Clients

//     do
//         WebApp.Start(host, startup) |> ignore
//         printfn "Signalr server running on %s" host

//     member x.Send message = clients.All?messageToTheClient message        