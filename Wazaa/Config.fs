module Wazaa.Config

open System
open System.Configuration
open System.IO
open System.Net
open System.Net.NetworkInformation
open System.Net.Sockets
open System.Reflection
open FSharp.Data.Json
open FSharp.Data.Json.Extensions

let (@@) x y = Path.Combine(x, y)

type PeerInfo = {
    Host: string
    Port: int
}

let mutable SharedFolderPath =
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) @@ "wazaa"

let KnownPeers =
    let rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    let path = rootPath @@ "Data" @@ "machines.txt"
    if File.Exists(path) then
        let data = JsonValue.Load(path)
        [ for peer in data -> { Host = peer.[0].AsString(); Port = peer.[1].AsInteger() } ]
    else
        []:list<PeerInfo>

let LocalEndPoint =
    let host =
        let unicastAddresses = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                               |> Seq.filter (fun x -> match x.OperationalStatus with | OperationalStatus.Up -> true | _ -> false)
                               |> Seq.collect (fun x -> x.GetIPProperties().UnicastAddresses)
        try
            unicastAddresses
            |> Seq.filter (fun x -> match x.DuplicateAddressDetectionState with | DuplicateAddressDetectionState.Preferred -> true | _ -> false)
            |> Seq.filter (fun x -> match uint32(x.AddressPreferredLifetime) with | UInt32.MaxValue -> false | _ -> true)
            |> Seq.map (fun x -> x.Address)
            |> Seq.head
        with :? System.NotImplementedException -> // Linux interfaces have some unimplemented methods
            unicastAddresses
            |> Seq.map (fun x -> x.Address)
            |> Seq.filter (fun x -> match x.AddressFamily with | AddressFamily.InterNetwork -> true | _ -> false)
            |> Seq.head

    let mutable port = 0
    if not (Int32.TryParse(ConfigurationManager.AppSettings.["port"], &port)) then
        port <- 0

    new IPEndPoint(host, port)
