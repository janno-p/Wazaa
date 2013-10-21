module Wazaa.Client

open System
open System.Net
open System.Net.Sockets
open System.Text

type SearchFileArgs = { FileName:string; IPAddress:string; Port:int }

let DefaultTimeToLive = 5

let SearchFile (peers:seq<IPEndPoint>) (args:Map<string,string>) =
    peers |> Seq.map (fun peer -> async {
        use client = new TcpClient()
        client.Connect(peer)
        use stream = client.GetStream()
        let encodedFileName = WebUtility.UrlEncode(args.["filename"])
        let message = (sprintf "GET /searchfile?name=%s&sendip=%s&sendport=%s&ttl=%d HTTP/1.0\r\n\r\n" encodedFileName args.["sendip"] args.["sendport"] DefaultTimeToLive)
        let buffer = Encoding.ASCII.GetBytes(message)
        stream.Write(buffer, 0, buffer.Length)
        stream.Close()
    }) |> Async.Parallel

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

let FoundFile (peer:IPEndPoint) (files:seq<string>) =
    async {
        use client = new TcpClient()
        client.Connect(peer)

        let content = new StringBuilder()
        content.AppendFormat(@"{ ""id"": ""{0}"",", String.Empty).AppendLine()
               .AppendLine(@"  ""files"":").AppendLine()
               .AppendLine(@"  [") |> ignore
        files |> Seq.iter (fun x ->
            content.AppendFormat(@"    {""ip"":""{0}"", ""port"":""{1}"", ""name"":""{2}""}", "", "", x) |> ignore
            if not (x.Equals(Seq.last files)) then
                content.Append(",") |> ignore
            content.AppendLine() |> ignore
        )
        content.AppendLine("  ]")
               .AppendLine("}") |> ignore

        use stream = client.GetStream()
        let header = (sprintf "POST /foundfile HTTP/1.0\r\nContent-Type: application/json; charset=UTF-8\r\nContent-Length: %d\r\nServer: Wazaa/0.0.1\r\n\r\n" content.Length)
        let headerBuffer = Encoding.ASCII.GetBytes(header)
        stream.Write(headerBuffer, 0, headerBuffer.Length)
        let contentBuffer = Encoding.UTF8.GetBytes(content.ToString())
        stream.Write(contentBuffer, 0, contentBuffer.Length)
        stream.Close()
    } |> Async.Start
