module Wazaa.Gui.Main

open System.Windows.Forms
open Wazaa.Config
open Wazaa.Gui.MainForm
open Wazaa.Server

[<EntryPoint>]
let Main(args) =
    use form = new MainForm()

    let server = HttpServer(LocalEndPoint)
    server |> RunServerAsync |> Async.Start

    Application.Run(form)

    0
