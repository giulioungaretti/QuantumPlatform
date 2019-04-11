module Grains

open System.Threading.Tasks
open Microsoft.Extensions.Logging

open FSharp.Control.Tasks

open Orleans
open Orleans.EventSourcing
open Orleans.Providers

open Interfaces
open GrainState
open Shared

[<StorageProvider(ProviderName = "OrleansStorage")>]
type SampleStateGrain(log:ILogger<SampleStateGrain>) =
    inherit JournaledGrain<SampleHolder, Event>()
    interface ISampleState with
        member this.GetSample(): Task<Sample option> = 
            Task.FromResult this.State.Sample

        member this.NewSample(sample: Sample): Task = 
            match this.State.Sample with
            | Some _x ->
                failwith Error.SampleExists
            | None ->
                this.RaiseEvent(Event.NewSample sample)
                this.ConfirmEvents ()
        member this.NewStep(step: Step): Task = 
            this.RaiseEvent(Event.NewStep step)
            this.ConfirmEvents ()
        
        member this.Events ()=
            let retrieve = this.RetrieveConfirmedEvents(0, this.Version)
            task {
                let! retrieved =  retrieve
                return List.ofSeq retrieved
            }
 

type SampleGrain(log:ILogger<SampleGrain>) =
    inherit Grain<SampleHolder>()
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
    inherit Grain<Register<ISampleState>>()
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
