module NewSample

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch
open Thoth.Json
open Shared

open Fulma


type Model = { Counter: Counter option }

type Msg =
| NoOp


// initialModel represents the starting model of this page
let initialModel = {Counter = None}

// place hodler for now, this is to be used if this page needs to load
// data before becoming the current page
let init () : Model * Cmd<Msg> =
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
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let view (model : Model) (dispatch : Msg -> unit) =
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "SAFE Template" ] ] ]

          Container.container []
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ Heading.h3 [] [ str (match model.Counter with
                                            | Some x -> "Press buttons to manipulate counter: " + sprintf "%i "x.Value
                                            | None -> "nothing" )]]

                Columns.columns []
                    [ Column.column [] [ button "-" (fun _ -> dispatch NoOp) ]
                      Column.column [] [ button "+" (fun _ -> dispatch NoOp) ] ] ]
        ]