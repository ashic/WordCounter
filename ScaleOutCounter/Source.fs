module Source
open System
open NetMQ
open System.Threading.Tasks
open System.Text
open Common

type Source (filePath:string, connectionParams:ConnectionParams, minRequiredConnections:int) = 
   
    ///Initiates the handshake with clients.
    ///Sends out prepare messages, waits for acks from 
    ///specified minimum clients, resends prepares in case
    ///of timeout.
    ///Once this returns, the required number of clients are
    ///connected and it is safe to start sending real data
    let initHandshake (publisher: NetMQSocket) (waiter:NetMQSocket) = 
        let pushPrepare count (socket:NetMQSocket) = 
            for i=1 to count do
                socket.SendMore([||])
                |> pushMessage Prepare 

        let sendPrepare ()= 
            log "source" "pushing prepare messages"
            pushPrepare 10 publisher
        
        sendPrepare()
        waiter.Options.ReceiveTimeout <- TimeSpan.FromSeconds(2.0)
        let mutable received = Set.empty

        while received.Count < minRequiredConnections do
            
            match receive waiter with
            | Empty -> 
                log "source" "no ack received and quota not met...resending prepares"
                sendPrepare()
            | Ack id ->
                log "source" "received client ack"   
                received <- received.Add id
                log "source" (String.Format("Acks: {0}", received.Count))
            | _ -> raise(new ArgumentException("Unexpected message received at source waiter"))
                  
                
        log "source" "quota met...ready to send data"
    
    ///Pushing lines from file to sockets.
    ///There are n copies of the same line pushed
    ///to n sockets (i.e. one each).
    ///This is inline with the "Brutal Shotgun Massacre" 
    ///brokerless reliability pattern.
    ///A count of distinct lines is pushed.
    let pushMessages (publisher:NetMQSocket) = 
        log "source" "pushing messages"
        let messageCount = ref 0
        System.IO.File.ReadLines filePath
        |> Seq.iter (fun line ->
            messageCount := !messageCount + 1
            pushMessage (Line(!messageCount, line)) (publisher.SendMore([||]))
        )
        log "source" "messages pushed"
        !messageCount    
            

    ///Pushes completions to clients to signal end of input
    let pushCompletions (socket:NetMQSocket) (numOfCompletions:int) (totalMessageCount:int) = 
        log "source" "pushing completions"
        for i=1 to numOfCompletions do
            pushMessage (Complete totalMessageCount) (socket.SendMore([||]))
        log "source" "completions pushed"

    ///The workflow
    let work () = 
        log "source" "starting"
        let ctx = connectionParams.ctx
        
        log "source" "creating and binding waiter"
        let waiter = ctx.CreatePullSocket()
        waiter.Bind connectionParams.readySignalEndpoint

        log "source" "creating and binding publishe"
        let publisher = ctx.CreatePublisherSocket()
        publisher.Bind connectionParams.sourceEndpoint
            
        log "source" "initiating handshake"
        initHandshake publisher waiter
        log "source" "required clients ready..disposing waiter"
        waiter.Close()
        waiter.Dispose()
        log "source" "waiter disposed"


        pushMessages publisher    //push lines
        |> pushCompletions publisher 10 //send 10 completions per socket

        log "source" "work done...disposing pushers"
        publisher
        |>fun s -> s.Close(); s.Dispose()
        log "source" "all finished...bye bye"

    ///Starts the source and returns the task
    member this.Run () =
        Task.Factory.StartNew(work, TaskCreationOptions.LongRunning)
