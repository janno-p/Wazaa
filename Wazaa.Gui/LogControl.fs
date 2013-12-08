module Wazaa.Gui.LogControl

open System.Drawing
open System.Windows.Forms
open Wazaa.Logger

type LogControl() as this =
    inherit UserControl()

    let logTextView = new RichTextBox(ReadOnly = true,
                                      WordWrap = true,
                                      Dock = DockStyle.Fill,
                                      HideSelection = false,
                                      SelectionStart = 0,
                                      SelectionLength = 0)

    let appendText color message =
        let restoreSelection = (logTextView.SelectionStart = logTextView.TextLength && logTextView.SelectionLength = 0)
        logTextView.SelectionColor <- color
        logTextView.AppendText message
        logTextView.AppendText System.Environment.NewLine
        if restoreSelection then
            logTextView.SelectionStart <- logTextView.TextLength
            logTextView.SelectionLength <- 0

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
