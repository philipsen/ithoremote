module Db
open MongoDB.Driver
open IthoEvent
open House

// Register default Giraffe dependencies
let mongoUrl = "mongodb+srv://itho:aapnootmies@cluster0-xqsdh.mongodb.net?retryWrites=false"
let mongo = MongoClient (mongoUrl)
let db = mongo.GetDatabase "test"

let houses = db.GetCollection<House>("houses")
let events = db.GetCollection<IthoEvent>("events")