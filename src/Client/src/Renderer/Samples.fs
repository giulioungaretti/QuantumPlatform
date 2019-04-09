namespace Samples

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop
open Fable.PowerPack.Fetch
open Thoth.Json
open Routes
open Shared
open URL.URL
open Fulma

// open Style
[<RequireQualifiedAccessAttribute>]
module Samples =
    type Model =
        { Samples : Samples }

    type Msg =
        | NoOp

    // defines the initial state and initial command (= side-effect) of the application
    let initModel samples =
        { Samples = samples }

    let init(msg: Result<Samples, System.Exception> -> 'msg) :  Cmd<'msg> =
        let samplesFetch = fetchAs<Samples>  (apiURL samples) (Decode.Auto.generateDecoder())
        Cmd.ofPromise
            samplesFetch
            []
            (Ok >> msg)
            (Error >> msg)

    // The update function computes the next state of the application based on the current state and the incoming events/messages
    // It can also run side-effects (encoded as commands) like calling the server via Http.
    // these commands in turn, can dispatch messages to which the update function will react.
    let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
        match model, msg with
        | _ -> model, Cmd.none


    let sampleRow (sample:Sample) =  
        tr [][
            td [][str <| Option.defaultValue "Untitled" sample.Name]
            td [][str <| sample.toTime.ToString ()]
        ]
    let viewSamples (samples: Samples)  =
        Table.table [ Table.IsBordered
                      Table.IsFullWidth
                      Table.IsStriped ]
            [ thead [ ]
                [ tr [ ]
                     [ th [ ] [ str "Sample name" ]
                       th [ ] [ str "Created on" ] ] ]
              tbody [ ] (List.map sampleRow samples)
            ]


    let view (model : Model) (dispatch : Msg -> unit) =
        Card.card []
            [ Card.header []
                  [ Card.Header.title []
                        [ str "samples"
                          ] ]

              Card.content [] [viewSamples model.Samples]

           ]