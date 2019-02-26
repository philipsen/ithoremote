open Giraffe
open Microsoft.AspNetCore.Hosting
open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe.GiraffeViewEngine

let index = 
      html [] [
          head [] [
              title [] [ str "Giraffe!" ]
          ]
          body [] [
              h1 [] [ str "Hello!" ]
              p [] [ str "A test of Giraffe and .NET Core"]
          ]
      ]

let webApp =
    choose [
        route "/ping"   >=> text "pong"
        route "/"       >=> htmlFile "/pages/index.html" 
        route "/aap" >=> (index |> renderHtmlDocument |> htmlString) ]

let configureApp (app : IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0