module Common
open NetMQ
open Newtonsoft.Json
open System

type ConnectionParams = {ctx:NetMQContext; 
    sourceEndpoint:string; 
    readySignalEndpoint:string; 
    sinkEndpoint:string}

type Message = 
    | Prepare 
    | Line of Sequence:int*Body:string
    | Complete of Total:int
    | LineResult of Sequence:int*Map<string, int>
    | Ack of id:string
    | Empty


let log source msg=
   printfn "[%s] %s" source msg

///Helper to push a message to a socket
let pushMessage (message:Message) (socket:NetMQ.IOutgoingSocket) = 
    match message with
    | Prepare -> socket.Send "0"
    | Line (s, b) -> 
        socket
            .SendMore("1")
            .SendMore(s.ToString())
            .Send(b)
    | Complete count -> 
        socket
            .SendMore("2")
            .Send(count.ToString())
    | LineResult (s, m) -> 
        let serialized = JsonConvert.SerializeObject m
        socket
            .SendMore("3")
            .SendMore(s.ToString())
            .Send serialized
    | Ack id -> socket.SendMore("4").Send(id)
    | Empty -> raise(new InvalidOperationException("Can't send empty message"))

//Helper to receive a message
let receive (socket:NetMQ.IReceivingSocket) : Message = 
    try
        let preamble = socket.ReceiveString()
    

        match preamble with
        | "0" -> Prepare
        | "1" -> 
            let sequence = Int32.Parse(socket.ReceiveString())
            let body = socket.ReceiveString()
            Line (sequence, body)
        | "2" -> let count = Int32.Parse(socket.ReceiveString())
                 Complete count
        | "3" -> let sequence = Int32.Parse(socket.ReceiveString())
                 let str = socket.ReceiveString()           
                 let m = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, int>>(str) 
                 let toMap dictionary = 
                    (dictionary :> seq<_>)
                    |> Seq.map (|KeyValue|)
                    |> Map.ofSeq
                 let wordMap = toMap m
                 LineResult (sequence, wordMap)
        | "4" -> let id = socket.ReceiveString()
                 Ack id
        | null -> Empty
        | _ -> raise (new ArgumentException("Invalid preamble"))


    with 
    | :? NetMQ.AgainException -> Empty
             