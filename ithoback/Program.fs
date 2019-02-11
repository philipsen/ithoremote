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

open ithoback.HttpHandlers
open HttpHouse
open HousesMongoDB
open MqttConnection
open Signalr
open Microsoft.AspNetCore.Http

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


let config = 
  { SerilogConfig.defaults with 
      ErrorHandler = fun ex context -> 
        printf "error {%A}" ex
        setStatusCode 500 >=> text "internal error" }
let appWithLogger = SerilogAdapter.Enable(webApp, config)

Log.Logger <- 
  LoggerConfiguration()
    .Destructure.FSharpTypes()
    .WriteTo.Console()
    //.WriteTo.Console(new JsonFormatter())
    .CreateLogger()


// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    //logger.Error(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder
           .WithOrigins("http://localhost:4200")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials()
           |> ignore


let configureApp (app : IApplicationBuilder) (env : IHostingEnvironment) (loggerFactory : ILoggerFactory) =
    app
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseGiraffe(appWithLogger)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
           .AddConsole()
           .AddDebug() |> ignore


let configureAppConfiguration  (context: WebHostBuilderContext) (config: IConfigurationBuilder) =  
    config
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables() |> ignore

// let agentFactory(serviceProvider : IServiceProvider) =
//     let thing = serviceProvider.GetService<string>()
    // MailboxProcessor.Start(fun (inbox : MailboxProcessor<Message>) ->
    //     // /* loop implementation */
    //     // 0
    //     // printf "nope {%A}"
    // )

type Startup() =
    member __.ConfigureServices (services : IServiceCollection) =
        services.AddCors()    |> ignore
        services.AddGiraffe() |> ignore
        services.AddSignalR() |> ignore
        services.AddHouseMongoDB(Db.houses) |> ignore
        // services.AddEventMongoDB(Db.events) |> ignore
        // services.AddStateGet(Db.events) |> ignore
        // services.AddSingleton<MailboxProcessor<Message>>(agentFactory) |> ignore
        services.AddMqtt() |> ignore

    member __.Configure (app : IApplicationBuilder)
                        (env : IHostingEnvironment)
                        (loggerFactory : ILoggerFactory) =
        app
            .UseHttpsRedirection()
            .UseCors(configureCors)
            .UseSignalR(fun routes -> routes.MapHub<IthoHub>(PathString("/ithoHub")) |> ignore)
            //.UseWebSockets()
            //.UseMiddleware<WebSocketMiddleware>()
            .UseGiraffe appWithLogger

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        //.UseIISIntegration()
        // .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureAppConfiguration(configureAppConfiguration)
        .UseStartup<Startup>()
        // .ConfigureServices(configureServices)
        // .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0



// module Bla = 
//     let manager = Startup.__serviceProvider.GetRequiredService<IConnectionManager>();
//     let hub = manager.GetHubContext<ChatHub>();
