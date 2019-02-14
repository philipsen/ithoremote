module Signalr
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging

type IthoHub (logger : ILogger<IthoHub>) =
    inherit Hub ()

    let _logger = logger    

    override this.OnConnectedAsync() =
        async{
            this.Context.UserIdentifier |> sprintf "a new client connected{%A}" |>  _logger.LogInformation
            } |> Async.StartAsTask :> _
