module Routes

open Fable.Import.Browser
open Elmish.Browser.UrlParser

[<RequireQualifiedAccess>]
type SampleRoute =
    | New
    | Sample of string

[<RequireQualifiedAccess>]
type Route =
    | Home
    | Sample of SampleRoute
    | Samples


let hashPrefix = sprintf "#%s"

[<Literal>]
let home = "home"

[<Literal>]
let sample = "sample"
[<Literal>]
let samples = "samples"

[<Literal>]
let newSample = "new"

let combine xs = List.fold (sprintf "%s/%s") "" xs

let toString route =
    match  route with
    | Route.Home -> home 
    | Route.Samples -> samples
    | Route.Sample subRoute ->
        match subRoute with 
            | SampleRoute.New -> combine [sample
                                          newSample] 
            | SampleRoute.Sample sampleGuid -> combine [sample
                                                        sampleGuid] 
   |> hashPrefix


let subRoute =
    oneOf [
        map SampleRoute.New (s "new")
        map (SampleRoute.Sample) (str)
    ]

let route : Parser<Route->Route,Route> =
    oneOf [
        map Route.Home (s "/")
        map Route.Home (s home)
        map Route.Samples (s samples)
        map Route.Sample (s sample </> subRoute)
    ]

let href =
    toString >> Fable.Helpers.React.Props.Href




// type alias to browsers navigation event 
[<Literal>]
let private NavigationEvent = "NavigationEvent"

// go to a an unsafe string url
let newUrl (newUrl : string) : Elmish.Cmd<_> =
    [ fun _ -> 
        history.pushState ((), "", newUrl)
        let ev = document.createEvent_CustomEvent()
        ev.initCustomEvent (NavigationEvent, true, true, obj())
        window.dispatchEvent ev |> ignore ]

// go to safer url
let navigate xs : Elmish.Cmd<_> =
    let nextUrl = hashPrefix (combine xs)
    [ fun _ -> 
        history.pushState ((), "", nextUrl)
        let ev = document.createEvent_CustomEvent()
        ev.initCustomEvent (NavigationEvent, true, true, obj())
        window.dispatchEvent ev |> ignore ]
