namespace GiraffeServer

open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open System.Threading.Tasks

open Interfaces
open Shared

module HttpHandlers =
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe

    let handleGetSample =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let log = ctx.GetLogger("handleGetHello")
            task {
                // use client = ctx.GetService<Orleans.IClusterClient>()
                log.LogInformation("got clinet")
                // let friend = client.GetGrain<Iello> 0L
                // let! rehandleGetHellosponse = friend.SayHello ("Good morning, my friend!")
                // asd
                log.Log(LogLevel.Information, "got response")
                return! json "as" next ctx
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
                    do! Task.Delay(1000)
                    return! ctx.WriteJsonAsync samples''
                }
        )
    let handlePostSample =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let log = ctx.GetLogger("handlePostSample")
            let client = ctx.GetService<Orleans.IClusterClient>()
            task {
                let! sample = ctx.BindModelAsync<Sample>()
                let sampleGrain =
                    client.GetGrain<ISampleState> <| sample.GUID 
                // save sample
                do! sampleGrain.NewSample(sample)
                log.LogDebug("Sample actor {%a}: on!", sample)
                // register sample
                let userid = 0L
                let samplesGrain = client.GetGrain<ISamples>userid
                let! _ = samplesGrain.Register(sampleGrain)
                log.LogDebug("Sample actor {%a}: registered!", sample)
                return! next ctx
            }
