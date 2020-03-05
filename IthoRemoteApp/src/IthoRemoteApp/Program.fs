module IthoRemoteApp.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.SerilogExtensions
open Serilog 
open IthoRemoteApp.HttpHandlers

open IthoRemoteApp.HouseService
open IthoRemoteApp.Signalr

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        subRoute "/api"
            (choose [
                handlers
            ])
        setStatusCode 404 >=> text "Not Found" ]

// Enable logging on an exisiting HttpHandler 
let webAppWithLogging = SerilogAdapter.Enable(webApp)

// Configure Serilog: sinks and enrichers go here
Log.Logger <- 
  LoggerConfiguration()
    // add native destructuring
    .Destructure.FSharpTypes()
    // from Serilog.Sinks.Console
    .WriteTo.Console() 
    .CreateLogger() 

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : Microsoft.Extensions.Logging.ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:4200")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    match (env.EnvironmentName = "development") with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler
    |> ignore

    app.UseHttpsRedirection() |> ignore
    app.UseCors(configureCors) |> ignore
    //app.UseSignalR(fun routes -> routes.MapHub<IthoHub>(PathString("/ithoHub")) |> ignore)  |> ignore
    app.UseRouting() |> ignore
    app.UseEndpoints(fun endpoints ->
        let ps = PathString("/ithoHub")
        printf "map ep %s" (ps.ToString())
        endpoints.MapHub<IthoHub>(ps.ToString()) |> ignore
     ) |> ignore
    app.UseGiraffe(webAppWithLogging) |> ignore    
    Mqtt.MqttConnection (app.ApplicationServices) |> ignore
    MyEventStore.EventStoreConnection (app.ApplicationServices) |> ignore


let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services.AddSignalR() |> ignore
    services.AddHouseService() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l ->  l.Equals LogLevel.Debug |> not)
           .AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    // Mqtt.connect
    WebHostBuilder()
        .UseKestrel()
        .UseIISIntegration()
        
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        //.ConfigureLogging(configureLogging)
        // .UseUrls("http://0.0.0.0:5000,https://0.0.0.0:5001")
        .Build()
        .Run()
    0