module Wazaa.Gui.LogControl

open System.Drawing
open System.Windows.Forms
open Wazaa.Logger

type LogControl() as this =
    inherit UserControl()

    let logTextView = new RichTextBox(Enabled = false, WordWrap = true, Dock = DockStyle.Fill)

    let appendText color message =
        logTextView.SelectionColor <- color
        logTextView.AppendText message
        logTextView.AppendText System.Environment.NewLine

    let threadSafe func =
        match this.InvokeRequired with
        | true -> this.Invoke (new MethodInvoker(fun x -> func())) |> ignore
        | _ -> func()

    do this.Dock <- DockStyle.Fill
    do this.Controls.Add(logTextView)

    interface ILogger with
        member this.Info message = threadSafe (fun () -> appendText Color.Green message)
        member this.Warning message = threadSafe (fun () -> appendText Color.Orange message)
        member this.Error message = threadSafe (fun () -> appendText Color.Red message)
