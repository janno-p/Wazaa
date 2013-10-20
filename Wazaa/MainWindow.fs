module Wazaa.MainWindow

open System
open System.Diagnostics
open System.Reflection
open Gdk;
open Gtk;
open Wazaa.Logger

let windowTitle =
    let assembly = Assembly.GetExecutingAssembly()
    let fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location)
    sprintf "Wazaa v. %s" fileVersionInfo.FileVersion

let defaultWazaaDirectory =
    let userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    System.IO.Path.Combine(userDirectory, "wazaa")

type MyWindow() as this =
    inherit Window(windowTitle)

    let mutable portNumber = Wazaa.Server.DefaultPortNumber
    let mutable sharedFolder = defaultWazaaDirectory

    let btnClearLog = new Button(Label="Clear Log")

    let txtServerLog =
        let txt = new TextView(CanFocus=true, Editable=false, WrapMode=WrapMode.WordChar, BorderWidth=1u)
        txt.Buffer.TagTable.Add(new TextTag("info", Foreground="#007F00"))
        txt.Buffer.TagTable.Add(new TextTag("warning", Foreground="#FF7F00"))
        txt.Buffer.TagTable.Add(new TextTag("error", Foreground="#FF0000"))
        txt

    let logSection =
        let expander = new Expander("Log Messages")
        let swin = new ScrolledWindow()
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

    let configurationSection =
        let expander = new Expander("Configuration")
        let vbox = new VBox()
        let customizationFrame = new Frame("Application customization")
        vbox.PackStart(customizationFrame, false, true, 3u)
        let portLabel = new Label("Port number: ")
        portLabel.Justify <- Justification.Right
        let folderLabel = new Label("Shared folder: ")
        folderLabel.Justify <- Justification.Right
        let customizationTable =
            let table = new Table(2u, 3u, false, ColumnSpacing=5u, RowSpacing=5u)
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
            table.BorderWidth <- 10u
            table
        customizationFrame.Add(customizationTable)
        let knownPeersFrame = new Frame("Known peers")
        vbox.PackStart(knownPeersFrame, false, true, 3u)
        expander.Add(vbox)
        expander

    let mainContainer =
        let vbox = new VBox()
        vbox.PackStart(searchSection, true, true, 3u)
        vbox.PackStart(logSection, false, true, 3u)
        vbox.PackStart(configurationSection, false, true, 3u)
        this.Add(vbox)
        vbox

    let regularTabColor = new Gdk.Color(0uy, 0uy, 0uy)
    let noticeTabColor = new Gdk.Color(0uy, 127uy, 0uy)
    let errorTabColor = new Gdk.Color(127uy, 0uy, 0uy)

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

    do this.DeleteEvent.AddHandler (fun o e ->
        Application.Quit()
        e.RetVal <- true
    )

    do this.ShowAll()

    let AppendToServerLog message (tag:string) =
        let mutable endIter = txtServerLog.Buffer.EndIter
        txtServerLog.Buffer.InsertWithTagsByName(&endIter, message + Environment.NewLine, tag)
        txtServerLog.ScrollToIter(endIter, 0.0, false, 0.0, 0.0) |> ignore

    member val PortNumber = portNumber with get, set

    interface ILogger with
        member this.Info message =
            Gtk.Application.Invoke(fun sender args -> AppendToServerLog message "info")
        member this.Warning message =
            Gtk.Application.Invoke(fun sender args -> AppendToServerLog message "warning")
        member this.Error message =
            Gtk.Application.Invoke(fun sender args -> AppendToServerLog message "error")
