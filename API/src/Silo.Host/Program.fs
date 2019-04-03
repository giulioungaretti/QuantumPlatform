open System
open System.Net
open System.Threading.Tasks
open Grains
open Interfaces
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Orleans
open Orleans.Configuration
open Orleans.Hosting



[<CLIMutable>]
type Config =
    {
        OrleansTableURL : string
    }

let buildSiloHost (context: Microsoft.Extensions.Hosting.HostBuilderContext) (siloBuilder:ISiloBuilder) =
       let conf  = context.Configuration
       let url = conf.GetSection("OrleansTableUrl").Get<Config>().OrleansTableURL
       siloBuilder
         .Configure<ClusterOptions>(fun (options:ClusterOptions) -> 
               options.ClusterId <- "orleans-dev-docker"; 
               options.ServiceId <- "FsharpOrleansDev")
         .ConfigureEndpoints(siloPort=11111, gatewayPort=30000)
         .UseAzureStorageClustering(fun (options:AzureStorageClusteringOptions)-> 
                options.ConnectionString <- url )
         .ConfigureApplicationParts(fun parts ->
             parts.AddApplicationPart(typeof<HelloGrain>.Assembly)
                   .AddApplicationPart(typeof<IHello>.Assembly)
                   .WithCodeGeneration() |> ignore)
      |> ignore

let configureAppConfiguration (context: Microsoft.Extensions.Hosting.HostBuilderContext) (config: IConfigurationBuilder) =  
    config
       .AddJsonFile("appsettings.json",false,true)
       .AddJsonFile(sprintf "appsettings.%s.json" context.HostingEnvironment.EnvironmentName ,true, true)
       .AddEnvironmentVariables() |> ignore
    let env = context.HostingEnvironment
    (match env.IsDevelopment() with
    | true -> 
        config.AddUserSecrets<Config>() |> ignore
    | false -> 
        ()
    )

[<EntryPoint>]
let main _ =
      HostBuilder()
       .UseOrleans(buildSiloHost)
       .ConfigureAppConfiguration(configureAppConfiguration)
       .ConfigureLogging(fun logging -> logging.AddConsole() |> ignore)
       .Build()
       .RunAsync() |> ignore
      0