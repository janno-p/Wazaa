namespace Wazaa
    module Server =
        open System
        open System.IO
        open System.Net
        open System.Net.Sockets
        open System.Text

        let headers = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK\r\nContent-Type: text/plain; charset=UTF-8\r\nContent-Length: 1\r\nServer: Wazaa/0.0.1\r\n\r\n")
        let content = Encoding.ASCII.GetBytes("0")

        let serveClient (client:TcpClient) = async {
            use stream = client.GetStream()
            do! stream.AsyncWrite(headers)
            do! stream.AsyncWrite(content)
            stream.Close()
        }

        let HttpServer (ip, port) =
            let server = new TcpListener(IPAddress.Parse(ip), port)
            server.Start()
            while true do
                let client = server.AcceptTcpClient()
                printfn "New client: %O" (client.Client.RemoteEndPoint :?> IPEndPoint)
                Async.Start(serveClient client)
