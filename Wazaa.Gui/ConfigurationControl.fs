module Wazaa.Gui.ConfigurationControl

open System.Drawing
open System.Windows.Forms
open Wazaa.Config

type ConfigurationControl() as this =
    inherit UserControl()

    let portLabel = new Label(Text = "Port: ", TextAlign = ContentAlignment.MiddleRight)
    let portTextBox = new TextBox(Text = LocalEndPoint.Port.ToString())

    let sharedPathLabel = new Label(Text = "Shared path: ", TextAlign = ContentAlignment.MiddleRight)
    let sharedPathTextBox = new TextBox(Text = SharedFolderPath)

    let x = new FlowLayoutPanel(Dock = DockStyle.Fill)
    do [portLabel :> Control; portTextBox :> Control; sharedPathLabel :> Control; sharedPathTextBox :> Control] |> Seq.iter x.Controls.Add

    do this.Controls.Add(x)
    do this.Dock <- DockStyle.Fill


(*
let configurationSection =
        let expander = new Expander("Configuration")
        let vbox = new VBox(BorderWidth=10u)
        let customizationFrame = new Frame("Application customization")
        vbox.PackStart(customizationFrame, false, true, 3u)
        let customizationTable =
            let table = new Table(2u, 3u, false, ColumnSpacing=5u, RowSpacing=5u, BorderWidth=10u)
            table.Attach(portLabel, 0u, 1u, 0u, 1u, AttachOptions.Shrink, AttachOptions.Shrink, 5u, 5u)
            table.Attach(portNumberEntry, 1u, 2u, 0u, 1u)
            table.Attach(folderLabel, 0u, 1u, 1u, 2u, AttachOptions.Shrink, AttachOptions.Shrink, 5u, 5u)
            let hboxSharedFolder = new HBox()
            hboxSharedFolder.PackStart(sharedFolderEntry, true, true, 0u)
            hboxSharedFolder.PackEnd(selectSharedFolderButton, false, false, 0u)
            table.Attach(hboxSharedFolder, 1u, 2u, 1u, 2u)
            let hboxButton = new HBox()
            hboxButton.PackEnd(saveConfigurationButton, false, false, 0u)
            table.Attach(hboxButton, 1u, 2u, 2u, 3u)
            table
        customizationFrame.Add(customizationTable)
        let knownPeersFrame = new Frame("Known peers")
        vbox.PackStart(knownPeersFrame, false, true, 3u)
        expander.Add(vbox)
        expander
*)
