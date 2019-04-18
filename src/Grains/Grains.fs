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
            match this.State.Sample with
            | Some _x ->
                this.RaiseEvent(Event.NewStep step)
                this.ConfirmEvents ()
            | None ->
                failwith Error.SampleDoesNotExists

         
        member this.NewMeasurement(guid): Task=
            let measurementGrainRef = this.GrainFactory.GetGrain<IMeasurementState> guid
            this.RaiseEvent(Event.NewMeasurement (guid, measurementGrainRef))
            this.ConfirmEvents ()
        
        member this.GetMeasurements () : Task<System.Collections.Generic.Dictionary<System.Guid, IMeasurementState>> =
            log.LogDebug("getting measurements from sample")
            Task.FromResult this.State.Measurements
        
        member this.Events ()=
            let retrieve = this.RetrieveConfirmedEvents(0, this.Version)
            task {
                let! retrieved =  retrieve
                return List.ofSeq retrieved
            }

[<StorageProvider(ProviderName = "OrleansStorage")>]
type MeasurementStateGrain(log:ILogger<MeasurementStateGrain>) =
    inherit JournaledGrain<MeasurementHolder, MeasurementEvent>()
    // NOTE: this is needed because F# https://github.com/Microsoft/visualfsharp/issues/2307
    member __.Grain = base.GrainFactory.GetGrain<ISampleState>
    member __.RaiseEvent m= base.RaiseEvent(MeasurementEvent.NewMeasurement m)
    member __.ConfirmEvents() = base.ConfirmEvents()
    interface IMeasurementState with
        member this.GetMeasurement(): Task<Measurement option> = 
            Task.FromResult this.State.Measurement


        member this.NewMeasurement(measurement: Measurement): Task = 
            match this.State.Measurement with
            | Some _x ->
                failwith Error.MeasurementExists
            | None ->
                task{
                    let sampleGrain = this.Grain measurement.SampleGUID
                    let! sample = sampleGrain.GetSample ()
                    match sample with
                    | Some _ ->
                        this.RaiseEvent(measurement)
                        return! Task.WhenAll [
                                 this.ConfirmEvents ()
                                 sampleGrain.NewMeasurement measurement.GUID
                                    ]
                    | None ->
                       failwith Error.SampleDoesNotExists
                } :> Task

        member this.NewRun(run: Run): Task = 
            this.RaiseEvent(MeasurementEvent.NewRun run)
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

[<StorageProvider(ProviderName = "OrleansStorage")>]
type MeasurementsGrain(logger : ILogger<MeasurementsGrain>) = 
    inherit Grain<Register<IMeasurementState>>()
    member this.logger = logger
    interface IMeasurements with
        member this.All()  = 
            do this.logger.LogDebug("Measurements: {%a}", this.State.set)
            this.State.set
            |> List.ofSeq
            |> Task.FromResult

        member this.Register measurement =
            do this.logger.LogDebug("adding measurement: {%a}", measurement)
            this.State.set.Add measurement |> ignore
            this.WriteStateAsync()
