module HousesMongoDB

open House
open MongoDB.Driver
open Microsoft.Extensions.DependencyInjection

let find (collection : IMongoCollection<House>) (criteria : HouseCriteria) : House[] =
  match criteria with
  | HouseCriteria.All -> collection.Find(Builders.Filter.Empty).ToEnumerable() |> Seq.toArray

let findByName (collection : IMongoCollection<House>) (name : string) : House =
  let houses = collection.Find(fun x -> x.name = name).Single()
  houses

let save (collection : IMongoCollection<House>) (house : House) : House =
  let houses = collection.Find(fun x -> x.Id = house.Id).ToEnumerable()

  match Seq.isEmpty houses with
  | true -> collection.InsertOne house
  | false ->
      let filter = Builders<House>.Filter.Eq((fun x -> x.Id), house.Id)
      let update =
        Builders<House>.Update
          .Set((fun x -> x.name), house.name)

      collection.UpdateOne(filter, update) |> ignore

  house

type IServiceCollection with
  member this.AddHouseMongoDB(collection : IMongoCollection<House>) =
    this.AddSingleton<HouseFind>(find collection) |> ignore
    this.AddSingleton<HouseFindByName>(findByName collection) |> ignore
    this.AddSingleton<HouseSave>(save collection) |> ignore