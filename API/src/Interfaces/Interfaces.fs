namespace Interfaces

open System
open System.Threading.Tasks

open Shared


[<Serializable>]
type SampleHolder(sample : Sample option) =
    new() = SampleHolder None
    member val Sample : Sample option = None with get, set


type Register<'a> () =
    member val set  = Collections.Generic.HashSet<'a> () with set, get

// a sample is identified by a guid
type ISample =
    inherit Orleans.IGrainWithGuidKey
    abstract GetSample : unit -> Task<Sample option>
    abstract SetSample : Sample -> Task


// one could think of a container as belonging to some user_id:int
type ISamples =
    inherit Orleans.IGrainWithIntegerKey
    abstract Register :  ISample-> Task<ISample>
    abstract All : unit -> Task<List<ISample>>