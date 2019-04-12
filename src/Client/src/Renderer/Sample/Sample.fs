module Sample

open Elmish
open Fable.Helpers.React
open Fable.PowerPack.Fetch
open Thoth.Json

open Routes
open Shared
open URL
open Fulma
open Fulma

// open Style
type Model =
    { Sample : Sample
      Error : string option }

type Msg =
    | SampleName of string
    | PostSample
    | PostedSample of Result<Response,exn>
    | ClearError

// initialModel represents the starting model of this page
let time =
    (System.DateTime.Now |> System.DateTimeOffset)
        .ToUnixTimeSeconds()

let initialModel sample =
    { Sample = sample
      Error = None }

// place hodler for now, this is to be used if this page needs to load
// data before becoming the current page
let init(guid: System.Guid) (msg: Result<Sample, System.Exception> -> 'msg) :  Cmd<'msg> =
    // fetch some side effect (e.g. inital data load from a server)
    let sampleUlr = sprintf URL.URL.getSample guid
    let sampleFetch = fetchAs<Sample> (URL.apiURL sampleUlr) (Decode.Auto.generateDecoder())
    Cmd.ofPromise
        sampleFetch
        []
        (Ok >> msg)
        (Error >> msg)


// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match msg with
    | SampleName sampleName ->
        let newSample' = { model.Sample with Name = Some sampleName }
        { model with Sample = newSample' }, Cmd.none
    |  PostSample ->
        let post = postRecord (URL.apiURL URL.sample) model.Sample
        let cmd = 
                    Cmd.ofPromise
                         post
                         []
                         (Ok >> PostedSample)
                         (Error >> PostedSample)

        model,  cmd
    | PostedSample (Ok _ ) ->
        model, Cmd.none
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
                 Input.Placeholder placeHolder ]

let view (model : Model) (dispatch : Msg -> unit) =
    Errors.wrap model.Error 
               (Card.card [] 
                        [ Card.header []
                              [ Card.Header.title []
                                    [ str ""] ]

                          Card.content []
                              [ Level.level []
                                    [ Level.item []
                                          [ Level.title []
                                                        [ str
                                                          <| Option.defaultValue "Untitled sample"
                                                                 model.Sample.Name  ]
                                       ]

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
