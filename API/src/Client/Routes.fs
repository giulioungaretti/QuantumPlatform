module Routes

open Elmish.Browser.UrlParser

[<RequireQualifiedAccess>]
type SampleRoute =
    | New

[<RequireQualifiedAccess>]
type Route =
    | Home
    | Sample of SampleRoute


let toString route =
    match  route with
    | Route.Home -> "#/home"
    | Route.Sample subRoute ->
        match subRoute with 
            | SampleRoute.New -> "#/sample/new"

let subRoute =
    oneOf [
        map SampleRoute.New (s "new")
    ]

let route : Parser<Route->Route,Route> =
    oneOf [
        map Route.Home (s "/")
        map Route.Home (s "home")
        map Route.Sample (s "sample" </> subRoute)
    ]


let href =
    toString >> Fable.Helpers.React.Props.Href