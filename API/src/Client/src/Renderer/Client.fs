namespace Client
open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Routes


// MODEL
module app = 

    type CurrentPage =
      | Home 
      | NewSample of NewSample.Model


    type Msg =
        | SampleMsg of NewSample.Msg
        | SetRoute of Route option

    type Model = {
        currentPage : CurrentPage
    }


    // update model based on url update
    let setRoute (result: Option<Route>) model =
        match result with
        | None ->
            model, Navigation.modifyUrl "#" // redirecto home
        | Some (Route.Sample _) -> 
            { model with currentPage = NewSample NewSample.initialModel }, Cmd.none
        | Some Route.Home ->
            { model with currentPage = Home }, Cmd.none


    let init result =
        let (model, cmd) = setRoute result { currentPage = Home }
        model, cmd


    // VIEW

    let viewPage (page: CurrentPage) (dispatch: Msg->unit) = 
          match page with
          | Home  -> 
              str "Home page"
          | NewSample model ->
               NewSample.view model (SampleMsg >> dispatch)
                  


    let view (model: Model) (dispatch : Msg->unit) =
        div [] [
            Fulma.Columns.columns [Fulma.Columns.IsGap (Fulma.Screen.All, Fulma.Columns.Is2)] [
                Fulma.Column.column [] [
                    a [ href Route.Home] [ str "Home" ]
                ]
                Fulma.Column.column [] [
                    a [ href <| Route.Sample SampleRoute.New 
                      ] [ str "New" ]
                ]
            ]
            div [] [
              viewPage model.currentPage dispatch
            ]
        ]


    // UPDATE

    let update (msg: Msg) (model: Model) =
        match  msg with
        | SetRoute route ->
            setRoute route model
        | _ ->
            model, Cmd.none

    // PROGRAM
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
    |> Program.toNavigable (parseHash Routes.route) (fun route model ->
        model, 
        Cmd.ofMsg <| Msg.SetRoute route
    )
    |> Program.run