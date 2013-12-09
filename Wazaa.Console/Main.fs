module Wazaa.Console.Main

open System
open System.Configuration
open System.Threading
open Wazaa
open Wazaa.Config
open Wazaa.Server

[<EntryPoint>]
let main args =
    let listener = { new IMessageListener with member this.FilesFound arg = () }

    let computation = StartServer LocalEndPoint listener
    printfn "Press [CTRL+C] to shut down the server!"

    let isCancelled = ref false
    let token = ref (new CancellationTokenSource())

    Console.CancelKeyPress.AddHandler (fun o e ->
        isCancelled := true
        (!token).Cancel()
        (!token).Dispose()
        printfn "Server shut down."
    )

    try
        Async.RunSynchronously(computation, cancellationToken = (!token).Token)
    with
    | e -> printfn "%O" e //if not !isCancelled then printfn "%O" e

    0
