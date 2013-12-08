module Wazaa.Logger

type ILogger = 
    abstract member Info : string -> unit
    abstract member Warning : string -> unit
    abstract member Error : string -> unit

let mutable GlobalLogger =
    { new ILogger with
        member this.Info message =
            lock this (fun () -> printfn "%s" message)
        member this.Warning message =
            lock this (fun () -> printfn "%s" message)
        member this.Error message =
            lock this (fun () -> printfn "%s" message) }
