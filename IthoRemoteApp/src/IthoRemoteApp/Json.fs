
module IthoRemoteApp.Json

open Newtonsoft.Json

let serialize obj =
    JsonConvert.SerializeObject obj

