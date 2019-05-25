module Routes

open Elmish
open Elmish.UrlParser
open Browser

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
    toString >> Fable.React.Props.Href


// type alias to browsers navigation event 
[<Literal>]
let private NavigationEvent = "NavigationEvent"

// go to a an unsafe string url
let newUrl (newUrl : string) : Elmish.Cmd<_> =
    Navigation.Navigation.newUrl newUrl

// go to safer url
let navigate xs : Elmish.Cmd<_> =
    let nextUrl = hashPrefix (combine xs)
    Navigation.Navigation.newUrl nextUrl