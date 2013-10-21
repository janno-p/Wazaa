module Wazaa.Gui.Main

open System
open System.Configuration
open System.Net
open System.Threading
open Gtk
open Wazaa.Config
open Wazaa.Logger
open Wazaa.Server

[<EntryPoint>]
let Main(args) =
    Application.Init()

    let window = new MainWindow.MyWindow()
    GlobalLogger <- window

    let server = HttpServer(LocalEndPoint)
    server |> RunServerAsync |> Async.Start

    window.Show()
    Application.Run()

    server.Stop()
    0
