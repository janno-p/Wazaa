module Wazaa.Server

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Text
open System.Text.RegularExpressions
open Wazaa.Logger

let headers = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK\r\nContent-Type: text/plain; charset=UTF-8\r\nContent-Length: 1\r\nServer: Wazaa/0.0.1\r\n\r\n")
let content = Encoding.UTF8.GetBytes("0")

let pattern = @"^(?<method>GET|POST)\s+\/?(?<path>.*?)(\s+HTTP\/1\.[01])$"

let DefaultPortNumber = 2345

let ParsePair (c:char) (pair:string) =
    let breakAt = pair.IndexOf(c)
    if breakAt < 0 then
        (WebUtility.UrlDecode(pair), "")
    else
        (WebUtility.UrlDecode(pair.Substring(0, breakAt)), WebUtility.UrlDecode(pair.Substring(breakAt + 1)))

let ParseQueryParams (query:string) =
    if String.IsNullOrEmpty(query) then
        Map.empty
    else
        query.Split('&') |> Seq.map (ParsePair '=') |> Map.ofSeq

let (|Path|_|) request =
    let input =
        match request with
            | null -> ""
            | _ -> request
    let m = Regex.Match(input, pattern, RegexOptions.IgnoreCase)
    if m.Success then
        let (path, query) = ParsePair '?' m.Groups.["path"].Value
        let queryParams = ParseQueryParams query
        Some (m.Groups.["method"].Value.ToUpper(), path, queryParams)
    else None

let RespondFiles (directory:DirectoryInfo) (param:SearchFileParams) =
    let files = directory.EnumerateFiles()
                |> Seq.filter (fun x -> x.Name.Contains(param.FileName))
                |> Seq.map (fun x -> x.Name)
                |> Seq.toList
    if not (Seq.isEmpty files) then
        Client.FoundFile param.EndPoint files

let ForwardSearchRequest (param:SearchFileParams) =
    param.TimeToLive <- param.TimeToLive - 1
    if not (param.NoAsk |> List.exists (fun x -> x.Equals(Config.LocalEndPoint.Address))) then
        param.NoAsk <- List.append param.NoAsk [Config.LocalEndPoint.Address]
    let peers = Config.KnownPeers
                |> Seq.filter (fun x -> not (param.NoAsk |> List.exists (fun y -> y.Equals(x.Address.ToString()))))
                |> Seq.toList
    Client.SearchFile peers param

let HandleSearchFile (param:SearchFileParams) = async {
    match param.IsValid() with
    | true ->
        let directory = new DirectoryInfo(Config.SharedFolderPath)
        if directory.Exists then
            RespondFiles directory param
        if param.TimeToLive > 1 then
            ForwardSearchRequest param
    | false -> ()
}

let ServeClient (client:TcpClient) = async {
    use stream = client.GetStream()
    use reader = new StreamReader(stream)
    let request = reader.ReadLine()
    GlobalLogger.Info (sprintf "#IN# (%O) %s" client.Client.RemoteEndPoint request)
    match request with
        | Path ("GET", "getfile", query) -> GlobalLogger.Info (sprintf "TODO: Get File: %O" query)
        | Path ("GET", "searchfile", query) -> do! HandleSearchFile (SearchFileParams.Parse(query))
        | Path ("POST", "foundfile", query) -> GlobalLogger.Info (sprintf "TODO: Found File: %O" query)
        | _ -> GlobalLogger.Warning "not ok"
    do! stream.AsyncWrite(headers)
    do! stream.AsyncWrite(content)
    stream.Close()
}

let RunServerAsync (server:TcpListener) = async {
    let isCancelled = ref false
    use! c = Async.OnCancel(fun () ->
                isCancelled := true
                server.Stop()
                GlobalLogger.Warning "Server stopped."
                )
    try
        while true do
            let client = server.AcceptTcpClient()
            Async.Start(ServeClient client)
    with
    | :? SocketException as e ->
        match !isCancelled with
        | true -> ()
        | _ -> GlobalLogger.Error (sprintf "Error occured in listener: %O" e)
}

let HttpServer (localEndPoint:IPEndPoint) : TcpListener =
    let server = new TcpListener(localEndPoint)
    server.Start()
    GlobalLogger.Info (sprintf "Server started on host %O." localEndPoint.Address)
    GlobalLogger.Info (sprintf "Listening incoming connections on port %d..." localEndPoint.Port)
    server
