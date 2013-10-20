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

type MyWindow() as this =
    inherit Window(windowTitle)

    let txtServerLog =
        let txt = new TextView(CanFocus=true, Editable=false, WrapMode=WrapMode.WordChar)
        txt.Buffer.TagTable.Add(new TextTag("info", Foreground="#007F00"))
        txt.Buffer.TagTable.Add(new TextTag("warning", Foreground="#FF7F00"))
        txt.Buffer.TagTable.Add(new TextTag("error", Foreground="#FF0000"))
        txt.BorderWidth <- 1u
        txt

    let btnClearLog = new Button(Label="Clear Log")

    let entSearchFile = new Entry()
    let btnSearchFile = new Button(Label="Search")
    let treeFiles = new TreeView()

    let notebook =
        let notebook = new Notebook()
        let vpaned = new VPaned()
        notebook.Add(vpaned)
        notebook.SetTabLabelText(vpaned, "Wazaa")
        this.Add(notebook)
        let scrolledWindow = new ScrolledWindow()
        scrolledWindow.Add(txtServerLog)
        vpaned.Pack2(scrolledWindow, true, true)
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

    let labelSettings = new Label("Settings :)")
    do notebook.Add(labelSettings)
    do notebook.SetTabLabelText(labelSettings, "Settings")

    let regularTabColor = new Gdk.Color(0uy, 0uy, 0uy)
    let noticeTabColor = new Gdk.Color(0uy, 127uy, 0uy)
    let errorTabColor = new Gdk.Color(127uy, 0uy, 0uy)

    let OnClearLogButtonClickedEvent o e =
        txtServerLog.Buffer.Clear()

    let OnSwitchPageEvent o (e:SwitchPageArgs) =
        let widget = notebook.GetNthPage(int(e.PageNum))
        let label = notebook.GetTabLabel(widget)
        label.ModifyFg(StateType.Active, regularTabColor)
    
    let OnDeleteEvent o (e:DeleteEventArgs) =
        Application.Quit()
        e.RetVal <- true

    do notebook.SwitchPage.AddHandler (fun o e -> OnSwitchPageEvent o e)
    do btnClearLog.Clicked.AddHandler (fun o e -> OnClearLogButtonClickedEvent o e)

    do this.SetDefaultSize(400,300)
    do this.DeleteEvent.AddHandler (fun o e -> OnDeleteEvent o e)

    do this.ShowAll()

    let AppendToServerLog message (tag:string) =
        let mutable endIter = txtServerLog.Buffer.EndIter
        txtServerLog.Buffer.InsertWithTagsByName(&endIter, message + Environment.NewLine, tag)
        txtServerLog.ScrollToIter(endIter, 0.0, false, 0.0, 0.0) |> ignore

    interface ILogger with
        member this.Info message =
            Gtk.Application.Invoke(fun sender args -> AppendToServerLog message "info")
        member this.Warning message =
            Gtk.Application.Invoke(fun sender args -> AppendToServerLog message "warning")
        member this.Error message =
            Gtk.Application.Invoke(fun sender args -> AppendToServerLog message "error")
