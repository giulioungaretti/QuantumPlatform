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
            let log = ctx.GetLogger("handleGetHello")
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
    let handlePostSample: HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let log = ctx.GetLogger("handlePostSample")
            task {
                let client = ctx.GetService<Orleans.IClusterClient>()
                let! sample = ctx.BindModelAsync<Sample>()
                let sampleGrain =
                    client.GetGrain<ISampleState> <| sample.GUID 
                // save sample
                try
                    do! sampleGrain.NewSample(sample)
                    log.LogDebug("Sample actor {%a}: on!", sample)
                    // register sample
                    let userid = 0L
                    let samplesGrain = client.GetGrain<ISamples>userid
                    let! _ = samplesGrain.Register(sampleGrain)
                    log.LogDebug("Sample actor {%a}: registered!", sample)
                    return! next ctx
                with
                    | Failure (Error.SampleExists) ->
                        ctx.SetStatusCode 400
                        return! ctx.WriteTextAsync Error.SampleExists
            }
