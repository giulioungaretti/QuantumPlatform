open System.IO
#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open System

open Fake.Core
open Fake.DotNet
open Fake.IO

let siloPath = Path.getFullName "./Silo.Host"
let serverPath = Path.getFullName "./Server"
let clientPath = Path.getFullName "./Client"
let clientPathMain = Path.combine clientPath "src/Main"
let clientPathWeb = Path.combine clientPath "src/Renderer"
let clientDeployPathWeb = Path.combine clientPathWeb "deploy"
let clientDeployPathElectron = Path.combine clientPath "dist"
let fableCache p = Path.combine p "/.fable" 
let nodeModules p = Path.combine p "/node_modules"
let deployDir = Path.getFullName "./deploy"

let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | _ ->
        let errorMsg =
            tool + " was not found in path. " +
            "Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
        failwith errorMsg

let nodeTool = platformTool "node" "node.exe"
let yarnTool = platformTool "yarn" "yarn"

let runTool cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs
    Command.RawCommand (cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let runDotNet cmd workingDir =
    let result =
        DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

let openBrowser url =
    //https://github.com/dotnet/corefx/issues/10361
    Command.ShellCommand url
    |> CreateProcess.fromCommand
    |> CreateProcess.ensureExitCodeWithMessage "opening browser failed"
    |> Proc.run
    |> ignore

// remove all the caches because sometimes
// fable gets stuck (? node )
Target.create "HardClean" (fun _ ->
    [ deployDir
      clientDeployPathWeb
      clientDeployPathElectron
      fableCache clientPath
      fableCache clientPathWeb
      nodeModules clientPath
      nodeModules clientPathWeb
      ]
    |> Shell.cleanDirs
)


Target.create "Clean" (fun _ ->
    [ deployDir
      clientDeployPathWeb
    //   clientDeployPathElectron
    ]
    |> Shell.cleanDirs
)

Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    runTool nodeTool "--version" clientPath 
    printfn "Yarn version:"
    runTool yarnTool "--version" clientPath 
    runTool yarnTool "install --frozen-lockfile" clientPathWeb
    runDotNet "restore" clientPathMain
    runDotNet "restore" clientPathWeb
)

Target.create "InstallClientElectron" (fun _ ->
    printfn "Node version:"
    runTool nodeTool "--version" clientPath 
    printfn "Yarn version:"
    runTool yarnTool "--version" clientPath 
    runTool yarnTool "install --frozen-lockfile" clientPath 
    runDotNet "restore" clientPathMain
    runDotNet "restore" clientPathWeb
)

Target.create "BuildWeb" (fun _ ->
    runDotNet "build" serverPath
    runTool yarnTool "webpack-cli -p" clientPathWeb
)

Target.create "BuildElectron" (fun _ ->
    runDotNet "build" serverPath
    runTool yarnTool "dist" clientPathWeb
)

Target.create "Server" ( fun _ ->
    async {
        return runDotNet "watch run" serverPath
    }
    |> Async.RunSynchronously
    |> ignore
)

Target.create "Silo" (fun _ ->
    async {
        return runDotNet "watch run" siloPath
    }
    |> Async.RunSynchronously
    |> ignore
)

Target.create "ClientWeb" (fun _ ->
    let client = async {
        return runTool yarnTool "webpack-dev-server" clientPathWeb
    }
    let browser = async {
        do! Async.Sleep 5000
        openBrowser "http://localhost:8080"
    }
    let tasks = [client 
                 browser]
    tasks 
    |> Async.Parallel 
    |> Async.RunSynchronously
    |> ignore
 )

Target.create "ClientElectron" (fun _ ->
    let client = async {
        return runTool yarnTool "dev" clientPath
    }
    client 
    |> Async.RunSynchronously
    |> ignore
 )

Target.create "Run" (fun _ ->
    let server = async {
        runDotNet "watch run" serverPath
    }
    let client = async {
        runTool yarnTool "webpack-dev-server" clientPathWeb
    }
    let browser = async {
        do! Async.Sleep 5000
        openBrowser "http://localhost:8080"
    }

    let vsCodeSession = Environment.hasEnvironVar "vsCodeSession"
    let safeClientOnly = Environment.hasEnvironVar "safeClientOnly"

    let tasks =
        [
          if not safeClientOnly then yield server
          yield client
          if not vsCodeSession then yield browser ]

    tasks
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "RunElectron" (fun _ ->
    let server = async {
        runDotNet "watch run" serverPath
    }
    let client = async {
        runTool yarnTool "dev" clientPath
    }
    let browser = async {
        do! Async.Sleep 5000
        openBrowser "http://localhost:8080"
    }

    let vsCodeSession = Environment.hasEnvironVar "vsCodeSession"
    let safeClientOnly = Environment.hasEnvironVar "safeClientOnly"

    let tasks =
        [ if not safeClientOnly then yield server
          yield client
          if not vsCodeSession then yield browser ]

    tasks
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)





open Fake.Core.TargetOperators

"Clean"
    ==> "InstallClient"
    ==> "BuildWeb"


"Clean"
    ==> "HardClean"
    ==> "InstallClientElectron"
    ==> "BuildElectron"

"Clean"
    ==> "InstallClient"
    ==> "Run"

"Clean"
    ==> "InstallClient"
    ==> "RunElectron"

Target.runOrDefaultWithArguments "Build"