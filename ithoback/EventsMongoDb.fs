module EventsMongoDB

open IthoEvent
open MongoDB.Driver
open Microsoft.Extensions.DependencyInjection

let find (collection : IMongoCollection<IthoEvent>) (criteria : EventCriteria) : IthoEvent[] =
  match criteria with
  | EventCriteria.All -> collection.Find(Builders.Filter.Empty).ToEnumerable() |> Seq.toArray
  | EventCriteria.ByName(name) -> collection.Find(fun x -> x.house.Equals name).ToEnumerable() |> Seq.toArray

type IServiceCollection with
  member this.AddEventMongoDB(collection : IMongoCollection<IthoEvent>) =
    this.AddSingleton<EventFind>(find collection) |> ignore