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
        let txt = new TextView(CanFocus=true, Editable=false, WrapMode=WrapMode.WordChar)
        txt.Buffer.TagTable.Add(new TextTag("info", Foreground="#007F00"))
        txt.Buffer.TagTable.Add(new TextTag("warning", Foreground="#FF7F00"))
        txt.Buffer.TagTable.Add(new TextTag("error", Foreground="#FF0000"))
        txt.BorderWidth <- 1u
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

    let searchSection =
        let frame = new Frame("Wazaa Search")

        frame

    let configurationSection =
        let expander = new Expander("Configuration")
        let vbox = new VBox()
        let customizationFrame = new Frame("Application customization")
        vbox.PackStart(customizationFrame, false, true, 3u)
        let customizationTable = new Table(2u, 3u, false)
        let portLabel = new Label("Port number: ")
        portLabel.Justify <- Justification.Right
        customizationTable.Attach(portLabel, 0u, 1u, 0u, 1u, AttachOptions.Shrink, AttachOptions.Shrink, 5u, 5u)
        customizationTable.Attach(portNumberEntry, 1u, 2u, 0u, 1u)
        let folderLabel = new Label("Shared folder: ")
        folderLabel.Justify <- Justification.Right
        customizationTable.Attach(folderLabel, 0u, 1u, 1u, 2u, AttachOptions.Shrink, AttachOptions.Shrink, 5u, 5u)
        let hboxSharedFolder = new HBox()
        hboxSharedFolder.PackStart(sharedFolderEntry, true, true, 0u)
        hboxSharedFolder.PackEnd(selectSharedFolderButton, false, false, 0u)
        customizationTable.Attach(hboxSharedFolder, 1u, 2u, 1u, 2u)
        customizationTable.ColumnSpacing <- 5u
        customizationTable.RowSpacing <- 5u
        let hboxButton = new HBox()
        hboxButton.PackEnd(saveConfigurationButton, false, false, 0u)
        customizationTable.Attach(hboxButton, 1u, 2u, 2u, 3u)
        customizationTable.BorderWidth <- 10u
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

    (*






    let entSearchFile = new Entry()
    let btnSearchFile = new Button(Label="Search")
    let treeFiles = new TreeView()

    let notebook =
        let vbox = new VBox(false, 0)
        let hbox = new HBox(false, 0)
        let lbl = new Label("File name: ")
        hbox.PackStart(lbl, false, false, 0u)
        hbox.PackStart(entSearchFile, true, true, 0u)
        hbox.PackStart(btnSearchFile, false, false, 0u)
        vbox.PackStart(hbox, false, false, 0u)
        let sw2 = new ScrolledWindow()
        sw2.Add(treeFiles)
        treeFiles.BorderWidth <- 1u
        treeFiles.ModifyBg(StateType.Normal, new Gdk.Color(128uy, 0uy, 0uy))
        let fileNameColumn = new TreeViewColumn(Title="File name")
        treeFiles.AppendColumn(fileNameColumn) |> ignore
        let ipPortColumn = new TreeViewColumn(Title="IP address / Port")
        treeFiles.AppendColumn(ipPortColumn) |> ignore
        let store = new ListStore(typeof<string>, typeof<string>)
        store.AppendValues("File1.txt", "127.0.0.1:2345") |> ignore
        treeFiles.Model <- store
        let rendererName = new CellRendererText()
        fileNameColumn.PackStart(rendererName, true)
        let rendererIPPort = new CellRendererText()
        ipPortColumn.PackStart(rendererIPPort, true)
        fileNameColumn.AddAttribute(rendererName, "text", 0)
        ipPortColumn.AddAttribute(rendererIPPort, "text", 1)
        vbox.PackStart(sw2, true, true, 0u)
        vpaned.Pack1(vbox, true, false)
        notebook

    *)

    let regularTabColor = new Gdk.Color(0uy, 0uy, 0uy)
    let noticeTabColor = new Gdk.Color(0uy, 127uy, 0uy)
    let errorTabColor = new Gdk.Color(127uy, 0uy, 0uy)

    let OnClearLogButtonClickedEvent o e =
        txtServerLog.Buffer.Clear()

    let OnDeleteEvent o (e:DeleteEventArgs) =
        Application.Quit()
        e.RetVal <- true

    do btnClearLog.Clicked.AddHandler (fun o e -> OnClearLogButtonClickedEvent o e)

    do this.SetDefaultSize(400,300)
    do this.DeleteEvent.AddHandler (fun o e -> OnDeleteEvent o e)

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
