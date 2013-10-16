module Wazaa.Server

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Text
open System.Text.RegularExpressions

let headers = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK\r\nContent-Type: text/plain; charset=UTF-8\r\nContent-Length: 1\r\nServer: Wazaa/0.0.1\r\n\r\n")
let content = Encoding.UTF8.GetBytes("0")

let pattern = @"^(?<method>GET|POST)\s+\/?(?<path>.*?)(\s+HTTP\/1\.[01])$"

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
    let m = Regex.Match(request, pattern, RegexOptions.IgnoreCase)
    if m.Success then
        let (path, query) = ParsePair '?' m.Groups.["path"].Value
        let queryParams = ParseQueryParams query
        Some (m.Groups.["method"].Value.ToUpper(), path, queryParams)
    else None

let serveClient (client:TcpClient) = async {
    use stream = client.GetStream()
    use reader = new StreamReader(stream)
    let request = reader.ReadLine()
    printfn "%s" request
    match request with
        | Path ("GET", "getfile", query) -> printfn "TODO: Get File: %O" query
        | Path ("GET", "searchfile", query) -> printfn "TODO: Search File: %O" query
        | Path ("POST", "foundfile", query) -> printfn "TODO: Found File: %O" query
        | _ -> printfn "not ok"
    do! stream.AsyncWrite(headers)
    do! stream.AsyncWrite(content)
    stream.Close()
}

let HttpServer (ip, port) =
    let server = new TcpListener(IPAddress.Parse(ip), port)
    server.Start()
    while true do
        let client = server.AcceptTcpClient()
        printfn "New client: %O" client.Client.RemoteEndPoint
        Async.Start(serveClient client)
