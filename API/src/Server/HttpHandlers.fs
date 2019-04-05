namespace GiraffeServer
open Microsoft.Extensions.Logging
open System.Threading.Tasks
open Shared

module HttpHandlers =

    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open GiraffeServer.Models
    open Interfaces

    let handleGetHello =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let log = ctx.GetLogger("handleGetHello")
            task {
                use client = ctx.GetService<Orleans.IClusterClient>()
                log.LogInformation("got clinet")
                let friend = client.GetGrain<IHello> 0L
                let! response = friend.SayHello ("Good morning, my friend!")
                log.Log(LogLevel.Information, "got response")
                return! json response next ctx
            } 

    let getInitCounter () : Task<Counter> = task { return { Value = 42 } }

    let handleGetHey =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let log = ctx.GetLogger("handlegGetHey")
            task {
                let! counter = getInitCounter()
                log.Log(LogLevel.Information, "got response")
                return! json counter next ctx
            } 