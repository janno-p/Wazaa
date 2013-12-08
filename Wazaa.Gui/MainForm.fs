module Wazaa.Gui.MainForm

open System.Diagnostics
open System.Drawing
open System.Reflection
open System.Windows.Forms
open Wazaa.Logger
open Wazaa.Gui.ConfigurationControl
open Wazaa.Gui.LogControl
open Wazaa.Gui.PeerControl
open Wazaa.Gui.SearchControl
open Wazaa.Server

let formTitle =
    let assembly = Assembly.GetExecutingAssembly()
    let fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location)
    sprintf "Wazaa v. %s" fileVersionInfo.FileVersion

type MainForm() as form =
    inherit Form()

    let searchControl = new SearchControl()
    let configurationControl = new ConfigurationControl()
    let peersControl = new PeerControl()
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

    let logControl =
        let control = new LogControl()
        logTabPage.Controls.Add(control)
        GlobalLogger <- control
        control

    let tabControl =
        let tabControl = new TabControl(Dock = DockStyle.Fill)
        [logTabPage; configurationTabPage; peersTabPage] |> Seq.iter tabControl.TabPages.Add
        tabControl

    do
        form.Text <- formTitle
        form.MinimumSize <- new Size(380, 500)

        let container = new SplitContainer(Orientation = Orientation.Horizontal,
                                           Dock = DockStyle.Fill,
                                           BorderStyle = BorderStyle.FixedSingle,
                                           FixedPanel = FixedPanel.Panel2,
                                           SplitterWidth = 1)
        container.Panel1.Controls.Add(searchControl)
        container.Panel2.Controls.Add(tabControl)

        form.Controls.Add(container)
        container.SplitterDistance <- 275

    member this.Configuration = configurationControl
    member this.Search = searchControl
