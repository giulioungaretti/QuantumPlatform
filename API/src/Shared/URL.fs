namespace URL

module URL=

    [<Literal>]
    let api = "/api"

    let sample = "/sample"
    let samples = "/samples"

    let apiURL s:string = sprintf "/api/%s" s