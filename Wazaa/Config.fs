module Wazaa.Config

open System
open System.Configuration
open System.Net
open System.Net.NetworkInformation
open System.Net.Sockets

let mutable SharedFolderPath =
    let userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    System.IO.Path.Combine(userDirectory, "wazaa")

let KnownPeers = []:list<IPEndPoint>

let ReadServerConfiguration () =
    let host = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                     |> Seq.filter (fun x -> match x.OperationalStatus with | OperationalStatus.Up -> true | _ -> false)
                     |> Seq.collect (fun x -> x.GetIPProperties().UnicastAddresses)
                     |> Seq.filter (fun x -> match x.DuplicateAddressDetectionState with | DuplicateAddressDetectionState.Preferred -> true | _ -> false)
                     |> Seq.filter (fun x -> match uint32(x.AddressPreferredLifetime) with | UInt32.MaxValue -> false | _ -> true)
                     |> Seq.map (fun x -> x.Address)
                     |> Seq.head

    let mutable port = 0
    if not (Int32.TryParse(ConfigurationManager.AppSettings.["port"], &port)) then
        port <- 0

    new IPEndPoint(host, port)
