open System
open System.IO
open System.Text.RegularExpressions

let counter (filePath:string) = 
    
    let cleanPunctuation line = 
        Regex.Replace(line, @"[^\w\s]", String.Empty)

    let toLowerCase (line:string) = 
        line.ToLowerInvariant()

    let toWords (line:string) = 
        line.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)

    filePath
    |> File.ReadLines
    |> Seq.map cleanPunctuation
    |> Seq.map toLowerCase
    |> Seq.collect toWords
    |> Seq.countBy id
    |> Seq.sort


[<EntryPoint>]
let main argv = 
    argv.[0]
    |>counter
    |> Seq.iter (fun (k, v) -> printfn "%s: %d" k v)

    0 // return an integer exit code
