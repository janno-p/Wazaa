module Wazaa.Gui.MainWindow

(*
open System
open System.Diagnostics
open System.Reflection
open Gdk;
open Gtk;
open Wazaa.Logger

type MyWindow() as this =
    inherit Window(windowTitle)

    let mutable portNumber = Wazaa.Server.DefaultPortNumber
    let mutable sharedFolder = Wazaa.Config.SharedFolderPath

    let btnClearLog = new Button(Label="Clear Log")

    let txtServerLog =
        let txt = new TextView(CanFocus=true, Editable=false, WrapMode=WrapMode.WordChar, BorderWidth=1u)
        txt.Buffer.TagTable.Add(new TextTag("info", Foreground="#007F00"))
        txt.Buffer.TagTable.Add(new TextTag("warning", Foreground="#FF7F00"))
        txt.Buffer.TagTable.Add(new TextTag("error", Foreground="#FF0000"))
        txt

    let logSection =
        let expander = new Expander("Log Messages")
        let swin = new ScrolledWindow(BorderWidth=10u)
        swin.Add(txtServerLog)
        expander.Add(swin)
        expander

    let portNumberEntry = new Entry(Text=portNumber.ToString())
    let sharedFolderEntry = new Entry(Text=sharedFolder)

    let saveConfigurationButton = new Button(Label="Apply Changes", Sensitive=false)
    let selectSharedFolderButton = new Button(Label="...")

    let searchFileEntry = new Entry()
    let searchFileButton = new Button(Label="Search", Sensitive=false)

    let treeStore = new ListStore(typeof<string>, typeof<string>)
    do treeStore.AppendValues("File1.txt", "127.0.0.1:2345") |> ignore

    let treeFiles =
        let tree = new TreeView(treeStore)
        let addColumn title columnId =
            let renderer = new CellRendererText()
            let column = new TreeViewColumn(Title=title)
            column.PackStart(renderer, true)
            column.AddAttribute(renderer, "text", columnId)
            tree.AppendColumn(column) |> ignore
        addColumn "File name" 0
        addColumn "IP address / Port" 1
        tree

    let searchSection =
        let frame = new Frame("Wazaa Search")
        let searchFileLabel = new Label("Search file: ")
        let vbox = new VBox(BorderWidth=10u)
        let hboxSearchBar = new HBox()
        hboxSearchBar.PackStart(searchFileLabel, false, false, 5u)
        hboxSearchBar.PackStart(searchFileEntry, true, true, 5u)
        hboxSearchBar.PackStart(searchFileButton, false, false, 5u)
        vbox.PackStart(hboxSearchBar, false, true, 0u)
        frame.Add(vbox)
        let swin = new ScrolledWindow()
        swin.Add(treeFiles)
        vbox.PackStart(swin, true, true, 5u)
        frame

    

    let mainContainer =
        let vbox = new VBox()
        vbox.PackStart(searchSection, true, true, 3u)
        vbox.PackStart(logSection, false, true, 3u)
        vbox.PackStart(configurationSection, false, true, 3u)
        this.Add(vbox)
        vbox

    do this.SetDefaultSize(400, 300)

    do btnClearLog.Clicked.AddHandler (fun o e ->
        txtServerLog.Buffer.Clear()
    )

    do searchFileEntry.Changed.AddHandler (fun o e ->
        searchFileButton.Sensitive <-
            match searchFileEntry.Text with
            | null | "" -> false
            | _ -> true
    )

    member val PortNumber = portNumber with get, set
*)
