module Wazaa.Gui.Main

open System
open System.Threading
open System.Windows.Forms
open Wazaa.Config
open Wazaa.Gui.MainForm
open Wazaa.Server

[<EntryPoint>]
[<STAThread>]
let Main(args) =
    use form = new MainForm()

    let token = ref (new CancellationTokenSource())
    Async.Start(StartServer LocalEndPoint form.Search, (!token).Token)

    form.Configuration.PortChanged.AddHandler(fun _ port ->
        (!token).Cancel()
        (!token).Dispose()
        LocalEndPoint.Port <- port
        token := new CancellationTokenSource()
        Async.Start(StartServer LocalEndPoint form.Search, (!token).Token))

    Application.EnableVisualStyles()
    Application.Run(form)

    0
