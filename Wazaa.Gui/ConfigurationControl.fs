module Wazaa.Gui.ConfigurationControl

open System.Drawing
open System.Windows.Forms
open Wazaa.Config

type ConfigurationControl() as this =
    inherit UserControl()

    let portChangedEvent = new Event<_>()

    let portLabel = new Label(Text = "Port: ", TextAlign = ContentAlignment.MiddleRight)

    let portTextBox =
        let textBox = new TextBox(Text = LocalEndPoint.Port.ToString())
        textBox.Leave.AddHandler (fun sender args ->
            match Wazaa.Config.ParseUShort textBox.Text with
            | Some num -> portChangedEvent.Trigger(int num)
            | None ->
                match MessageBox.Show("Invalid port number. Do you want to revert changes.", "", MessageBoxButtons.YesNo) with
                | DialogResult.Yes -> textBox.Text <- LocalEndPoint.Port.ToString()
                | _ -> textBox.Focus() |> ignore
            )
        textBox

    let sharedPathLabel = new Label(Text = "Shared path: ", TextAlign = ContentAlignment.MiddleRight)
    let sharedPathTextBox = new TextBox(Text = SharedFolderPath, Dock = DockStyle.Fill)

    let layout =
        let layout = new TableLayoutPanel(ColumnCount = 2, RowCount = 3, Dock = DockStyle.Fill)
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100.0f)) |> ignore
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100.0f)) |> ignore
        layout.Controls.Add(portLabel, 0, 0)
        layout.Controls.Add(portTextBox, 1, 0)
        layout.Controls.Add(sharedPathLabel, 0, 1)
        layout.Controls.Add(sharedPathTextBox, 1, 1)
        this.Controls.Add(layout)
        layout

    do this.Dock <- DockStyle.Fill

    [<CLIEvent>]
    member this.PortChanged = portChangedEvent.Publish
