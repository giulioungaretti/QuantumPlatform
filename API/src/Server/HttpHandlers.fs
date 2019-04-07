namespace GiraffeServer

open Microsoft.Extensions.Logging
open System.Threading.Tasks
open Shared
open Interfaces

module HttpHandlers =
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open Shared

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
                return! next ctx
            }
