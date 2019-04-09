namespace Sample

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop
open Fable.PowerPack.Fetch
open Thoth.Json
open Routes
open Shared
open Fulma

// open Style
[<RequireQualifiedAccessAttribute>]
module Sample =
    type Model =
        { Sample : Sample }

    type Msg =
        | NoOp
        | SampleName of string

    // initialModel represents the starting model of this page
    let time =
        (System.DateTime.Now |> System.DateTimeOffset)
            .ToUnixTimeSeconds()

    let initialModel =
        { Sample =
              { Name = None
                Time = time } }

    // place hodler for now, this is to be used if this page needs to load
    // data before becoming the current page
    let init() : Model * Cmd<Msg> =
        // fetch some side effect (e.g. inital data load from a server)
        let initalCmd = Cmd.none
        initialModel, initalCmd

    // The update function computes the next state of the application based on the current state and the incoming events/messages
    // It can also run side-effects (encoded as commands) like calling the server via Http.
    // these commands in turn, can dispatch messages to which the update function will react.
    let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
        match model, msg with
        | (model, SampleName sampleName) ->
            let newSample' = { model.Sample with Name = Some sampleName }
            { model with Sample = newSample' }, Cmd.none
        | _ -> model, Cmd.none

    let button txt onClick =
        Button.button [ Button.IsFullWidth
                        Button.Color IsPrimary
                        Button.OnClick onClick ] [ str txt ]

    let input placeHolder msg dispatch =
        Input.text [ Input.OnChange(fun ev ->
                         // NOTE: for readers:
                         // we can use !!ev.target?value
                         // !!jsObj ->  unbox jsObj to type a
                         // obj?prop -> Dynamically access property rop from obj
                         // this means if things go wrong we get an undefined as a string here!
                         // or use the .Value which is a tiny more type safe!
                         ev.Value |> (msg >> dispatch))
                     Input.Placeholder placeHolder ]

    let view (model : Model) (dispatch : Msg -> unit) =
        Card.card []
            [ Card.header []
                  [ Card.Header.title []
                        [ str
                          <| Option.defaultValue "Untitled sample"
                                 model.Sample.Name ] ]

              Card.content []
                  [ Level.level []
                        [ Level.item []
                              [ input "sample name" SampleName dispatch ]

                          Level.right []
                              [ Level.item [] []

                                Level.item []
                                    [ div []
                                          [ Level.heading []
                                                [ str "created on:" ]

                                            div []
                                                [ str
                                                  <| model.Sample.toTime.ToString
                                                         ()

                                                  //   .ToShortDateString()
                                                  str
                                                  <| model.Sample.toTime.ToString
                                                         () ] ] ] ] ] ]

              Card.footer []
                  [ Card.Footer.a [] [ str "Save" ]

                    Card.Footer.a [ GenericOption.Props [ href Route.Home ] ]
                        [ str "Discard" ] ] ]
