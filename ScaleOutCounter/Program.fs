// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System
open NetMQ
open System.Threading.Tasks
open System.Text
open Common
open Source
open Worker
open Sink

    
[<EntryPoint>]
let main argv = 
    let ctx = NetMQContext.Create()
    let p:ConnectionParams = { 
        ctx=ctx;
        sourceEndpoint = "tcp://127.0.0.1:9897";
        readySignalEndpoint = "tcp://127.0.0.1:9890";
        sinkEndpoint = "tcp://127.0.0.1:9091"
    }

    let cancel = new System.Threading.CancellationTokenSource()

    let sink = (new Sink(p)).Run()

    let workers = 
        [1..8]
        |> Seq.map (fun i -> (new Worker(i.ToString(), p, 0)).Run())

    Task.Delay(1000).Wait();

    let source = (new Source(argv.[0], p, 6)).Run()
    
    
    workers
    |> Seq.append [source]
    |> Array.ofSeq
    |> Task.WaitAll    

    0 // return an integer exit code
