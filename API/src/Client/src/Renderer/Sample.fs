namespace Sample

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch
open Thoth.Json
open Shared
open Fulma

// open Style
[<RequireQualifiedAccessAttribute>]
module Sample =
    type Model =
        { Sample : Sample }

    type Msg = NoOp

    // initialModel represents the starting model of this page
    let time = System.DateTime.Now

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
        | _ -> model, Cmd.none

    let button txt onClick =
        Button.button [ Button.IsFullWidth
                        Button.Color IsPrimary
                        Button.OnClick onClick ] [ str txt ]

    let input =
        Container.container []
            [ Field.div []
                  [ Control.p [ Control.IsExpanded ]
                        [ Input.text [ Input.Option.CustomClass
                                           Input.Classes.Size.IsFullwidth
                                       Input.Placeholder "Type sample name" ] ] ] ]

    let view (model : Model) (dispatch : Msg -> unit) =
        Card.card [] [ Card.header []
                           [ Card.Header.title []
                                 [ str
                                   <| Option.defaultValue "Untitled sample"
                                          model.Sample.Name ] ]

                       Card.content []
                           [ Level.level []
                                 [ input

                                   Level.right []
                                       [ Level.item [] []

                                         Level.item []
                                             [ str
                                               <| model.Sample.Time.ToShortDateString
                                                      () ]

                                         Level.item []
                                             [ str
                                               <| model.Sample.Time.ToShortTimeString
                                                      () ] ] ] ]
                       Card.footer [] [ Card.Footer.a [] [ str "Save" ]
                                        Card.Footer.a [] [ str "Delete" ] ] ]
// [ Level.level []
//       [ Level.left [] [ input ]
//         Level.right []
//   Level.level [] [] ]
