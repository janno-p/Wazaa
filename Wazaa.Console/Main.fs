module Wazaa.Console.Main

open System
open System.Configuration
open System.Threading
open Wazaa.Server

[<EntryPoint>]
let main args = 
    let host = ConfigurationManager.AppSettings.["host"]

    let mutable port = 0
    if not (Int32.TryParse(ConfigurationManager.AppSettings.["port"], &port)) then
        port <- 0

    let server = HttpServer(host, port)
    printfn "Press [CTRL+C] to shut down the server!"

    Console.CancelKeyPress.AddHandler (fun o e ->
        server.Stop()
        printfn "Server shut down."
    )

    server |> RunServerAsync |> Async.RunSynchronously
    server.Stop()

    0
