module Wazaa.Server

open FSharp.Data.Json
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Text
open System.Text.RegularExpressions
open Wazaa.Client
open Wazaa.Config
open Wazaa.Logger

let headers = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK\r\nContent-Type: text/plain; charset=UTF-8\r\nContent-Length: 1\r\nServer: Wazaa/0.0.1\r\n\r\n")
let content = Encoding.UTF8.GetBytes("0")

let pattern = @"^(?<method>GET|POST)\s+\/?(?<path>.*?)(\s+HTTP\/1\.[01])$"

let DefaultPortNumber = 2345

type IMessageListener =
    abstract member FilesFound : (string * string * string) list -> unit

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

let RespondFiles (directory : DirectoryInfo) (args : SearchFileArgs) =
    let files = directory.EnumerateFiles()
                |> Seq.map (fun file -> file.Name)
                |> Seq.filter (fun fileName -> fileName.Contains(args.Name))
                |> Seq.toList
    match files with
    | [] -> ()
    | files -> Client.FoundFile args files

let ForwardSearchRequest (args : SearchFileArgs) =
    let address = Config.LocalEndPoint.Address.ToString()
    let noAskList =
        match args.NoAskList |> Seq.exists (fun x -> x.Equals(address)) with
        | true -> args.NoAskList
        | false -> args.NoAskList |> Seq.append [address]
    let peers = Config.KnownPeers
                |> Seq.filter (fun peer -> not (noAskList |> Seq.exists (fun adr -> adr.Equals(peer.Address.ToString()))))
                |> Seq.filter (fun peer -> not (peer.Address = Config.LocalEndPoint.Address && peer.Port = Config.LocalEndPoint.Port))
                |> Seq.toList
    Client.SearchFile peers { args with
                                TimeToLive = args.TimeToLive - 1
                                NoAsk = noAskList |> String.concat "_" }

let HandleSearchFile (args : SearchFileArgs) =
    async { match args.AreValid() with
            | true ->
                let directory = new DirectoryInfo(Config.SharedFolderPath)
                if directory.Exists then
                    RespondFiles directory args
                if args.TimeToLive > 1 then
                    ForwardSearchRequest args
            | false -> () }

let rec ReadHeader (reader : StreamReader) =
    let line = reader.ReadLine()
    match line with
    | "" -> []
    | x -> let values = ReadHeader reader
           match x.Split([| ':' |], 2) with
           | [| key; value |] -> (key.ToLower().Trim(), value.Trim()) :: values
           | _ -> values

let (?) (header : (string * string) list) key =
    match header |> Seq.tryFind (fun x -> (fst x) = key) with
    | Some (_, value) -> value
    | _ -> ""

let ReadContent (reader : StreamReader) length (encoding : Encoding) =
    seq { match length with
          | length when length > 0 ->
              let buffer = Array.create length 0uy
              match JsonValue.Load(reader) with
              | JsonValue.Object o ->
                  if o.ContainsKey("files") then
                      match o.["files"] with
                      | JsonValue.Array arr ->
                          for item in arr do
                              match item with
                              | JsonValue.Object file ->
                                  if file.ContainsKey("ip") && file.ContainsKey("port") && file.ContainsKey("name") then
                                      let ip = match file.["ip"] with | JsonValue.String s -> s | _ -> ""
                                      let port = match file.["port"] with | JsonValue.String s -> s | _ -> ""
                                      let name = match file.["name"] with | JsonValue.String s -> s | _ -> ""
                                      yield (ip, port, name)
                              | _ -> ()
                      | _ -> ()
              | _ -> ()
          | _ -> () }

let ReadFileList (reader : StreamReader) (listener : IMessageListener) =
    let header = ReadHeader reader
    let charset = header?``content-type``.Split([| ';' |])
                  |> Seq.map (fun x -> x.Trim().Split([| '=' |], 2))
                  |> Seq.tryFind (fun x -> x.[0].Trim().ToLower() = "charset")
    let encoding = match charset with
                   | Some [| _; enc |] -> Encoding.GetEncoding(enc)
                   | _ -> Encoding.UTF8
    ReadContent reader (ConvertToInt header?``content-length``) encoding
    |> Seq.toList
    |> listener.FilesFound

let ServeClient (client : TcpClient) listener =
    async { use stream = client.GetStream()
            use reader = new StreamReader(stream)
            let request = reader.ReadLine()
            GlobalLogger.Info (sprintf "#IN# (%O) %s" client.Client.RemoteEndPoint request)
            match request with
            | Path ("GET", "getfile", query) -> GlobalLogger.Info (sprintf "TODO: Get File: %O" query)
            | Path ("GET", "searchfile", query) -> do! HandleSearchFile (SearchFileArgs.Parse query)
            | Path ("POST", "foundfile", query) -> ReadFileList reader listener
            | _ -> GlobalLogger.Warning "not ok"
            stream.Write(headers, 0, headers.Length)
            stream.Write(content, 0, content.Length)
            stream.Close() }

let RunServerAsync listener (server : TcpListener) =
    async { let isCancelled = ref false
            use! c = Async.OnCancel(fun () ->
                        isCancelled := true
                        server.Stop()
                        GlobalLogger.Warning "Server stopped."
                        )
            try
                while true do
                    let client = server.AcceptTcpClient()
                    Async.Start(ServeClient client listener)
            with
            | :? SocketException as e ->
                match !isCancelled with
                | true -> ()
                | _ -> GlobalLogger.Error (sprintf "Error occured in listener: %O" e) }

let HttpServer (localEndPoint : IPEndPoint) =
    let server = new TcpListener(localEndPoint)
    server.Start()
    GlobalLogger.Info (sprintf "Server started on host %O." localEndPoint.Address)
    GlobalLogger.Info (sprintf "Listening incoming connections on port %d..." localEndPoint.Port)
    server
