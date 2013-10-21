module Wazaa.Config

open System
open System.Configuration
open System.IO
open System.Net
open System.Net.NetworkInformation
open System.Net.Sockets
open System.Reflection
open Newtonsoft.Json

let mutable SharedFolderPath =
    let userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    System.IO.Path.Combine(userDirectory, "wazaa")

let KnownPeers =
    let rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    let path = Path.Combine(rootPath, "Data", "machines.txt")
    if File.Exists(path) then
        use reader = new StreamReader(path)
        match JsonConvert.DeserializeObject(reader.ReadToEnd()) with
        | :? Linq.JArray as arr ->
            for x in arr do
                printfn "%O" x
        | _ -> ()
    []:list<IPEndPoint>

let LocalEndPoint =
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
