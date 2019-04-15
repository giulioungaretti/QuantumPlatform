namespace URL

module URL=

    [<Literal>]
    let api = "/api"


    let getSample = PrintfFormat<(System.Guid -> string),unit,string,string,System.Guid>"/sample/%O" 
    let getMeasurement = PrintfFormat<(System.Guid -> string),unit,string,string,System.Guid>"/measurement/%O" 
    let postStep = PrintfFormat<(System.Guid -> string),unit,string,string,System.Guid>"/step/%O" 

    [<Literal>]
    let sample = "/sample"
    [<Literal>]
    let samples = "/samples"
    [<Literal>]
    let measurement = "/measurement"
    [<Literal>]
    let measurements = "/measurements"

    let apiURL s:string = sprintf "/api%s" s
