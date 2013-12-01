module Wazaa.Gui.MainForm

open System.Windows.Forms
open Wazaa.Logger
open Wazaa.Gui.LogControl

type MainForm() as form =
    inherit Form()

    let logControl = new LogControl()

    do GlobalLogger <- logControl

    do logControl.Dock <- DockStyle.Fill
    do form.Controls.Add(logControl)
