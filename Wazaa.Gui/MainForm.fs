module Wazaa.Gui.MainForm

open System.Diagnostics
open System.Drawing
open System.Reflection
open System.Windows.Forms
open Wazaa.Logger
open Wazaa.Gui.ConfigurationControl
open Wazaa.Gui.LogControl
open Wazaa.Gui.PeerControl

let formTitle =
    let assembly = Assembly.GetExecutingAssembly()
    let fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location)
    sprintf "Wazaa v. %s" fileVersionInfo.FileVersion

type MainForm() as form =
    inherit Form()

    let logControl = new LogControl()
    let configurationControl = new ConfigurationControl()
    let peersControl = new PeerControl()
    let splitContainer = new SplitContainer(Orientation = Orientation.Horizontal, Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, SplitterWidth = 1)
    let tabControl = new TabControl(Dock = DockStyle.Fill)
    let logTabPage = new TabPage("Log")
    let configurationTabPage = new TabPage("Configuration")
    let peersTabPage = new TabPage("Peers")

    let createLayout () =
        let layout = new TableLayoutPanel(ColumnCount = 3, RowCount = 1, Dock = DockStyle.Fill)
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.0f)) |> ignore
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300.0f)) |> ignore
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.0f)) |> ignore
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0f)) |> ignore
        layout

    let configurationControl =
        let layout = createLayout()
        let control = new ConfigurationControl()
        layout.Controls.Add(control, 1, 0)
        configurationTabPage.Controls.Add(layout)
        control

    let peersControl =
        let layout = createLayout()
        let control = new PeerControl()
        layout.Controls.Add(control, 1, 0)
        peersTabPage.Controls.Add(layout)
        control

    do GlobalLogger <- logControl

    do logTabPage.Controls.Add(logControl)

    do [logTabPage; configurationTabPage; peersTabPage] |> Seq.iter tabControl.TabPages.Add

    do splitContainer.Panel2.Controls.Add(tabControl)

    do form.Text <- formTitle
    do form.Controls.Add(splitContainer)

    member this.Configuration = configurationControl
