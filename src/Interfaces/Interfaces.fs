namespace Interfaces

open System
open System.Threading.Tasks

open Shared

// events
[<Serializable>]
type Event =
    | NewSample of Sample
    | NewStep of Step

// a sample is identified by a guid
type ISample =
    inherit Orleans.IGrainWithGuidKey
    abstract GetSample : unit -> Task<Sample option>
    abstract SetSample : Sample -> Task

type ISampleState =
    inherit Orleans.IGrainWithGuidKey
    abstract GetSample : unit -> Task<Sample option>
    abstract NewSample : Sample -> Task
    abstract NewStep :  Step -> Task
    abstract Events : unit -> Task<Event list>


// Register is just a a container of things
type Register<'a> () =
    member val set  = Collections.Generic.HashSet<'a> () with set, get

// one could think of a container as belonging to some user_id:int
type ISamples =
    inherit Orleans.IGrainWithIntegerKey
    abstract Register :  ISampleState-> Task<ISampleState>
    abstract All : unit -> Task<List<ISampleState>>