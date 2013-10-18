module Wazaa.Main

open System
open System.Net
open System.Threading
open Gtk

let host = "127.0.0.1"
let port = 2345

[<EntryPoint>]
let Main(args) =
    Application.Init()

    let server = Server.HttpServer(host, port)

    let window = new MainWindow.MyWindow()
    window.Show()
    Application.Run()

    server.Stop()
    0
