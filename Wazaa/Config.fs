module Wazaa.Config

open System
open System.Configuration
open System.Net

let ReadServerConfiguration () =
    let host = ConfigurationManager.AppSettings.["host"]

    let mutable port = 0
    if not (Int32.TryParse(ConfigurationManager.AppSettings.["port"], &port)) then
        port <- 0

    new IPEndPoint(IPAddress.Parse(host), port)
