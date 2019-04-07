module Grains

open System.Threading.Tasks
open Orleans
open Orleans.Providers
open FSharp.Control.Tasks
open Interfaces
open Shared

//TODO: add logging
[<StorageProvider(ProviderName = "OrleansStorage")>]
type SampleGrain() =
    inherit Grain<SampleHolder>()
    interface ISample with
        member this.GetSample() : Task<Sample option> =
            Task.FromResult this.State.Sample
        member this.SetSample(sample : Sample) : Task =
            this.State.Sample <- Some sample
            let write = this.WriteStateAsync()
            task { do! write } |> ignore
            Task.CompletedTask
