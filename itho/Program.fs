  open Giraffe
  open Giraffe.GiraffeViewEngine
  open Microsoft.AspNetCore.Builder
  open Microsoft.Extensions.DependencyInjection
  open Microsoft.AspNetCore.Hosting
  //open Microsoft.AspNetCore.Mvc.UrlHelperExtensions
  open System
  
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
      choose [ route "/" >=> (index |> renderHtmlDocument |> htmlString) ]
  
  let configureApp (app : IApplicationBuilder) =
      app.UseGiraffe(webApp)
               
  let configureServices (services : IServiceCollection) =
      services.AddGiraffe() |> ignore
  
  //[]
  let main _ =
      WebHostBuilder()
          .UseKestrel()
          .Configure(Action<IApplicationBuilder>  configureApp)
          .ConfigureServices(configureServices)
          .Build()
          .Run()