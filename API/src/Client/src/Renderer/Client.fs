namespace Client

open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fulma

open Sample
open Samples
open Shared

open Routes
open Routes
// MODEL
module app =

    type Page =
        | Home
        | NewSample of Sample.Model
        | Samples of Samples.Model

    type CurrentPage = 
        | TransitioningFrom of Page
        | Loaded of Page
        member this.Page = match this with
                           | TransitioningFrom p ->p
                           | Loaded p ->p

    type Msg =
        | SampleMsg of Sample.Msg
        | SamplesMsg of Samples.Msg
        | SetRoute of Route option
        | LoadedSamples of Result<Samples, exn>

    type Model =
        { currentPage : CurrentPage 
          error: string option
        }
        member __.loading = match __.currentPage with
                            | Loaded  _-> false 
                            | TransitioningFrom _ -> true

    // update model based on url update
    let setRoute (result : Option<Route>) model =
        match result with
        | None -> model, Navigation.modifyUrl "#" // redirecto home
        | Some Route.Home -> { model with currentPage = Loaded Home }, Cmd.none
        | Some(Route.Sample _) ->
            { model with currentPage = Loaded <| NewSample Sample.initialModel }, Cmd.none
        | Some Route.Samples -> 
            let cmd = Samples.init LoadedSamples 
            { model with currentPage = TransitioningFrom model.currentPage.Page }, cmd

    let init result =
        let (model, cmd) = setRoute result { currentPage =Loaded Home
                                             error=None }
        model, cmd

    // VIEW
    let viewPage (page : Page) (dispatch : Msg -> unit) =
        match page with
        | Home -> str "Home page"
        | Samples model-> Samples.view model (SamplesMsg >> dispatch)
        | NewSample model -> Sample.view model (SampleMsg >> dispatch)

    let view (model : Model) (dispatch : Msg -> unit) =
        let navbar = 
                Navbar.navbar []
                  [ Navbar.Item.a [ Navbar.Item.Props [ href Route.Home ] ]
                        [ str "Home" ]

                    Navbar.Item.a
                        [ Navbar.Item.Props
                              [ href <| Route.Sample SampleRoute.New ] ]
                        [ str "New" ]

                    Navbar.Item.a
                        [ Navbar.Item.Props [ href Route.Samples ] ]
                        [ str "Samples" ] 
                   ]
        let page = 
              match model.loading with 
              |true ->
                Section.section [] [ str "loading" ] 
                // PageLoader.pageLoader [ PageLoader.Color IsSuccess
                //                             PageLoader.IsActive true ]
                        // [ ]
              |false ->
                Section.section [] [ viewPage model.currentPage.Page dispatch ] 

        Container.container []
            [ navbar
              page
              ]

    // UPDATE
    let update (msg : Msg) (model : Model) =
        match (msg, model.currentPage.Page) with
        | (SetRoute route, _) -> 
            setRoute route model
        | (SampleMsg msg', NewSample model') ->
            let model', cmd = Sample.update msg' model'
            { model with currentPage = Loaded <| NewSample model' }, Cmd.map SampleMsg cmd
        | (SamplesMsg msg', Samples model') ->
            let model', cmd = Samples.update msg' model'
            { model with currentPage = Loaded <| Samples model' }, Cmd.map SamplesMsg cmd
        | (LoadedSamples result, _) ->
            match result with
            | Ok samples' ->
                let model' = Samples.initModel samples'
                {model with currentPage = Loaded  <| Samples model'}, Cmd.none
            | Error error ->
                {model with error = Some <| error.ToString () }, Cmd.none
        // wrong message to wrong page
        | (_, _) -> 
            Fable.Import.Browser.console.log "Got message for wrong page!" 
            model, Cmd.none
