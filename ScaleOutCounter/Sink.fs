module Sink
open Common
open System
open System.Threading.Tasks

type Sink (connectionParams:ConnectionParams) =
    let mutable currentMap = Map.empty
    let mutable linesProcessed = []

    let updateMap (sequence:int) (m:Map<string, int>) = 
        if List.exists ((=) sequence) linesProcessed = false then
            log "sink" "processing new line"
            linesProcessed <- sequence::linesProcessed
            
            m
            |> Map.toSeq
            |> Seq.iter (fun(k,v) ->
                let existing = 
                    if Map.containsKey k currentMap then
                        currentMap.[k]
                    else
                        0
                currentMap <- currentMap.Add(k, existing + v)
            )


    let work () = 
        log "sink" "starting"
        let ctx = connectionParams.ctx
        use socket = ctx.CreatePullSocket()
        socket.Bind  connectionParams.sinkEndpoint
        
        log "sink" "socket bound"

        let processing = ref true

        while !processing do
            match receive socket with
            | Empty -> ()
            | LineResult (s, m) -> updateMap s m
            | Complete total -> 
                if total = linesProcessed.Length then
                    processing := false
            | _ -> raise (new ArgumentException("Only line results are expected at sink"))


        log "sink" "results accumulated"

        currentMap
        |> Map.toSeq
        |> Seq.sort
        |> Seq.iter (fun (k, v) -> printfn "%s: %d" k v)

        log "sink" "all done...bye bye"
    
    member this.Run() = 
        Task.Factory.StartNew(work, TaskCreationOptions.LongRunning)
