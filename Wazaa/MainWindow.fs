module Wazaa.MainWindow

open System
open System.Diagnostics
open System.Reflection
open Gtk;

let windowTitle =
    let assembly = Assembly.GetExecutingAssembly()
    let fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location)
    sprintf "Wazaa v. %s" fileVersionInfo.FileVersion

type MyWindow() as this =
    inherit Window(windowTitle)

    do this.SetDefaultSize(400,300)
    do this.DeleteEvent.AddHandler(fun o e -> this.OnDeleteEvent(o,e))
    do this.ShowAll()

    member this.OnDeleteEvent(o,e:DeleteEventArgs) = 
        Application.Quit ()
        e.RetVal <- true
