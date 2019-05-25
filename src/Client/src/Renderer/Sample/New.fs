[<RequireQualifiedAccessAttribute>]
module Sample.New

open Elmish
open Fable.React
open Routes
open Shared
open URL
open Fulma
open Thoth.Fetch
open Fetch.Types
open Elmish.Navigation

type Model =
    { Sample : Sample
      Error : string option }

type Msg =
    | SampleName of string
    | PostSample
    | PostedSample of Result<Response,exn>
    | ClearError

// initialModel represents the starting model of this page
let time ()=
    (System.DateTime.Now |> System.DateTimeOffset)
        .ToUnixTimeSeconds()

let initialModel() =
    { Sample =
          { Name = None
            GUID = System.Guid.NewGuid()
            Steps = None
            Time = time() }
      Error = None }

// place hodler for now, this is to be used if this page needs to load
// data before becoming the current page
let init() : Model * Cmd<Msg> =
    // fetch some side effect (e.g. inital data load from a server)
    let initalCmd = Cmd.none
    initialModel(), initalCmd


// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match msg with
    | SampleName sampleName ->
        let newSample' = { model.Sample with Name = Some sampleName }
        { model with Sample = newSample' }, Cmd.none
    |  PostSample ->
        // let post _ = Fetch.post((URL.apiURL URL.sample), model.Sample, isCamelCase = true)
        // let cmd = 
        //             Cmd.OfPromise.either
        //                  post
        //                  []
        //                  (Ok >> PostedSample)
        //                  (Error >> PostedSample)

        model,  Cmd.none
    | PostedSample (Ok _ ) ->
        //NOTE: this can be improved by using and external message and passing the state up
        //instead we re-route and thus roundtrip to the server to get the same data :/ 
        let newRoute = 
            model.Sample.GUID.ToString ()
            |> SampleRoute.Sample
            |> Route.Sample
            |> Routes.toString
        model, Navigation.newUrl newRoute
    | PostedSample (Error error) ->
        {model with Error = Some <| error.ToString () }, Cmd.none
    | ClearError ->
        { model with Error = None}, Cmd.none


let button color txt onClickmsg dispatch =
    Button.button [ Button.IsFullWidth
                    Button.Color color
                    Button.IsOutlined
                    Button.OnClick ( fun _ -> dispatch  onClickmsg) ] [ str txt ]

let input placeHolder msg dispatch =
    Input.text [ Input.OnChange(fun ev ->
                     // NOTE: for readers:
                     // we can use !!ev.target?value
                     // !!jsObj ->  unbox jsObj to type a
                     // obj?prop -> Dynamically access property rop from obj
                     // this means if things go wrong we get an undefined as a string here!
                     // or use the .Value which is a tiny more type safe!
                     ev.Value |> (msg >> dispatch))
                 Input.DefaultValue ""
                 Input.Placeholder placeHolder ]

let view (model : Model) (dispatch : Msg -> unit) =
    Errors.wrap model.Error 
               (Card.card [] 
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
                                                              <| model.Sample.toTime.LocalDateTime
                                                                                .ToShortDateString()
                                                                     ] ] ] ] ] ]

                          Card.footer []
                              [ Card.Footer.a [ ]
                                               [ button IsBlack "save" PostSample dispatch] 

                                Card.Footer.a [ GenericOption.Props [ href Route.Home ] ]
                                              [ button IsDanger "discard" PostSample dispatch ] 
                              ]
                    ])
                    ClearError dispatch
