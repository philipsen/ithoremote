namespace IthoRemoteApp
module Log =
    let log = Serilog.Log.Logger
    let Information = log.Information
    let Fatal = log.Fatal
