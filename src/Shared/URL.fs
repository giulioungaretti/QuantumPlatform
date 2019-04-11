namespace URL

module URL=

    [<Literal>]
    let api = "/api"

    [<Literal>]
    let sample = "/sample"
    [<Literal>]
    let samples = "/samples"

    let apiURL s:string = sprintf "/api%s" s