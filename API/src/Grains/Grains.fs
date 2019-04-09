module Grains

open System.Threading.Tasks
open Orleans
open Orleans.Providers
open FSharp.Control.Tasks
open Interfaces
open Shared
open System

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
    // implement dumb comparison so we can save the grain in a set
    // and compare by grainID 
    interface IComparable<SampleGrain> with
        member this.CompareTo other =
            let k = this.GetPrimaryKey ()
            let ok = other.GetPrimaryKey ()
            compare k ok
    interface IComparable with
        member this.CompareTo(obj: obj): int = 
            match obj with 
                | :? Grain as other -> compare (this.GetPrimaryKey()) (other.GetPrimaryKey())
                // "random" in any other case (which make sense for this case since we use a set
                // and set contains items of same type
                | _ -> 1



[<StorageProvider(ProviderName = "OrleansStorage")>]
type SamplesGrain() = 
    inherit Grain<Register<SampleGrain>>()
    interface ISamples<SampleGrain> with
        member this.All(): Task<List<SampleGrain>> = 
            this.State.set 
            |> Set.toList
            |> Task.FromResult

        member this.Register sampleG =
            this.State.set.Add sampleG |> ignore
            Task.FromResult sampleG
