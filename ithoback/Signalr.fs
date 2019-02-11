module Signalr
open Microsoft.AspNetCore.SignalR
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection

open System

type IGiraffeHubClient =
    interface
        abstract Send: msg: string -> Task
    end

type IthoHub (logger : ILogger<IthoHub>) =
    inherit Hub ()

    let _logger = logger    

    override this.OnConnectedAsync() =
        async{
            this.Context.UserIdentifier |> sprintf "a new client connected{%A}" |>  _logger.LogInformation
            } |> Async.StartAsTask :> _


let agentFactory(sp: IServiceProvider) =
    let _hub = sp.GetService<IHubContext<IthoHub>>()
    _hub.Clients.All.SendAsync("ithoHub", "{}")
    