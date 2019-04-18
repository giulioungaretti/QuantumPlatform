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
    // member val Steps : Shared.Steps option = None with get, set
    member val Measurements = new Dictionary<System.Guid, IMeasurementState> () with get,set

    member this.Apply (event: Event) : SampleHolder =
        match event with
        | NewSample sample ->
            printfn "new sample %O" sample
            this.Sample <- Some sample
            this
        | NewStep step ->
            match this.Sample with
            | Some sample ->
                printfn "new step %O" step
                this.Sample <- Some <| sample.AddStep step
                this
            | None ->
                this

        | Event.NewMeasurement (guid, meas) ->
            printfn "new measurement %O" guid
            this.Measurements.Add(guid, meas)
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

            
