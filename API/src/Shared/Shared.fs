namespace Shared

[<CLIMutable>]
type Sample =
    { Name : string option
      Time : int64 }
    member this.toTime = 
        this.Time
        |> System.DateTimeOffset.FromUnixTimeSeconds


[<CLIMutable>]
type Samples =  string list
