module Signalr
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging

type IthoHub (logger : ILogger<IthoHub>) =
    inherit Hub ()

    let _logger = logger    

    override this.OnConnectedAsync() =
        async{
            let id = this.Context.UserIdentifier
            id |> sprintf "a new client connected{%A}" |>  _logger.LogInformation
            id |> printf "a new client connected{%A}" 
            } |> Async.StartAsTask :> _
