namespace Client

open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Routes
open Sample
open Fulma

// MODEL
module app =
    type CurrentPage =
        | Home
        | NewSample of Sample.Model

    type Msg =
        | SampleMsg of Sample.Msg
        | SetRoute of Route option

    type Model =
        { currentPage : CurrentPage }

    // update model based on url update
    let setRoute (result : Option<Route>) model =
        match result with
        | None -> model, Navigation.modifyUrl "#" // redirecto home
        | Some(Route.Sample _) ->
            { model with currentPage = NewSample Sample.initialModel }, Cmd.none
        | Some Route.Home -> { model with currentPage = Home }, Cmd.none

    let init result =
        let (model, cmd) = setRoute result { currentPage = Home }
        model, cmd

    // VIEW
    let viewPage (page : CurrentPage) (dispatch : Msg -> unit) =
        match page with
        | Home -> str "Home page"
        | NewSample model -> Sample.view model (SampleMsg >> dispatch)

    let view (model : Model) (dispatch : Msg -> unit) =
        Container.container []
            [ Navbar.navbar []
                  [ Navbar.Item.a [ Navbar.Item.Props [ href Route.Home ] ]
                        [ str "Home" ]

                    Navbar.Item.a
                        [ Navbar.Item.Props
                              [ href <| Route.Sample SampleRoute.New ] ]
                        [ str "New" ] ]
              Section.section [] [ viewPage model.currentPage dispatch ] ]

    // UPDATE
    let update (msg : Msg) (model : Model) =
        match (msg, model.currentPage) with
        | (SetRoute route, _) -> setRoute route model
        | (SampleMsg msg', NewSample model') ->
            let model', cmd = Sample.update msg' model'
            { model with currentPage = NewSample model' }, Cmd.map SampleMsg cmd
        // wrong message to wrong page
        | (_, _) -> model, Cmd.none
