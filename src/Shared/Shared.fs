namespace Shared

[<CLIMutable>]
type Step =
    { Name : string option
      GUID : System.Guid
      Time : int64 }
    member this.toTime = 
        this.Time
        |> System.DateTimeOffset.FromUnixTimeSeconds

[<CLIMutable>]
type Steps = Step list

[<CLIMutable>]
type Sample =
    { Name : string option
      GUID : System.Guid
      Steps : Steps Option 
      Time : int64 }
    member this.toTime = 
        this.Time
        |> System.DateTimeOffset.FromUnixTimeSeconds
    member this.AddStep (step:Step) =
        match this.Steps with
                      | Some steps ->
                          {this with Steps =  Some <| List.append steps [step]}
                      | _ ->
                          {this with Steps =  Some <| List.singleton step}

[<CLIMutable>]
type Run =
    { Name : string option
      GUID : System.Guid
      Time : int64 }
    member this.toTime = 
        this.Time
        |> System.DateTimeOffset.FromUnixTimeSeconds

[<CLIMutable>]
type Runs = Run list


[<CLIMutable>]
type Measurement =
    { Name : string option
      GUID : System.Guid
      SampleGUID : System.Guid
      Runs : Runs option
      Time : int64 }
    member this.toTime = 
        this.Time
        |> System.DateTimeOffset.FromUnixTimeSeconds



[<CLIMutable>]
type Measurements =  Measurement list

[<CLIMutable>]
type Samples =  Sample list

[<RequireQualifiedAccessAttribute>]
module Error =
    [<Literal>]
    let  SampleExists = "SampleExists"
    let  SampleDoesNotExists = "SampleDoesNotExists"
    let  MeasurementExists = "MeasurementExists"