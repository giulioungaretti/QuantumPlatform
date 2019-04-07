open System.IO
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Configuration.Json
open Microsoft.Extensions.DependencyInjection
open Orleans
open Orleans.Configuration
open Orleans.Hosting
open Interfaces
open Grains
open Config

// NOTE TO SELF this magical generic host is a trap :/
// There are many subtle differences with the web one!
let buildSiloHost (context : Microsoft.Extensions.Hosting.HostBuilderContext)
    (siloBuilder : ISiloBuilder) =
    let conf = context.Configuration
    let url = conf.GetSection("OrleansTableUrl").Get<Config>().OrleansTableURL
    siloBuilder.Configure<ClusterOptions>(fun (options : ClusterOptions) ->
    options.ClusterId <- "orleans-dev-docker"
    options.ServiceId <- "FsharpOrleansDev")
        .ConfigureEndpoints(siloPort = 11111, gatewayPort = 30000)
        // .AddMemoryGrainStorage("OrleansStorage")
        .UseAzureStorageClustering(fun (options : AzureStorageClusteringOptions) ->
        options.ConnectionString <- url)
        .ConfigureApplicationParts(fun parts ->
        parts.AddApplicationPart(typeof<SampleGrain>.Assembly)
             .AddApplicationPart(typeof<ISample>.Assembly).WithCodeGeneration()
        |> ignore)
    |> ignore

let configureAppConfiguration (context : Microsoft.Extensions.Hosting.HostBuilderContext)
    (config : IConfigurationBuilder) =
    printf "%O" context.HostingEnvironment.EnvironmentName
    // TODO:  find a way to share appsettings.json
    config.AddJsonFile("appsettings.json", false, true)
          .AddJsonFile(sprintf "appsettings.%s.json"
                           context.HostingEnvironment.EnvironmentName, true,
                       true).AddEnvironmentVariables() |> ignore
    let env = context.HostingEnvironment
    (match env.IsDevelopment() with
     | true -> config.AddUserSecrets<Config>() |> ignore
     | false -> ())

let configureHostConfiguration (configHost : IConfigurationBuilder) =
    configHost.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("hostsettings.json").AddEnvironmentVariables()
    |> ignore

[<EntryPoint>]
let main _ =
    HostBuilder()
        .ConfigureHostConfiguration(fun c -> (configureHostConfiguration c))
        .UseOrleans(buildSiloHost)
        .ConfigureAppConfiguration(configureAppConfiguration)
        .ConfigureLogging(fun logging -> logging.AddConsole() |> ignore)
        .Build()
        .RunAsync() |> ignore
    0
