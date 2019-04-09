module GiraffeServer.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Giraffe
open GiraffeServer.HttpHandlers
open Orleans
open Config
open URL

let webApp =
    choose [ subRoute URL.api
                 (choose
                      [ GET >=> choose [ route URL.sample >=> handleGetSample
                                         route URL.samples >=> handleGetSamples ]
                        POST >=> choose [ route URL.sample >=> handlePostSample ] ])
             setStatusCode 404 >=> text "Not Found giraffe" ]

// ---------------------------------
// Error handler
// ---------------------------------
let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError
        (ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------
let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080").AllowAnyMethod()
           .AllowAnyHeader() |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseGiraffe(webApp)

let configureLogging (context : WebHostBuilderContext)
    (builder : ILoggingBuilder) =
    let conf = context.Configuration.GetSection("Logging")
    builder.AddConfiguration(conf).AddConsole().AddDebug() |> ignore

let configureServices (services : IServiceCollection) =
    let conf = services.BuildServiceProvider().GetService<IConfiguration>()
    let url = conf.GetSection("OrleansTableUrl").Get<Config>().OrleansTableURL
    services.AddCors()
        .AddGiraffe()
        .AddSingleton<Giraffe.Serialization.Json.IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer())
        .AddSingleton<IClusterClient> (OrleansClient.client url)
        |> ignore

let configureAppConfiguration (context : WebHostBuilderContext)
    (config : IConfigurationBuilder) =
    config.AddJsonFile("appsettings.json", false, true)
          .AddJsonFile(sprintf "appsettings.%s.json"
                           context.HostingEnvironment.EnvironmentName, true,
                       true).AddEnvironmentVariables() |> ignore
    let env = context.HostingEnvironment
    (match env.IsDevelopment() with
     | true -> config.AddUserSecrets<Config>() |> ignore
     | false -> ())

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .ConfigureAppConfiguration(configureAppConfiguration)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0
