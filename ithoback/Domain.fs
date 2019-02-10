namespace Domain
open Serilog

module log = 
    let log = LoggerConfiguration().WriteTo.Console().CreateLogger()
    log.Information "Load Domain"

type VentilationBaseState =
    | Eco
    | Comfort

type Target =
    | Keuken
    | Badkamer
    | Wc
    | Eco
    | Comfort
    | Off
