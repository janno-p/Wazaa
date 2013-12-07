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

let mutable SharedFolderPath =
    match ConfigurationManager.AppSettings.["shared_path"] with
    | null | "" -> Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) @@ "wazaa"
    | path -> path

let Parse conversionFunc value =
    try
        Some (conversionFunc value)
    with
        _ -> None

let ParseIPAddress = Parse IPAddress.Parse
let ParseInt = Parse Int32.Parse
let ParseUShort = Parse UInt16.Parse
let ConvertUShort = Parse (Convert.ToUInt16 : decimal -> uint16)

let ParsePort value =
    match value with
    | JsonValue.String p -> ParseUShort p
    | JsonValue.Number p -> ConvertUShort p
    | _ -> None

let ParseIPAddressAndPort data =
    match data with
    | [|JsonValue.String a; _|] -> match ParseIPAddress(a) with
                                   | Some address -> match ParsePort(data.[1]) with
                                                     | Some port -> Some (new IPEndPoint(address, (int port)))
                                                     | _ -> None
                                   | _ -> None
    | _ -> None

let ParseIPEndPoint data =
    match data with
    | JsonValue.Array arr -> ParseIPAddressAndPort(arr)
    | _ -> None

let KnownPeers : IPEndPoint list =
    let rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    let path = rootPath @@ "machines.txt"
    if File.Exists(path) then
        let data = JsonValue.Load(path)
        match data with
        | JsonValue.Array arr -> arr |> Seq.choose ParseIPEndPoint |> Seq.toList
        | _ -> []
    else
        []

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

    let port = match ParseInt ConfigurationManager.AppSettings.["port"] with
               | Some port -> port
               | _ -> 0

    new IPEndPoint(host, port)

let UpdatePortNumber port =
    let configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
    configuration.AppSettings.Settings.["port"].Value <- port.ToString()
    configuration.Save()
    ConfigurationManager.RefreshSection("appSettings")
    LocalEndPoint.Port <- port
