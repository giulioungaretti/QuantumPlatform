namespace Interfaces

open System
open System.Threading.Tasks
open Shared
open Orleans
open System
open System


[<Serializable>]
type SampleHolder(sample : Sample option) =
    new() = SampleHolder None
    member val Sample : Sample option = None with get, set

type Register<'a when 'a:comparison> () =
    member __.set  = Set.empty<'a> 


// a sample is identified by a guid
type ISample =
    inherit Orleans.IGrainWithGuidKey
    abstract GetSample : unit -> Task<Sample option>
    abstract SetSample : Sample -> Task


// one could think of a container as belonging to some user_id:int
type ISamples<'a when 'a :> IGrain> =
    inherit Orleans.IGrainWithIntegerKey
    abstract Register : 'a -> Task<'a>
    abstract All : unit -> Task<List<'a>>