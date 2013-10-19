module Wazaa.Logger

type ILogger = 
    abstract member Info : string -> unit
    abstract member Warning : string -> unit
    abstract member Error : string -> unit

type ConsoleLogger() =
    interface ILogger with
        member this.Info message =
            printfn "%s" message
        member this.Warning message =
            printfn "%s" message
        member this.Error message =
            printfn "%s" message
