[<RequireQualifiedAccessAttribute>]
module Style

open Fulma

let center =
    Content.Modifiers
        [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ]
