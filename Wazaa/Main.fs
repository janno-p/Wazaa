namespace Wazaa
    module Main =
        
        open System
        open System.Net
        open Gtk

        let host = "127.0.0.1"
        let port = 2345
    
        [<EntryPoint>]
        let Main(args) = 
            (*Application.Init()
            let win = new MainWindow.MyWindow()
            win.Show()
            Application.Run()*)
            Server.HttpServer(host, port)
            0

