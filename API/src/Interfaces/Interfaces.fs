module Interfaces

open System
open System.Threading.Tasks
open Shared

type SampleHolder(sample : Sample option) =
    new() = SampleHolder None
    member val Sample : Sample option = None with get, set

type ISample =
    inherit Orleans.IGrainWithGuidKey
    abstract GetSample : unit -> Task<Sample option>
    abstract SetSample : Sample -> Task
