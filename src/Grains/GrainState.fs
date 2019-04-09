module GrainState

open System

open Interfaces

open Shared



// satate
[<Serializable>]
type SampleHolder(sample : Sample option) =
    new() = SampleHolder None
    member val Sample : Sample option = None with get, set

    member this.Apply (event: Event) : SampleHolder =
        match event with
        | NewSample sample ->
            // log.
            printfn "new sample %O" sample
            this.Sample <- Some sample
            this
        | NewStep step ->
            printfn "new step %O" step
            this
