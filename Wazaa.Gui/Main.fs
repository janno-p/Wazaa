module Wazaa.Gui.Main

open System
open System.Configuration
open System.Net
open System.Threading
open Gtk
open Wazaa

[<EntryPoint>]
let Main(args) =
    Application.Init()

    let localEndPoint = Config.ReadServerConfiguration()

    let window = new MainWindow.MyWindow()
    Server.logger <- window

    let server = Server.HttpServer(localEndPoint)
    server |> Server.RunServerAsync |> Async.Start

    window.Show()
    Application.Run()

    server.Stop()
    0
