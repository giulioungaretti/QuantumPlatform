module OrleansClient

open System
open Orleans
open Orleans.Configuration
open Orleans.Hosting
open System.Threading.Tasks
open FSharp.Control.Tasks
open Interfaces
open Grains
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging.Debug

let buildClient (url : string) (serviceProvider : IServiceProvider) =
    let builder = ClientBuilder()
    builder.Configure<ClusterOptions>(fun (options : ClusterOptions) ->
    options.ClusterId <- "orleans-dev-docker"
    options.ServiceId <- "FsharpOrleansDev")
        .UseAzureStorageClustering(fun (options : AzureStorageGatewayOptions) ->
        options.ConnectionString <- url)
        .ConfigureApplicationParts(fun parts ->
        parts.AddApplicationPart(typeof<SampleGrain>.Assembly)
             .AddApplicationPart(typeof<ISample>.Assembly).WithCodeGeneration()
        |> ignore)
        .Build()

let client (url : string) (serviceProvider : IServiceProvider) : IClusterClient =
    // NOTE: this seems like a dangerous patttern :/
    // hard to test, gives out nulls like candies and not very functional
    let log = serviceProvider.GetService<ILogger<Console>>()
    // NOTE: this mix of async + task makes me feel uneasy
    // but alas, I could not find another way to get ahold of a client
    async {
        let client = buildClient url serviceProvider
        client.Connect(fun (ex : Exception) ->
              task {
                  log.LogWarning
                      ("Exception while attempting to connect to Orleans cluster: {Exception}",
                       ex)
                  do! Task.Delay(1000)
                  return true
              }).GetAwaiter().GetResult()
        |> ignore
        log.LogInformation
            ("Orlean client initalized {%b}", client.IsInitialized)
        return client
    }
    |> Async.RunSynchronously
