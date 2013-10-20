module Wazaa.Console.Main

open System
open System.Configuration
open System.Threading
open Wazaa

[<EntryPoint>]
let main args = 
    let localEndPoint = Wazaa.Config.ReadServerConfiguration()

    let server = Server.HttpServer(localEndPoint)
    printfn "Press [CTRL+C] to shut down the server!"

    Console.CancelKeyPress.AddHandler (fun o e ->
        server.Stop()
        printfn "Server shut down."
    )

    server |> Server.RunServerAsync |> Async.RunSynchronously
    server.Stop()

    0
