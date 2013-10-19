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

    let notebook = new Notebook()
    do this.Add(notebook)

    let labelWazaa = new Label("Wazaa :)")
    do notebook.Add(labelWazaa)
    do notebook.SetTabLabelText(labelWazaa, "Wazaa")

    let lblServerLog = new Label("Server Log")
    let txtServerLog = new TextView(CanFocus = true, Editable = false, WrapMode = WrapMode.WordChar)
    let scrollWindow =
        txtServerLog.Buffer.TagTable.Add(new TextTag("info", Foreground="#007F00"))
        txtServerLog.Buffer.TagTable.Add(new TextTag("warning", Foreground="#FF7F00"))
        txtServerLog.Buffer.TagTable.Add(new TextTag("error", Foreground="#FF0000"))
        let w = new ScrolledWindow()
        w.Add(txtServerLog)
        txtServerLog.BorderWidth <- 1u
        let vb = new VBox(true, 0)
        let hb = new HBox(true, 0)
        vb.PackStart(hb, true, true, 10u)
        hb.PackStart(w, true, true, 10u)
        notebook.Add(vb)
        notebook.SetTabLabel(vb, lblServerLog)
        w

    let labelSettings = new Label("Settings :)")
    do notebook.Add(labelSettings)
    do notebook.SetTabLabelText(labelSettings, "Settings")

    let regularTabColor = new Gdk.Color(0uy, 0uy, 0uy)
    let noticeTabColor = new Gdk.Color(0uy, 127uy, 0uy)
    let errorTabColor = new Gdk.Color(127uy, 0uy, 0uy)

    let OnSwitchPageEvent o (e:SwitchPageArgs) =
        let widget = notebook.GetNthPage(int(e.PageNum))
        let label = notebook.GetTabLabel(widget)
        label.ModifyFg(StateType.Active, regularTabColor)

    do notebook.SwitchPage.AddHandler (fun o e -> OnSwitchPageEvent o e)

    let mutable color = new Gdk.Color()
    do Gdk.Color.Parse("red", &color) |> ignore
    do labelSettings.ModifyFg(StateType.Normal, color)
    do lblServerLog.ModifyFg(StateType.Active, color)

    do this.SetDefaultSize(400,300)
    do this.DeleteEvent.AddHandler(fun o e -> this.OnDeleteEvent(o, e))

    do this.ShowAll()

    let AppendToServerLog message (tag:string) =
        let mutable endIter = txtServerLog.Buffer.EndIter
        txtServerLog.Buffer.InsertWithTagsByName(&endIter, message + Environment.NewLine, tag)
        txtServerLog.ScrollToIter(endIter, 0.0, false, 0.0, 0.0) |> ignore
        if notebook.GetTabLabel(notebook.CurrentPageWidget) :?> Label <> lblServerLog then
            let color =
                match tag with
                | "info" | "warning" -> noticeTabColor
                | _ -> errorTabColor
            lblServerLog.ModifyFg(StateType.Active, color)

    member this.OnDeleteEvent(o,e:DeleteEventArgs) = 
        Application.Quit ()
        e.RetVal <- true

    interface ILogger with
        member this.Info message =
            Gtk.Application.Invoke(fun sender args -> AppendToServerLog message "info")
        member this.Warning message =
            Gtk.Application.Invoke(fun sender args -> AppendToServerLog message "warning")
        member this.Error message =
            Gtk.Application.Invoke(fun sender args -> AppendToServerLog message "error")
