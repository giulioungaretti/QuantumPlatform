module Grains

open System.Threading.Tasks
open Microsoft.Extensions.Logging

open Orleans
open Orleans.Providers
open FSharp.Control.Tasks

open Interfaces
open Shared

//TODO: add logging

[<StorageProvider(ProviderName = "OrleansStorage")>]
type SampleGrain(log:ILogger<SampleGrain>) =
    inherit Grain<SampleHolder>()
    member  this.logger = log
    interface ISample with
        member this.GetSample() : Task<Sample option> =
            Task.FromResult this.State.Sample
        member this.SetSample(sample : Sample) : Task =
            this.State.Sample <- Some sample
            let write = this.WriteStateAsync()
            task { do! write } |> ignore
            Task.CompletedTask


[<StorageProvider(ProviderName = "OrleansStorage")>]
type SamplesGrain(logger : ILogger<SamplesGrain>) = 
    inherit Grain<Register<ISample>>()
    member this.logger = logger
    interface ISamples with
        member this.All()  = 
            do this.logger.LogDebug("Samples: {%a}", this.State.set)
            this.State.set
            |> List.ofSeq
            |> Task.FromResult

        member this.Register sampleG =
            do this.logger.LogDebug("adding sample: {%a}", sampleG)
            this.State.set.Add sampleG |> ignore
            let write = this.WriteStateAsync()
            task { do! write } |> ignore
            Task.FromResult sampleG
