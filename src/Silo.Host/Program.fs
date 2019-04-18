open System.IO
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Config
open Orleans
open Orleans.Configuration
open Orleans.Hosting
open Interfaces
open Grains

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
        .AddMemoryGrainStorage("OrleansStorage")
        #if DEBUG
        .UseLocalhostClustering()
        #else
        .UseAzureStorageClustering(fun (options : AzureStorageClusteringOptions) ->
                                            options.ConnectionString <- url)
        #endif
        .ConfigureApplicationParts(fun parts ->
                    parts.AddApplicationPart(typeof<SampleGrain>.Assembly)
                         .AddApplicationPart(typeof<ISample>.Assembly)
                         .AddApplicationPart(typeof<SamplesGrain>.Assembly)
                         .AddApplicationPart(typeof<ISamples>.Assembly)
                         .AddApplicationPart(typeof<SampleStateGrain>.Assembly)
                         .AddApplicationPart(typeof<ISampleState>.Assembly)
                         .AddApplicationPart(typeof<MeasurementStateGrain>.Assembly)
                         .AddApplicationPart(typeof<IMeasurementState>.Assembly)
                         .AddApplicationPart(typeof<MeasurementsGrain>.Assembly)
                         .AddApplicationPart(typeof<IMeasurements>.Assembly)
                         .WithCodeGeneration()
                    |> ignore)
    |> ignore

let configureAppConfiguration (context : Microsoft.Extensions.Hosting.HostBuilderContext)
    (config : IConfigurationBuilder) =
    config.SetBasePath(Directory.GetCurrentDirectory())
          // TODO:  find a way to share appsettings.json
          .AddJsonFile("appsettings.json", false, true)
          .AddJsonFile(sprintf "appsettings.%s.json"
                           context.HostingEnvironment.EnvironmentName, true,
                       true).AddEnvironmentVariables() |> ignore
    let env = context.HostingEnvironment
    (match env.IsDevelopment() with
     | true -> config.AddUserSecrets<Config>() |> ignore
     | false -> ())

let configureLogging (context : Microsoft.Extensions.Hosting.HostBuilderContext) (builder : ILoggingBuilder) =
    let conf = context.Configuration.GetSection("Logging")
    builder.AddConfiguration(conf).AddConsole() |> ignore

let configureHostConfiguration (configHost : IConfigurationBuilder) =
    configHost.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("hostsettings.json").AddEnvironmentVariables()
    |> ignore

[<EntryPoint>]
let main _ =
    HostBuilder()
        .ConfigureHostConfiguration(fun c -> (configureHostConfiguration c))
        .ConfigureAppConfiguration(configureAppConfiguration)
        .ConfigureLogging(configureLogging)
        .UseOrleans(buildSiloHost)
        .Build()
        .Run()
    0
