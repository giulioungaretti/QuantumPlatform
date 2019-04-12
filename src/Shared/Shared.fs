namespace Shared
// By adding this condition, you can share your code between your client and server
#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif


[<CLIMutable>]
type Sample =
    { Name : string option
      GUID : System.Guid
      Time : int64 }
    member this.toTime = 
        this.Time
        |> System.DateTimeOffset.FromUnixTimeSeconds


[<CLIMutable>]
type Step =
    { Name : string option
      GUID : System.Guid
      Time : int64 }
    member this.toTime = 
        this.Time
        |> System.DateTimeOffset.FromUnixTimeSeconds

[<CLIMutable>]
type Measurement =
    { Name : string option
      GUID : System.Guid
      Time : int64 }
    member this.toTime = 
        this.Time
        |> System.DateTimeOffset.FromUnixTimeSeconds


[<CLIMutable>]
type Run =
    { Name : string option
      GUID : System.Guid
      Time : int64 }
    member this.toTime = 
        this.Time
        |> System.DateTimeOffset.FromUnixTimeSeconds


[<CLIMutable>]
type Samples =  Sample list

[<RequireQualifiedAccessAttribute>]
module Error =
    [<Literal>]
    let  SampleExists = "SampleExists"