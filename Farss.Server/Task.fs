module Task

open System.Threading.Tasks

let map (f: 'a -> 'b) (t: Task<_>) = task {
    let! result = t
    return f result
}


let bind (f: 'a -> Task) (t: Task<'a>) =
    task {
        let! result = t
        do! f result
        return ()
    }
    
let bindR (f: 'a -> Task<'b>) (t: Task<'a>) =
    task {
        let! result = t
        return! f result
    }

let traverse (tasks: Task<_> list) = 
    let rec inner tasks acc = task {
            match tasks with
            | [] -> 
                return acc
            | head::tail ->
                let! r = head
                let acc = r::acc
                return! inner tail acc
        }
    inner tasks []

let ignore (t: Task<_>) = task {
    let! _ = t
    return ()
}