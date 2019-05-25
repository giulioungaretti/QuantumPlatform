namespace Samples

open Elmish
open Elmish.Navigation
open Fable.React
open Fable.React.Props
open Fetch
open Fetch.Types
open Thoth.Fetch


open Routes
open Shared
open URL.URL
open Fulma
open Thoth.Fetch

// open Style
[<RequireQualifiedAccessAttribute>]
module Samples =
    type Model =
        { Samples : Samples 
        }

    type Msg =
        | NoOp
        | SelectSample of Sample

    // defines the initial state and initial command (= side-effect) of the application
    let initModel samples =
        { Samples = samples 
        }

    let init(msg: Result<Samples, System.Exception> -> 'msg) :  Cmd<'msg> =
        let samplesFetch _ = Fetch.fetchAs<Samples> (apiURL samples) 
        Cmd.OfPromise.either
                samplesFetch
                []
                (Ok >> msg)
                (Error >> msg)

    // The update function computes the next state of the application based on the current state and the incoming events/messages
    // It can also run side-effects (encoded as commands) like calling the server via Http.
    // these commands in turn, can dispatch messages to which the update function will react.
    let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
        match msg with
        | SelectSample sample' ->
            let s =
                sample'.GUID.ToString ()
                |> SampleRoute.Sample
                |> Route.Sample
                |> Routes.toString

            model, Navigation.newUrl s
        | _ -> model, Cmd.none


    let sampleRow dispatch msg (sample:Sample) =  
        tr [ 
            OnClick (fun _ -> dispatch <| msg sample )
            href ( Route.Sample <| SampleRoute.Sample (sample.GUID.ToString ()))][
            td [][str <| Option.defaultValue "Untitled" sample.Name]
            td [][str <| sample.toTime.ToString ()]
        ]
    let viewSamples (samples: Samples) dispatch msg =
        Table.table [ Table.IsBordered
                      Table.IsFullWidth
                      Table.IsHoverable
                      Table.IsStriped ]
            [ thead [ ]
                [ tr [ ]
                     [ th [ ] [ str "Sample name" ]
                       th [ ] [ str "Created on" ] ] ]
              tbody [ ] (List.map ( sampleRow dispatch msg ) samples ) 
            ]


    let view (model : Model) (dispatch : Msg -> unit) =
        Card.card []
            [ Card.header []
                  [ Card.Header.title []
                        [ str "samples"
                          ] ]

              Card.content [] [viewSamples model.Samples dispatch SelectSample ]  

           ]