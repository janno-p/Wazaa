module Wazaa.Client

open System
open System.Net
open System.Net.Sockets
open System.Text

type SearchFileArgs = { FileName:string; IPAddress:string; Port:int }

let DefaultTimeToLive = 5

let SearchFile (peers:seq<IPEndPoint>) (args:SearchFileArgs) =
    peers |> Seq.map (fun peer -> async {
        use client = new TcpClient()
        client.Connect(peer)
        use stream = client.GetStream()
        let encodedFileName = WebUtility.UrlEncode(args.FileName)
        let message = (sprintf "GET /searchfile?name=%s&sendip=%s&sendport=%d&ttl=%d HTTP/1.0\r\n\r\n" encodedFileName args.IPAddress args.Port DefaultTimeToLive)
        let buffer = Encoding.ASCII.GetBytes(message)
        stream.Write(buffer, 0, buffer.Length)
        stream.Close()
    }) |> Seq.iter Async.Start

let GetFile (peer:IPEndPoint) (fileName:string) =
    async {
        use client = new TcpClient()
        client.Connect(peer)

        use stream = client.GetStream()
        let encodedFileName = WebUtility.UrlEncode(fileName)
        let message = (sprintf "GET /getfile?fullname=%s HTTP/1.0\r\n\r\n" encodedFileName)
        let buffer = Encoding.ASCII.GetBytes(message)
        stream.Write(buffer, 0, buffer.Length)

        // TODO : Read response content
    } |> Async.Start
