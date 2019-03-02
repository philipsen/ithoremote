open Giraffe
open Microsoft.AspNetCore.Hosting
open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe.GiraffeViewEngine
open Microsoft.Extensions.FileProviders
open Microsoft.AspNetCore.Http
open System.IO

open Itho

let ngApp = tag "app-root"  [] []

let ngScripts = 
  [ "runtime.js"; "polyfills.js"; "styles.js"; "vendor.js"; "main.js" ]
  |> List.map (function js -> script [ _type "text/javascript"; _src js ] [] )

let index = 
  html [ _lang "en" ] [
      head [] [
          meta [ _charset "utf-8"; _name "viewport"; _content "width=device-width, initial-scale=1" ] 
          title [] [ str "Angular + Giraffe" ]
          ``base`` [ _href "/app/" ]
          link [ _rel "icon"; _type "icon"; _href "favicon.ico" ]
      ]

      body [] 
          (ngApp :: ngScripts)
  ]

type House = { name: string }

let housesList:(House list) = [
  { name = "wmt6" }
  { name = "wmt40" }
]

let webApp =
   choose [ 
     route "/app/" >=> (index |> renderHtmlDocument |> htmlString) 
     route "/app/houses" >=> (index |> renderHtmlDocument |> htmlString) 
     route "/api/houses" >=> (json housesList)
     ]

let configureApp (app : IApplicationBuilder) =
  app.UseStaticFiles(
      StaticFileOptions(
          FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "frontend", "dist", "frontend")), RequestPath = PathString("/app")))
      .UseSignalR(fun routes -> routes.MapHub<IthoHub>(PathString "/ithoHub")) // SignalR        
      .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore
    services.AddSignalR() |> ignore 
    services.AddHostedService<IthoService>() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0