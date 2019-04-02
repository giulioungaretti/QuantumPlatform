namespace GiraffeServer
open Microsoft.Extensions.Logging

module HttpHandlers =

    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open GiraffeServer.Models
    open Interfaces

    let handleGetHello =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let log = ctx.GetLogger("handleGetHello")
            use client = ctx.GetService<Orleans.IClusterClient>()
            log.LogInformation("got clinet")
            task {
                let friend = client.GetGrain<IHello> 0L
                let! response = friend.SayHello ("Good morning, my friend!")
                log.Log(LogLevel.Information, "got response")
                return! json response next ctx
            } 