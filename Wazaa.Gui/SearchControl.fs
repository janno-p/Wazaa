module Wazaa.Gui.SearchControl

open System.Drawing
open System.Windows.Forms

type SearchControl() as this =
    inherit UserControl()

    let searchLabel = new Label(Text = "Search file: ",
                                TextAlign = ContentAlignment.MiddleLeft,
                                AutoSize = true,
                                Dock = DockStyle.Fill)

    let searchButton = new Button(Text = "Search", Enabled = false, Dock = DockStyle.Fill)

    let searchTextBox =
        let textBox = new TextBox(Dock = DockStyle.Fill)
        textBox.TextChanged.AddHandler(fun sender args ->
            searchButton.Enabled <- textBox.Text |> String.length > 0
            )
        textBox

    let resultListView = 
        let listView = new ListView(Dock = DockStyle.Fill,
                                    MultiSelect = false,
                                    FullRowSelect = true,
                                    HideSelection = false,
                                    View = View.Details)
        [ "File Name"; "IP Address"; "Port" ] |> Seq.iter (fun str -> listView.Columns.Add(str, -1) |> ignore)
        listView.Items.Add("<No results>") |> ignore
        listView.Enabled <- false
        listView

    do
        let panel = new TableLayoutPanel(ColumnCount = 3, RowCount = 2, Dock = DockStyle.Fill)
        panel.ColumnStyles.Add(new ColumnStyle()) |> ignore
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100.0f)) |> ignore
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)) |> ignore
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)) |> ignore
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0f)) |> ignore
        panel.Controls.Add(searchLabel)
        panel.Controls.Add(searchTextBox)
        panel.Controls.Add(searchButton)
        panel.Controls.Add(resultListView)
        panel.SetColumnSpan(resultListView, 3)
        this.Controls.Add(panel)

    do this.Dock <- DockStyle.Fill
