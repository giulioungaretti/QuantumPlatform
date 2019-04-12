namespace GiraffeServer

open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open System.Threading.Tasks

open Interfaces
open Shared

module HttpHandlers =
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe

    let handleGetSample (guid)=
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let log = ctx.GetLogger("handleGetSample")
            task {
                let client = ctx.GetService<Orleans.IClusterClient>()
                log.LogInformation("got clinet")
                // NOTE: create the grain anyway even tho it could be waste of space
                // the post with the same guid will reuse this grain
                let samplet = (client.GetGrain<ISampleState> guid)
                let! sample = samplet.GetSample()
                // then just 404 if the grain did not contain an existing sample!
                match sample with
                | Some sample' ->
                    return! json sample' next ctx
                | None ->
                    ctx.SetStatusCode 404
                    return! ctx.WriteTextAsync "Could not find sample"
            }

    let handleGetSampleMeasurements sampleGuid =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let log = ctx.GetLogger("handleGetSampleMeasurements")
            task {
                let client = ctx.GetService<Orleans.IClusterClient>()
                log.LogInformation("got clinet")
                // NOTE: create the grain anyway even tho it could be waste of space
                // the post with the same guid will reuse this grain
                let samplet = (client.GetGrain<ISampleState> sampleGuid)
                let! sample = samplet.GetSample()
                log.LogDebug ("got sample {%a}", sample)
                // then just 404 if the grain did not contain an existing sample!
                match sample with
                | Some _ ->
                    let! measurements = samplet.GetMeasurements()
                    return! json measurements next ctx
                | None ->
                    ctx.SetStatusCode 404
                    return! ctx.WriteTextAsync "Could not find sample"
            }

    let handleGetSamples: HttpFunc -> HttpContext -> Task<HttpContext option>  =
        handleContext (
            fun (ctx:HttpContext) ->
                let log = ctx.GetLogger("handleGetSamples")
                task {
                    let client = ctx.GetService<Orleans.IClusterClient>()
                    let userid = 0L
                    let samplesGrain = client.GetGrain<ISamples>userid
                    let! samples = samplesGrain.All()
                    let! samples' = Task.WhenAll  (List.map 
                                                        (fun (s : ISampleState) 
                                                                -> s.GetSample()) 
                                                        samples) 
                    let samples'': Samples = Seq.choose id samples'
                                              |> List.ofSeq
                    log.LogDebug("%{a}got samples", samples'')
                    return! ctx.WriteJsonAsync samples''
                }
        )

    let handleGetMeasurements: HttpFunc -> HttpContext -> Task<HttpContext option>  =
        handleContext (
            fun (ctx:HttpContext) ->
                let log = ctx.GetLogger("handleGetMeasurements")
                task {
                    let client = ctx.GetService<Orleans.IClusterClient>()
                    let userid = 0L
                    let measurementsGrain = client.GetGrain<IMeasurements>userid
                    let! measurements = measurementsGrain.All()
                    let! measurements' = Task.WhenAll  (List.map 
                                                             (fun (m : IMeasurementState) 
                                                                    -> m.GetMeasurement()) 
                                                              measurements) 
                    let measurements'': Measurements = Seq.choose id measurements'
                                                           |> List.ofSeq
                    log.LogDebug("%{a}got Measurements", measurements'')
                    return! ctx.WriteJsonAsync measurements''
                }
        )

    let handlePostSample: HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let log = ctx.GetLogger("handlePostSample")
            task {
                try
                    let client = ctx.GetService<Orleans.IClusterClient>()
                    let! sample = ctx.BindModelAsync<Sample>()
                    let sampleGrain =
                        client.GetGrain<ISampleState> <| sample.GUID 
                    // save sample
                    do! sampleGrain.NewSample(sample)
                    log.LogDebug("Sample grain {%a}: on!", sample)

                    // NOTE: this is a bit tricky and could use a transaction
                    // if the registration below fails, we have miss the sample in the 
                    // samples registry.

                    // register sample
                    let userid = 0L
                    let samplesGrain = client.GetGrain<ISamples>userid
                    let! _ = samplesGrain.Register(sampleGrain)
                    log.LogDebug("Sample grain {%a}: registered!", sample)
                    return! next ctx
                with
                    | Failure (Error.SampleExists) ->
                        ctx.SetStatusCode 400
                        return! ctx.WriteTextAsync Error.SampleExists
                    | exn ->
                        let msg = exn.ToString()
                        ctx.SetStatusCode 500
                        log.LogError("Something went wrong wiht grain", msg)
                        return! ctx.WriteTextAsync msg

            }

    let handlePostMeasurement: HttpHandler =
        fun(next: HttpFunc) (ctx: HttpContext) ->
            let log = ctx.GetLogger("handlePostMeasurement")
            task{
                let client = ctx.GetService<Orleans.IClusterClient> ()
                let! measurement = ctx.BindModelAsync<Measurement> ()
                let measurementGrain = client.GetGrain<IMeasurementState>  measurement.GUID
                // save measurement
                do! measurementGrain.NewMeasurement measurement
                log.LogDebug("Sample grain %a: on!", measurement)
                // register measurement
                let userid = 0L
                let measurementsGrain = client.GetGrain<IMeasurements> userid
                do!  measurementsGrain.Register measurementGrain
                log.LogDebug("Measurement grain %a: registered", measurementGrain)
                return! next ctx
            }