module Wazaa.Gui.Main

open System.Threading
open System.Windows.Forms
open Wazaa.Config
open Wazaa.Gui.MainForm
open Wazaa.Server

[<EntryPoint>]
let Main(args) =
    use form = new MainForm()

    let tokenSource = ref (new CancellationTokenSource())
    let server = ref (HttpServer(LocalEndPoint))
    Async.Start(!server |> RunServerAsync, (!tokenSource).Token)

    form.Configuration.PortChanged.AddHandler(fun sender port ->
        (!tokenSource).Cancel()
        (!tokenSource).Dispose()
        LocalEndPoint.Port <- port
        tokenSource := new CancellationTokenSource()
        server := HttpServer(LocalEndPoint)
        Async.Start(!server |> RunServerAsync, (!tokenSource).Token)
        )

    Application.EnableVisualStyles()
    Application.Run(form)

    0
