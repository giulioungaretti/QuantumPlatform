namespace Interfaces

open System
open System.Threading.Tasks

open Shared


[<Serializable>]
type MeasurementEvent =
    | NewMeasurement of Measurement
    | NewRun of Run

type IMeasurementState =
    inherit Orleans.IGrainWithGuidKey
    abstract GetMeasurement : unit -> Task<Measurement Option>
    abstract NewMeasurement : Measurement -> Task
    abstract NewRun: Run -> Task
    abstract Events : unit -> Task<MeasurementEvent list>

// a sample is identified by a guid
type ISample =
    inherit Orleans.IGrainWithGuidKey
    abstract GetSample : unit -> Task<Sample option>
    abstract SetSample : Sample -> Task

// events
[<Serializable>]
type Event =
    | NewSample of Sample
    | NewStep of Step
    | NewMeasurement of System.Guid*IMeasurementState

type ISampleState =
    inherit Orleans.IGrainWithGuidKey
    abstract GetSample : unit -> Task<Sample option>
    abstract NewSample : Sample -> Task
    abstract NewStep :  Step -> Task
    abstract NewMeasurement: System.Guid -> Task
    abstract GetMeasurements : unit -> Task< System.Collections.Generic.Dictionary<System.Guid,IMeasurementState>>
    abstract Events : unit -> Task<Event list>

// Register is just a a container of things
type Register<'a> () =
    member val set  = Collections.Generic.HashSet<'a> () with set, get

// one could think of a container as belonging to some user_id:int
type ISamples =
    inherit Orleans.IGrainWithIntegerKey
    abstract Register :  ISampleState-> Task<ISampleState>
    abstract All : unit -> Task<List<ISampleState>>

// register all measurements 
type IMeasurements = 
    inherit Orleans.IGrainWithIntegerKey
    abstract Register :  IMeasurementState-> Task
    abstract All : unit -> Task<List<IMeasurementState>>
