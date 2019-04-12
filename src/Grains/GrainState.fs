module GrainState

open System

open Interfaces

open Shared

// satate
[<Serializable>]
type SampleHolder(sample : Sample option) =
    new() = SampleHolder None
    member val Sample : Sample option = sample with get, set
    member val Measurement = Map.empty with get, set


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
            this.Measurement <- 
             this.Measurement.Add (guid, meas)
            this

            
