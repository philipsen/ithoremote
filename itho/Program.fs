﻿open Giraffe
open Microsoft.AspNetCore.Hosting
open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe.GiraffeViewEngine
open Microsoft.Extensions.FileProviders
open Microsoft.AspNetCore.Http
open System.IO


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

let webApp =
   choose [ route "/" >=> (index |> renderHtmlDocument |> htmlString) ]

let configureApp (app : IApplicationBuilder) =
  app.UseStaticFiles(
      StaticFileOptions(
          FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "frontend", "dist", "frontend")), RequestPath = PathString("/app")))
      .UseGiraffe(webApp)

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