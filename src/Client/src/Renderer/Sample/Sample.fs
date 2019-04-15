module Sample

open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.PowerPack.Fetch
open Thoth.Json

open Routes
open Shared
open URL
open Fulma
open Fable.FontAwesome


type PageState =
    | Viewing 
    | AddingStep of Step

type Model =
    { Sample : Sample
      PageState: PageState
      Error : string option }

type Msg =
    | NoOp
    | SampleName of string
    | PostStep  of Step
    | Discard
    | PostedStep of Result<Response,exn>
    | ClearError
    | AddStep
    | UpdateStep of Step

let time () =
    (System.DateTime.Now |> System.DateTimeOffset)
        .ToUnixTimeSeconds()

let initialModel sample =
    { Sample = sample
      PageState = Viewing
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
    |   PostStep step ->
        let stepURL = sprintf URL.postStep model.Sample.GUID
        let post = postRecord (URL.apiURL stepURL ) step
        let cmd = 
                    Cmd.ofPromise
                         post
                         []
                         (Ok >> PostedStep)
                         (Error >> PostedStep)

        model,  cmd
    | PostedStep (Ok _ ) ->
        match model.PageState with
        |Viewing ->
            model, Cmd.none
        |AddingStep step  ->
            let newRoute = 
                model.Sample.GUID.ToString ()
                |> SampleRoute.Sample
                |> Route.Sample
                |> Routes.toString
            {model with 
                PageState = Viewing
                Sample = model.Sample.AddStep step},
             Navigation.newUrl newRoute
    | PostedStep (Error error) ->
        {model with Error = Some <| error.ToString () }, Cmd.none
    | AddStep ->
        {model with PageState = AddingStep <| {Name = None
                                               GUID = System.Guid.NewGuid () 
                                               Time= time() }

         }, Cmd.none 
    | UpdateStep step -> 
        {model with PageState = AddingStep step }
         , Cmd.none 
    | ClearError ->
        { model with Error = None}, Cmd.none
    | Discard ->
        let newRoute = 
            model.Sample.GUID.ToString ()
            |> SampleRoute.Sample
            |> Route.Sample
            |> Routes.toString
        {model with PageState = Viewing},  
         Navigation.newUrl newRoute
    | NoOp ->
        model, Cmd.none


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

let viewHeader placeHolder maybeTitle (time:System.DateTimeOffset) =
     [Level.right []
          [ Level.title []
                        [ str
                          <| Option.defaultValue placeHolder 
                                 maybeTitle ]
            ]

      Level.right []
          [ Level.item [] []

            Level.item []
                [ div []
                      [ Level.heading []
                            [ str "created on:" ]

                        div []
                            [ str
                              <| time.LocalDateTime
                                                .ToShortDateString()
                                     ] ] ] ] 
        ]


let viewFooter dispatch =
        Card.Footer.div[][
            Button.button [ Button.OnClick (fun _ -> dispatch AddStep ) ]
                       [ Icon.icon [ ]
                            [ Fa.i [ Fa.Solid.Plus ]
                               [ ] ] ]
            Level.heading [] [str "Add step"]
           ]

let viewContent page (sample: Sample) (dispatch)=
              [ Level.level []
                    [ Level.item []
                          [ Level.title []
                                        [ str
                                          <| Option.defaultValue "Untitled sample"
                                                 sample.Name  ]
                       ]

                      Level.right []
                          [ Level.item [] []

                            Level.item []
                                [ div []
                                      [ Level.heading []
                                            [ str "created on:" ]

                                        div []
                                            [ str
                                              <| sample.toTime.LocalDateTime
                                                                .ToShortDateString()
                                                     ] ] ] ] ] 
                Section.section [] <|
                                [Card.card [] [
                                    Card.header []
                                                  [ Card.Header.title []
                                                        [ str "steps"] ]
                                    Card.content[]  <|
                                                    match page with
                                                    | Viewing  ->
                                                        List.map (fun (s:Step) -> Level.level [] <| 
                                                                                    viewHeader "Untitled step" s.Name s.toTime) <|
                                                                                                    Option.defaultValue [] sample.Steps
                                                    | AddingStep step  ->
                                                        let cont =
                                                            List.map (fun (s:Step) -> Level.level [] <| 
                                                                                        viewHeader "Untitled step" s.Name s.toTime) <|
                                                                                                    Option.defaultValue [] sample.Steps
                                                        let input = 
                                                         Level.item [][ 
                                                              div [ Props.ClassName "fullWidth" ] [
                                                              Level.heading [] [str "Step name"]
                                                              input "Step name"  (fun s->  UpdateStep {step with Name = Some s}) dispatch]
                                                              ]
                                                        List.append  cont [input]
                                              
                                    Card.footer [] [
                                        match page with
                                        | Viewing  ->
                                            yield viewFooter dispatch
                                        | AddingStep step ->
                                          yield Card.Footer.a [ ]
                                                                   [ button IsBlack "Add"  (PostStep step) dispatch] 

                                          yield Card.Footer.a [ ]
                                                          [ button IsDanger "discard"  Discard dispatch ] 
                                        ]
                                  ]
                                 ]
                ]


let view (model : Model) (dispatch : Msg -> unit) =
    Errors.wrap model.Error 
               (Card.card [] 
                        [ 

                          Card.content [] <| viewContent model.PageState model.Sample dispatch

                    ])
                    ClearError dispatch

