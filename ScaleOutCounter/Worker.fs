module Worker
open System
open Common
open NetMQ
open System.Text
open System.Text.RegularExpressions
open System.Threading.Tasks

type Worker (workerId:string, connectionParams:ConnectionParams, sourceId:int)= 

    let cleanPunctuation line = 
        Regex.Replace(line, @"[^\w\s]", String.Empty)

    let toLowerCase (line:string) = 
        line.ToLowerInvariant()

    let toWords (line:string) = 
        line.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)


    let processMessage (sequence, line) (sink:NetMQSocket) = 
        let wordMap = 
            line
            |> cleanPunctuation
            |> toLowerCase
            |> toWords
            |> Seq.countBy id
            |> Seq.sort
            |> Map.ofSeq
        let result = LineResult(sequence, wordMap)

        pushMessage result sink
        
        String.Format("Processed line {0}", sequence)
        |> log workerId
    
    let ackSource (signaler:NetMQSocket) =
        log workerId "Acking source"
        pushMessage (Ack workerId) signaler

    let relayCompletionToSink (m:Message) (sink:NetMQSocket) = 
        pushMessage m sink
        "Sent completion to sink"
        |> log workerId
    
    let work () = 
        log workerId "Starting work"
        let ctx = connectionParams.ctx
        
        let signaler = ctx.CreatePushSocket()
        signaler.Connect connectionParams.readySignalEndpoint

        let source = ctx.CreateSubscriberSocket()
        source.Subscribe([||])
        source.Connect connectionParams.sourceEndpoint
        
        let sink = ctx.CreatePushSocket()
        sink.Connect connectionParams.sinkEndpoint

        log workerId "Sockets created"

        let processing = ref true

        while !processing do
            source.Receive() |> ignore //topic
            let message = receive source
            match message with
            | Prepare -> 
                log workerId "Received prepare"
                ackSource signaler
            | Line (s, b) -> 
                log workerId (String.Format("Received line {0}", s))
                processMessage (s, b) sink
            | Complete c as m -> 
                "Received completion" |> log workerId
                relayCompletionToSink m sink
                processing := false
            | _ -> raise (new ArgumentException("Unexpect message type at worker"))

    member this.Run () = 
        Task.Factory.StartNew(work, TaskCreationOptions.LongRunning)