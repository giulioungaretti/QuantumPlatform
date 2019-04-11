namespace URL

module URL=

    [<Literal>]
    let api = "/api"


    let getSample = PrintfFormat<(System.Guid -> string),unit,string,string,System.Guid>"/sample/%O" 
    [<Literal>]
    let sample = "/sample"
    [<Literal>]
    let samples = "/samples"

    let apiURL s:string = sprintf "/api%s" s
