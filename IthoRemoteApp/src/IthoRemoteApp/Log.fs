namespace IthoRemoteApp
module Log =
    let log = Serilog.Log.Logger
    let Information = log.Information
    let Error = log.Error
    let Fatal = log.Fatal
