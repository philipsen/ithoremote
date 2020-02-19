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

open HttpHouse
open HousesMongoDB
open Signalr
open MqttConnection
open Microsoft.AspNetCore.Http

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
    .CreateLogger()

let config = 
  { SerilogConfig.defaults with 
      ErrorHandler = fun ex context -> 
        Log.Error ex.Message
        setStatusCode 500 >=> text ex.Message }

let appWithLogger = SerilogAdapter.Enable(webApp, config)

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

let configureLogging (builder : ILoggingBuilder) =
    builder
        .AddConsole()
        .AddDebug() |> ignore


let configureAppConfiguration  (context: WebHostBuilderContext) (config: IConfigurationBuilder) =  
    config
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables() |> ignore

type Startup() =
    member __.ConfigureServices (services : IServiceCollection) =
        Log.Logger.Information "configure services"
        services.AddCors()    |> ignore
        services.AddGiraffe() |> ignore
        services.AddSignalR() |> ignore
        services.AddHouseMongoDB(Db.houses) |> ignore 
        services.AddMqttService() |> ignore       

    member __.Configure (app : IApplicationBuilder) 
                        (loggerFactory : ILoggerFactory) =
        loggerFactory.AddSerilog() |> ignore
        app
            .UseHttpsRedirection()
            .UseCors(configureCors)
            .UseSignalR(fun routes -> routes.MapHub<IthoHub>(PathString("/ithoHub")) |> ignore)
            .UseGiraffe appWithLogger

        Connection (app.ApplicationServices) |> ignore
            

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .ConfigureAppConfiguration(configureAppConfiguration)
        .UseStartup<Startup>() 
        .Build()
        .Run()
    0
