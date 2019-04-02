open Microsoft.Extensions.Logging
open Orleans
open Orleans.Hosting
open System
open FSharp.Control.Tasks

open Grains
open Interfaces

let buildSiloHost () =
      let builder = SiloHostBuilder()
      builder
        .UseLocalhostClustering()
        .ConfigureApplicationParts(fun parts ->
            parts.AddApplicationPart(typeof<HelloGrain>.Assembly)
                  .AddApplicationPart(typeof<IHello>.Assembly)
                  .WithCodeGeneration() |> ignore)
        .ConfigureLogging(fun logging -> logging.AddConsole() |> ignore)
        .Build()

[<EntryPoint>]
let main _ =
    let t = task {
        let host = buildSiloHost ()
        do! host.StartAsync ()
        printfn "Press any keys to terminate..."
        Console.Read() |> ignore
        printfn "silohost is stopped"
        do! host.StopAsync()
    }

    t.Wait()

    0
