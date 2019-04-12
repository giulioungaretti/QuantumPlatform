module GrainState

open System

open Interfaces

open Shared
open System.Collections.Generic

// satate
[<Serializable>]
type SampleHolder(sample : Sample option) =
    new() = SampleHolder None
    member val Sample : Sample option = sample with get, set
    member val Measurements = new Dictionary<System.Guid, IMeasurementState> () with get,set
    // member val Measurements = Map.empty with get, set


    member this.Apply (event: Event) : SampleHolder =
        match event with
        | NewSample sample ->
            printfn "new sample %O" sample
            this.Sample <- Some sample
            this
        | NewStep step ->
            printfn "new step %O" step
            this
        | Event.NewMeasurement (guid, meas) ->
            printfn "new measurement %O" guid
            this.Measurements.Add(guid, meas)
            //  this.Measurements.Add (guid, meas)
            this

            

[<Serializable>]
type MeasurementHolder(measurement : Measurement option) =
    new() = MeasurementHolder None
    member val Measurement : Measurement option = measurement with get, set

    member this.Apply (event: MeasurementEvent) : MeasurementHolder =
        match event with
        | MeasurementEvent.NewMeasurement measurement ->
            printfn "new Measurement %O" measurement 
            this.Measurement <- Some measurement 
            this
        | NewRun run ->
            printfn "new step %O" run
            this

            
