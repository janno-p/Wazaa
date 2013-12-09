module Wazaa.Server

open FSharp.Data.Json
open System
open System.Collections.Generic
open System.IO
open System.Net
open System.Net.Sockets
open System.Text
open System.Text.RegularExpressions
open Wazaa.Client
open Wazaa.Config
open Wazaa.Logger

let DefaultPortNumber = 2345

type IMessageListener =
    abstract member FilesFound : FileRecord list -> unit

let RespondFiles (directory : DirectoryInfo) (args : SearchFileArgs) =
    let files = directory.EnumerateFiles()
                |> Seq.map (fun file -> file.Name)
                |> Seq.filter (fun fileName -> fileName.ToLower().Contains(args.Name.ToLower()))
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
    if args.AreValid() then
        let directory = new DirectoryInfo(Config.SharedFolderPath)
        if directory.Exists then
            RespondFiles directory args
        if args.TimeToLive > 1 then
            ForwardSearchRequest args

let ReadContent (stream : Stream) =
    seq {
        match JsonValue.Load(stream) with
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
    }

let ReadFileList (request : HttpListenerRequest) (listener : IMessageListener) =
    ReadContent request.InputStream
    |> Seq.map (fun x -> let adr, port, name = x
                         match (ParseIPAddress adr, ParseUShort port) with
                         | Some adr, Some port -> Some { Name = name; Owner = new IPEndPoint(adr, int port) }
                         | _ -> None)
    |> Seq.choose (fun x -> x)
    |> Seq.toList
    |> listener.FilesFound

let WriteWazaaCode (response : HttpListenerResponse) code =
    let buffer = Encoding.ASCII.GetBytes(code.ToString())
    response.ContentLength64 <- buffer.LongLength
    response.OutputStream.Write(buffer, 0, buffer.Length)

let GetFileInfo (request : HttpListenerRequest) =
    let fileName = request.QueryString?fullname
    match fileName |> String.IsNullOrEmpty with
    | false -> 
        match new FileInfo(SharedFolderPath @@ fileName) with
        | file when file.Exists -> Some file
        | _ -> None
    | _ -> None

let WriteHeader (statusCode : HttpStatusCode) (response : HttpListenerResponse) =
    response.ContentType <- "text/plain"
    response.Headers.Add("Server", "Wazaa/0.0.1")
    response.StatusCode <- int statusCode
    match statusCode with
    | HttpStatusCode.OK -> response.StatusDescription <- "OK"
    | HttpStatusCode.NotFound -> response.StatusDescription <- "Not Found"
    | _ -> ()

let WriteOkHeader = WriteHeader HttpStatusCode.OK
let WriteNotFoundHeader = WriteHeader HttpStatusCode.NotFound

let WriteFileContent (fileInfo : FileInfo) (response : HttpListenerResponse) =
    response |> WriteOkHeader
    response.ContentType <- "application/octet-stream"
    response.ContentLength64 <- fileInfo.Length
    response.Headers.Add("Content-Disposition", sprintf @"inline; filename=""%s""" fileInfo.Name)
    use stream = fileInfo.OpenRead()
    CopyStream stream response.OutputStream

let HttpHandler (request : HttpListenerRequest) (response : HttpListenerResponse) (notifiable : IMessageListener) =
    async {
        try
            GlobalLogger.Info (sprintf "#IN# (%O) %s" request.RemoteEndPoint request.RawUrl)
            match (request.HttpMethod.ToUpper(), request.Url.AbsolutePath) with
            | ("GET", "/getfile") ->
                match GetFileInfo request with
                | Some file -> response |> WriteFileContent file
                | _ ->
                    GlobalLogger.Warning (sprintf "#OUT# (%O) File Not Found" request.RemoteEndPoint)
                    response |> WriteNotFoundHeader
                    WriteWazaaCode response 404
            | ("GET", "/searchfile") ->
                HandleSearchFile (SearchFileArgs.FromQuery request.QueryString)
                response |> WriteOkHeader
                WriteWazaaCode response 0
            | ("POST", "/foundfile") ->
                ReadFileList request notifiable
                response |> WriteOkHeader
                WriteWazaaCode response 0
            | _ ->
                GlobalLogger.Warning (sprintf "#OUT# (%O) Not Found" request.RemoteEndPoint)
                response |> WriteNotFoundHeader
                WriteWazaaCode response 404
            response.OutputStream.Close()
        with
        | e -> GlobalLogger.Error (sprintf "Error occured while serving request: %O" e)
    }

let StartServer (endPoint : IPEndPoint) notifiable =
    let listener = new HttpListener()
    listener.Prefixes.Add (sprintf "http://%s:%d/" (endPoint.Address.ToString()) endPoint.Port)
    listener.Start()
    GlobalLogger.Info (sprintf "Server started on host %O." endPoint.Address)
    GlobalLogger.Info (sprintf "Listening incoming connections on port %d..." endPoint.Port)
    let task = Async.FromBeginEnd(listener.BeginGetContext, listener.EndGetContext)
    async {
        use! c = Async.OnCancel(fun () ->
                                GlobalLogger.Warning "Server stopped."
                                listener.Stop())
        while true do
            let! context = task
            HttpHandler context.Request context.Response notifiable |> Async.Start
    }
