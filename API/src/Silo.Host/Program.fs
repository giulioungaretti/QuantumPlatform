open Orleans
open Orleans.Hosting
open System
open FSharp.Control.Tasks
open System.Threading;

open Grains
open Interfaces
open System.Runtime.Loader
open Microsoft.Extensions.Logging
open Orleans.Configuration


let buildSiloHost () =
      let builder = SiloHostBuilder()
      builder
        .Configure<ClusterOptions>(fun (options:ClusterOptions) -> 
              options.ClusterId <- "orleans-dev-docker"; 
              options.ServiceId <- "FsharpOrleansDev")
        .ConfigureEndpoints(siloPort=11111, gatewayPort=30000)
        .UseAzureStorageClustering(fun (options:AzureStorageClusteringOptions)-> 
               options.ConnectionString <- connectionString )
        .ConfigureApplicationParts(fun parts ->
            parts.AddApplicationPart(typeof<HelloGrain>.Assembly)
                  .AddApplicationPart(typeof<IHello>.Assembly)
                  .WithCodeGeneration() |> ignore)
        .ConfigureLogging(fun logging -> logging.AddConsole() |> ignore)
        .Build()


[<EntryPoint>]
let main _ =
    let siloStopped = new ManualResetEvent(false);
    let host = buildSiloHost ()
    let start = async {
      Async.AwaitTask (host.StartAsync ())
      |> ignore
      printfn "started silo"
    }

    let shutdonw = async {
        Async.AwaitTask ( host.StopAsync () ) |> ignore
        printfn "stopping silo"
        siloStopped.Set () |> ignore
    }
    System.Runtime.Loader.AssemblyLoadContext.Default.add_Unloading(fun _ -> 
      printfn "unloading assembly"
      Async.RunSynchronously shutdonw
      siloStopped.WaitOne() |> ignore
    )
    Async.RunSynchronously start
    siloStopped.WaitOne() |> ignore
    printfn "Exiting silo host"
    0
