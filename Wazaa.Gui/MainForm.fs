module Wazaa.Gui.MainForm

open System.Diagnostics
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

    do GlobalLogger <- logControl

    do logTabPage.Controls.Add(logControl)
    do configurationTabPage.Controls.Add(configurationControl)
    do peersTabPage.Controls.Add(peersControl)

    do [logTabPage; configurationTabPage; peersTabPage] |> Seq.iter tabControl.TabPages.Add

    do splitContainer.Panel2.Controls.Add(tabControl)

    do form.Text <- formTitle
    do form.Controls.Add(splitContainer)
