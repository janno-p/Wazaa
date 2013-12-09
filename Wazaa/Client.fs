module Wazaa.Client

open System
open System.Collections.Specialized
open System.IO
open System.Net
open System.Net.Sockets
open System.Text
open Wazaa.Config
open Wazaa.Logger

let DefaultTimeToLive = 5

let (?) (a : NameValueCollection) (b : string) =
    match a.[b] with
    | null -> ""
    | x -> x

type FileRecord = { Name : string
                    Owner : IPEndPoint }


type SearchFileArgs =
    { Name : string
      SendIP : string
      SendPort : uint16
      TimeToLive : int
      Id : string
      NoAsk : string }

    override this.ToString() =
        [ ("name", (WebUtility.UrlEncode this.Name))
          ("sendip", this.SendIP)
          ("sendport", this.SendPort.ToString())
          ("ttl", this.TimeToLive.ToString())
          ("id", this.Id)
          ("noask", this.NoAsk) ]
        |> Seq.choose (fun pair -> match String.IsNullOrEmpty(snd pair) with | true -> None | false -> Some pair)
        |> Seq.map (fun pair -> sprintf "%s=%s" (fst pair) (snd pair))
        |> String.concat "&"

    member this.AreValid() =
        match (String.IsNullOrEmpty(this.Name), ParseIPAddress(this.SendIP), this.SendPort) with
        | (true, _, _) | (_, None, _) | (_, _, 0us) -> false
        | _ -> true

    member this.NoAskList = this.NoAsk.Split('_') |> Seq.choose ParseIPAddress |> Seq.map (fun a -> a.ToString())

    static member FromQuery (query : NameValueCollection) =
        { Name = query?Name
          SendIP = query?SendIP
          SendPort = ConvertToUShort query?sendport
          TimeToLive = ConvertToInt query?ttl
          Id = query?id
          NoAsk = query?noask }


let SendRequest buffer peer =
    use client = new TcpClient()
    try
        client.Connect(peer)
        use stream = client.GetStream()
        stream.Write(buffer, 0, buffer.Length)
        stream.Close()
    with e -> GlobalLogger.Error (sprintf "#OUT# (%O) %s" peer e.Message)

let SearchFile (peers : IPEndPoint list) (args : SearchFileArgs) =
    let message = (sprintf "GET /searchfile?%s HTTP/1.0\r\n\r\n" (args.ToString()))
    let buffer = Encoding.ASCII.GetBytes(message)
    peers
    |> Seq.map (fun peer -> async {
        GlobalLogger.Info (sprintf "#OUT# (%O) %s" peer message)
        SendRequest buffer peer })
    |> Seq.iter Async.Start
    |> ignore

let CopyStream (input : Stream) (output : Stream) =
    let rec InnerCopyStream buffer (input : Stream) (output : Stream) =
        match input.Read(buffer, 0, buffer.Length) with
        | x when x < 1 -> ()
        | x -> output.Write(buffer, 0, x)
               InnerCopyStream buffer input output
    let buffer = Array.create 4096 0uy
    InnerCopyStream buffer input output

let GetFile (record : FileRecord) (fileInfo : FileInfo) =
    async {
        let message = (sprintf "GET /getfile?fullname=%s HTTP/1.0\r\n\r\n" (WebUtility.UrlEncode record.Name))
        GlobalLogger.Info (sprintf "#OUT# (%O) %s" record.Owner message)

        let url = (sprintf "http://%s:%d/getfile?fullname=%s" (record.Owner.Address.ToString()) record.Owner.Port record.Name)
        let request = WebRequest.Create(url)
        request.Method <- "GET"

        use response = request.GetResponse()
        use stream = response.GetResponseStream()

        if fileInfo.Exists then
            fileInfo.Delete()

        use fileStream = fileInfo.OpenWrite()
        CopyStream stream fileStream

        stream.Close()
    } |> Async.Start

let FoundFileContent args files =
    seq {
        let address = LocalEndPoint.Address.ToString()
        let port = LocalEndPoint.Port
        yield sprintf @"{ ""id"":""%s""," args.Id
        yield @"  ""files"":"
        yield "  ["
        yield files |> Seq.map (sprintf @"    {""ip"":""%s"", ""port"":""%d"", ""name"":""%s""}" address port) |> String.concat ("," + Environment.NewLine)
        yield "  ]"
        yield "}" }
    |> String.concat Environment.NewLine

let FoundFile (args : SearchFileArgs) (files : seq<string>) =
    async {
        let peer = match ParseIPAddress(args.SendIP) with
                   | Some adr -> new IPEndPoint(adr, int args.SendPort)
                   | _ -> failwith "Invalid IP address."
        let content = FoundFileContent args files
        let contentBuffer = Encoding.UTF8.GetBytes(content)
        let header = sprintf "POST /foundfile HTTP/1.0\r\nContent-Type: application/json; charset=UTF-8\r\nContent-Length: %d\r\nServer: Wazaa/0.0.1\r\n\r\n" contentBuffer.Length
        let headerBuffer = Encoding.ASCII.GetBytes(header)
        GlobalLogger.Info (sprintf "#OUT# (%O) %s" peer (header + content))
        SendRequest (Array.concat [ headerBuffer; contentBuffer ]) peer }
    |> Async.Start
