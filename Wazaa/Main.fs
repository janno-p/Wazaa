module Wazaa.Gui.Main

open System
open System.Net
open System.Threading
open Gtk
open Wazaa

let host = "127.0.0.1"
let port = 2345

[<EntryPoint>]
let Main(args) =
    Application.Init()

    let window = new MainWindow.MyWindow()
    Server.logger <- window

    let server = Server.HttpServer(host, port)
    server |> Server.RunServerAsync |> Async.Start

    window.Show()
    Application.Run()

    server.Stop()
    0
