module IthoRemoteApp.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.SerilogExtensions
open Serilog 
open IthoRemoteApp.HttpHandlers

open IthoRemoteApp.Signalr
open IthoRemoteApp.Mqtt
open IthoRemoteApp.ClientMessageService
open IthoRemoteApp.EventStore
open Microsoft.Extensions.Configuration

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
    app.UseRouting() |> ignore
    app.UseEndpoints(fun endpoints ->
        endpoints.MapHub<IthoHub>(PathString("/ithoHub").ToString()) |> ignore
     ) |> ignore
    app.UseGiraffe(webAppWithLogging) |> ignore 
    Common.Hub <- app.ApplicationServices.GetService<IHubContext<IthoHub>>()
    
    InitializeMqtt(app.ApplicationServices.GetService<IConfiguration>()) |> ignore
    InitializeEventStoreConnection() |> ignore

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services.AddSignalR() |> ignore

// let configureLogging (builder : ILoggingBuilder) =
//     builder.AddFilter(fun l ->  l.Equals LogLevel.Debug |> not)
//            .AddConsole()
//            .AddDebug() |> ignore

let configureAppConfiguration  (context: WebHostBuilderContext) (config: IConfigurationBuilder) =  
    config
        .AddJsonFile("appsettings.json",false,true)
        .AddJsonFile(sprintf "appsettings.%s.json" context.HostingEnvironment.EnvironmentName ,true)
        .AddEnvironmentVariables() |> ignore

        
[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .UseIISIntegration()        
        .ConfigureAppConfiguration(configureAppConfiguration)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0