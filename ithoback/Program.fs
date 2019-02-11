module ithoback.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

open Giraffe

open Giraffe.SerilogExtensions
open Serilog
open Serilog.Configuration
open Serilog.Formatting.Json
open Serilog.Sinks

open HttpHouse
open HousesMongoDB
open MqttConnection
open Signalr
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.SignalR

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        subRoute "/api"
            (choose [
                HttpHouse.handlers
            ])
        setStatusCode 404 >=> text "Not Found2" ]


Log.Logger <- 
  LoggerConfiguration()
    .MinimumLevel.Debug()
    .Destructure.FSharpTypes()
    .WriteTo.Console()
    //.WriteTo.Console(JsonFormatter())
    .CreateLogger()

Log.Information "this is a message"

let config = 
  { SerilogConfig.defaults with 
      ErrorHandler = fun ex context -> 
        Log.Error ex.Message
        setStatusCode 500 >=> text ex.Message }
let appWithLogger = SerilogAdapter.Enable(webApp, config)


// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    Log.Error(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message


let configureCors (builder : CorsPolicyBuilder) =
    builder
           .WithOrigins("http://localhost:4200")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials()
           |> ignore


let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    // services.AddLogging(fun l -> l.AddSerilog(Log.Logger, false)) |> ignore
    

let configureLogging (builder : ILoggingBuilder) =
    builder
        //.AddFilter(fun l -> l.Equals LogLevel.Error)
        .AddConsole()
        .AddDebug() |> ignore


let configureAppConfiguration  (context: WebHostBuilderContext) (config: IConfigurationBuilder) =  
    config
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables() |> ignore


       
type Startup() =
    member __.ConfigureServices (services : IServiceCollection) =
        services.AddCors()    |> ignore
        services.AddGiraffe() |> ignore
        services.AddSignalR() |> ignore
        services.AddHouseMongoDB(Db.houses) |> ignore
        // services.AddEventMongoDB(Db.events) |> ignore
        // services.AddStateGet(Db.events) |> ignore
        //services.AddSingleton<MailboxProcessor<Message>>(agentFactory) |> ignore
        services.AddMqtt() |> ignore

    member __.Configure (app : IApplicationBuilder)
                        (env : IHostingEnvironment)
                        (loggerFactory : ILoggerFactory) =
        loggerFactory.AddSerilog() |> ignore
        app
            .UseHttpsRedirection()
            .UseCors(configureCors)
            .UseSignalR(fun routes -> routes.MapHub<IthoHub>(PathString("/ithoHub")) |> ignore)
            .UseGiraffe appWithLogger    
            

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .ConfigureAppConfiguration(configureAppConfiguration)
        .UseStartup<Startup>()
        .ConfigureLogging(configureLogging)        
        .Build()
        .Run()
    0



// module Bla = 
//     let manager = Startup.__serviceProvider.GetRequiredService<IConnectionManager>();
//     let hub = manager.GetHubContext<ChatHub>();
