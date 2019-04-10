module Errors

open Fable.Import.React
open Fable.Helpers.React
open Fulma


// TODO: be helpeful to users and tell them what to do instead of an error:
// passs in the status code, case on in and 
// 404 -> nothing here change url
// 500 -> on snap, try again later, tenchical differnt
// etc etc
let view string msg dispatch =
              Message.message [ Message.Color IsDanger ]
                [ Message.header [ ]
                    [ str "Error processing you request"
                      Delete.delete [ Delete.Option.OnClick (fun _ -> dispatch msg) ]
                        [ ] ]
                  Message.body [ ]
                    [ str string ] ]

let wrap error content msg dispatch: ReactElement=
    match error with 
    | Some error' ->
        div[] [
            view error'  msg dispatch
            content
        ]
    | None ->
        content