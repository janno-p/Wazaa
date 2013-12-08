module Wazaa.Client

open System
open System.Net
open System.Net.Sockets
open System.Text
open Wazaa.Config
open Wazaa.Logger

let DefaultTimeToLive = 5

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
    static member Parse (query : Map<string,string>) =
        { Name = ""; SendIP = ""; SendPort = 0us; TimeToLive = 0; Id = ""; NoAsk = "" }

let SendRequest buffer peer =
    use client = new TcpClient()
    try
        client.Connect(peer)
        use stream = client.GetStream()
        stream.Write(buffer, 0, buffer.Length)
        stream.Close()
    with e -> GlobalLogger.Error (sprintf "#OUT# (%O) %s" peer e.Message)

let SearchFile (peers : IPEndPoint list) (args : SearchFileArgs) =
    GlobalLogger.Info (args.ToString())
    let message = (sprintf "GET /searchfile?%s HTTP/1.0\r\n\r\n" (args.ToString()))
    let buffer = Encoding.ASCII.GetBytes(message)
    peers
    |> Seq.map (fun peer -> async {
        GlobalLogger.Info (sprintf "#OUT# (%O) %s" peer message)
        SendRequest buffer peer })
    |> Seq.iter Async.Start
    |> ignore

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
