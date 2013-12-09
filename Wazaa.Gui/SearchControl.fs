module Wazaa.Gui.SearchControl

open System.Drawing
open System.IO
open System.Windows.Forms
open Wazaa.Client
open Wazaa.Config
open Wazaa.Server

type SearchControl() as this =
    inherit UserControl()

    let searchLabel = new Label(Text = "Search file: ",
                                TextAlign = ContentAlignment.MiddleLeft,
                                AutoSize = true,
                                Dock = DockStyle.Fill)

    let searchButton = new Button(Text = "Search", Enabled = false, Dock = DockStyle.Fill)

    let searchTextBox =
        let textBox = new TextBox(Dock = DockStyle.Fill)
        textBox.TextChanged.AddHandler(fun sender args -> searchButton.Enabled <- textBox.Text |> String.length > 0)
        textBox.KeyDown.AddHandler(fun sender args -> match args.KeyCode with | Keys.Enter -> searchButton.PerformClick() | _ -> ())
        textBox

    let resultListView = 
        let listView = new ListView(Dock = DockStyle.Fill,
                                    MultiSelect = false,
                                    FullRowSelect = true,
                                    HideSelection = false,
                                    View = View.Details,
                                    Enabled = false)
        listView.Columns.Add("File Name", 180) |> ignore
        listView.Columns.Add("IP Address", 120) |> ignore
        listView.Columns.Add("Port", 50) |> ignore
        listView.Items.Add("<No results>") |> ignore
        listView.DoubleClick.AddHandler(fun sender args ->
            let file = listView.SelectedItems.[0].Tag :?> FileRecord
            let dialog = new SaveFileDialog()
            dialog.FileName <- file.Name
            dialog.Filter <- "All Files (*.*)|*.*"
            match dialog.ShowDialog() with
            | DialogResult.OK -> GetFile file (new FileInfo(dialog.FileName))
            | _ -> ())
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

        searchButton.Click.AddHandler(fun sender args ->
            resultListView.Items.Clear()
            resultListView.Enabled <- false
            resultListView.Items.Add("<No results>") |> ignore
            SearchFile KnownPeers { Name = searchTextBox.Text
                                    SendIP = LocalEndPoint.Address.ToString()
                                    SendPort = (ConvertInt32ToUInt16 LocalEndPoint.Port)
                                    TimeToLive = DefaultTimeToLive
                                    Id = ""
                                    NoAsk = "" })

    do this.Dock <- DockStyle.Fill

    let threadSafe func =
        match this.InvokeRequired with
        | true -> this.Invoke (new MethodInvoker(fun x -> func())) |> ignore
        | _ -> func()

    let appendItems (files : FileRecord list) =
        if not resultListView.Enabled then
            resultListView.Items.Clear()
        files
        |> Seq.filter (fun x -> searchTextBox.TextLength > 0 && (x.Name.ToLower().Contains(searchTextBox.Text.ToLower())))
        |> Seq.iter (fun x -> if not ([ for i in resultListView.Items -> i.Tag :?> FileRecord ] |> Seq.exists (fun t -> t = x)) then
                                  resultListView.Items.Add(new ListViewItem([| x.Name; x.Owner.Address.ToString(); x.Owner.Port.ToString() |], Tag = x)) |> ignore)
        if resultListView.Items.Count > 0 then
            resultListView.Enabled <- true
        else
            resultListView.Items.Add("<No results>") |> ignore

    interface IMessageListener with
        member this.FilesFound files = threadSafe (fun () -> appendItems files)
