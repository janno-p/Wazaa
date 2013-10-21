namespace Wazaa

open System
open System.Net
open System.Text

type SearchFileParams () =
    // String to search from file name (name)
    member val FileName = "" with get, set

    // IP and port of the client who initiated the request (sendip, sendport)
    member val EndPoint = new IPEndPoint(0L, 0) with get, set

    //How many recursive request to make - every step decreases by one (ttl)
    member val TimeToLive = 0 with get, set

    // Optional id for the request (id)
    member val RequestId = "" with get, set

    // Optional list of client who should be excluded from recursive search, separated with underscore (noask)
    member val NoAsk = []:list<IPAddress> with get, set

    member this.IsValid () : bool =
        match String.IsNullOrEmpty(this.FileName) with
        | true -> false
        | false ->
            match this.EndPoint.Address.ToString() with
            | "0.0.0.0" -> false
            | _ ->
                match this.EndPoint.Port with
                | x when x > 0 -> true
                | _ -> false

    override this.ToString() : string =
        let sb = new StringBuilder()
        sb.AppendFormat("name={0}", WebUtility.UrlEncode(this.FileName)) |> ignore
        sb.AppendFormat("&sendip={0}", this.EndPoint.Address) |> ignore
        sb.AppendFormat("&sendport={0}", this.EndPoint.Port) |> ignore
        sb.AppendFormat("&ttl={0}", this.TimeToLive) |> ignore
        if not (String.IsNullOrEmpty(this.RequestId)) then
            sb.AppendFormat("&id={0}", this.RequestId) |> ignore
        if not (Seq.isEmpty this.NoAsk) then
            sb.AppendFormat("&noask={0}", String.Join("_", this.NoAsk)) |> ignore
        sb.ToString()

    static member Parse (query:Map<string,string>) : SearchFileParams =
        let p = new SearchFileParams()
        query
        |> Map.iter (fun key value ->
            match key.ToLower() with
            | "name" -> p.FileName <- value
            | "sendip" ->
                let mutable address = null:IPAddress
                match IPAddress.TryParse(value, &address) with
                | true -> p.EndPoint.Address <- address
                | false -> ()
            | "sendport" ->
                let mutable port = 0
                match Int32.TryParse(value, &port) with
                | true -> p.EndPoint.Port <- port
                | false -> ()
            | "ttl" ->
                let mutable ttl = 0
                match Int32.TryParse(value, &ttl) with
                | true -> p.TimeToLive <- ttl
                | false -> ()
            | "id" -> p.RequestId <- value
            | "noask" -> p.NoAsk <- value.Split('_')
                                    |> Seq.map (fun x ->
                                        let mutable address = null:IPAddress
                                        match IPAddress.TryParse(x, &address) with
                                        | true -> address
                                        | false -> null:IPAddress)
                                    |> Seq.filter (fun x -> x <> null)
                                    |> Seq.toList
            | _ -> ()
        )
        p
