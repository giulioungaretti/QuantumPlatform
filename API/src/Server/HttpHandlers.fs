namespace GiraffeServer

open Microsoft.Extensions.Logging
open System.Threading.Tasks
open Shared

open Interfaces
open Grains
open Microsoft.AspNetCore.Http

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
                let log = ctx.GetLogger("handleGetSample started")
                task {
                    let client = ctx.GetService<Orleans.IClusterClient>()
                    let userid = 0L
                    log.LogError("got client ")
                    let samplesGrain = client.GetGrain<ISamples>userid
                    log.LogError("got grain")
                    let! samples = samplesGrain.All()
                    log.LogError("got samples")
                    // NOTE 
                    return! ctx.WriteJsonAsync samples
                }
        )
    let handlePostSample =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let log = ctx.GetLogger("handleGetHello")
            let client = ctx.GetService<Orleans.IClusterClient>()
            task {
                let! sample = ctx.BindModelAsync<Sample>()
                log.LogError("{%a}", sample)
                let sampleGrain =
                    client.GetGrain<ISample> <| System.Guid.NewGuid()
                do! sampleGrain.SetSample(sample)
                // let userid = 0L
                // let samplesGrain = client.GetGrain<ISamples>userid
                // do! samplesGrain.Register(sampleGrain)
                return! next ctx
            }
