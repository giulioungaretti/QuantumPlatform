module Entry

open Elmish
open Elmish.React
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Client.app
#if DEBUG
open Elmish.Debug
// turn on hot module replacement
open Elmish.HMR
#endif


Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif

|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif

|> Program.toNavigable (parseHash Routes.route)
       (fun route model -> model, Cmd.ofMsg <| Msg.SetRoute route)
|> Program.run