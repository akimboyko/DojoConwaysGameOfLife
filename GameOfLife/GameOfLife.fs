﻿namespace GameOfLife

module Domain =
    open System
    open System.Collections.Immutable
    open System.Text
    open Akka.Actor
    open Akka.Configuration
    open Akka.FSharp

    type Coordinates = int * int

    type CellStatus =
    | Occupied
    | Unknown

    type Command =
    | Spawn of Coordinates
    | SpawnCompleted

    type Message =
    | AggregationStarted of int
    | Neighborhood of Coordinates * CellStatus
    | AggregationCompleted

    type Event =
    | Generation of int
    | LivingCell of Coordinates

    let poisonPill = PoisonPill.Instance

    let cellActorCont = 
        let cont (mailbox : Actor<Command>) =
            let rec loop() =
                actor { 
                    let! command = mailbox.Receive()
                    let sender = mailbox.Sender()
                    let self = mailbox.Self
                    let parent = mailbox.Context.Parent

                    match command with
                    | Spawn(xy) ->
                        let (x, y) = xy 
                        sender.Tell(Neighborhood((x, y), Occupied), parent)
                        sender.Tell(Neighborhood((x + 1, y + 1), Unknown), parent)
                        sender.Tell(Neighborhood((x + 1, y + 0), Unknown), parent)
                        sender.Tell(Neighborhood((x + 1, y - 1), Unknown), parent)
                        sender.Tell(Neighborhood((x + 0, y - 1), Unknown), parent)
                        sender.Tell(Neighborhood((x - 1, y - 1), Unknown), parent)
                        sender.Tell(Neighborhood((x - 1, y + 0), Unknown), parent)
                        sender.Tell(Neighborhood((x - 1, y + 1), Unknown), parent)
                        sender.Tell(Neighborhood((x + 0, y + 1), Unknown), parent)
                            
                        self <! poisonPill
                    
                    | _ -> failwith "Unsupported command"
                }

            loop()

        cont

    let aggregateActorCont = 
        let cont (mailbox : Actor<Message>) =
            let aggregate currentStatus status =
                if status = Occupied || currentStatus = Occupied then Occupied
                else Unknown

            let conditionForNewCell n status =
                (n = 2 && status = Occupied) || n = 3

            let rec loop n position currentStatus =
                actor { 
                    let! message = mailbox.Receive()
                    let sender = mailbox.Sender()
                    let self = mailbox.Self
                    let parentCoordinator = mailbox.Context.Parent

                    match message with
                    | AggregationStarted(_) -> failwith "Unsupported message"
                    | Neighborhood(xy, status) -> 
                        let n =
                            match status with
                            | Occupied -> n
                            | Unknown  -> n + 1

                        let status = aggregate currentStatus status 
                        let position = Some(xy)
                             
                        return! loop n position status
                    | AggregationCompleted ->
                        match position with
                        | Some(xy) when conditionForNewCell n currentStatus ->
                            sender <! Spawn (xy)
                        | _ -> ()

                        parentCoordinator <! AggregationCompleted
                        self <! poisonPill
                }

            loop 0 None Unknown

        cont

    type ActorType =
    | Cell
    | Neighbor

    let private name entityType xy =
        match xy with
        | (x, y) -> sprintf "%A:(%d,%d)" entityType x y

    let coordinatorActorCont = 
        let cont (mailbox : Actor<Command>) =
            let emptyDict = ImmutableDictionary.Empty

            let rec cellsLoop nGeneration (cells:IImmutableDictionary<Coordinates, ActorRef>) =
                actor { 
                    let! command = mailbox.Receive()
                    let sender = mailbox.Sender()
                    let self = mailbox.Self
                    let log = mailbox.Context.System.Log

                    match command with
                    | Spawn(xy) -> 
                        let cells =
                            if cells.ContainsKey xy then
                                cells
                            else
                                let cellName = name Cell xy
                                let cellRef = spawn mailbox cellName <| cellActorCont

                                cells.Add(xy, cellRef) 

                        log.Debug("spawn {0}!", xy)

                        return! cellsLoop nGeneration cells
                    | SpawnCompleted -> 
                        sender <! AggregationStarted(9 * cells.Count)
                        let eventStream = mailbox.Context.System.EventStream

                        pubblish (Generation nGeneration) eventStream

                        for xy in cells.Keys do
                            let actorRef = cells.[xy]
                            actorRef.Tell(Spawn(xy), sender)
                            pubblish (LivingCell xy) eventStream

                        log.Debug("spawn {0} completed!", nGeneration)

                        return! cellsLoop (nGeneration + 1) emptyDict
                }

            cellsLoop 0 emptyDict

        cont

    let collectorActorCont = 
        let cont (mailbox : Actor<Message>) =
            let emptyDict = ImmutableDictionary.Empty

            let rec collectLoop n (neighborhoods:IImmutableDictionary<Coordinates, ActorRef>) replyTo =
                actor { 
                    let! message = mailbox.Receive()
                    let sender = mailbox.Sender()
                    let self = mailbox.Self
                    let log = mailbox.Context.System.Log

                    match message with
                    | AggregationStarted(n) ->
                        log.Debug("aggregation {0}!", n)

                        if n = 0 then // end of game
                            sender <! SpawnCompleted
                            sender <! poisonPill
                            self   <! poisonPill
                            log.Debug("life converge to 0")
                        else
                            return! collectLoop n emptyDict sender
                    | Neighborhood(xy, state) ->
                        let (neighborhoods, actorRef) = 
                            let (success, actorRef) = neighborhoods.TryGetValue xy

                            if success then
                                (neighborhoods, actorRef)
                            else 
                                let neighborhoodName = name Neighbor xy
                                let neighborhoodRef = spawn mailbox neighborhoodName <| aggregateActorCont

                                (neighborhoods.Add(xy, neighborhoodRef), neighborhoodRef)
                                
                        actorRef <! message

                        let description = 
                            match state with 
                            | Occupied -> "occupied"
                            | Unknown  -> "unknown"

                        log.Debug("neighborhood {0}-{1}!", xy, description)

                        if n = 1 then
                            for actorRef in neighborhoods.Values do
                                actorRef.Tell(AggregationCompleted, replyTo)

                            let children = neighborhoods.Count
                            return! aggregationCompletedLoop children replyTo
                        else
                            return! collectLoop (n - 1) neighborhoods replyTo
                    | AggregationCompleted -> failwith "Unsupported message"
                }
            and aggregationCompletedLoop n replyTo =
                actor { 
                    let! message = mailbox.Receive()
                    let self = mailbox.Self
                    let log = mailbox.Context.System.Log

                    match message with
                    | AggregationStarted(_)
                    | Neighborhood(_)       ->  failwith "Unsupported message"
                    | AggregationCompleted  ->
                        log.Debug("aggregationCompleted {0}!", n)
                        if n <= 1 then
                            replyTo <! SpawnCompleted
                            return! collectLoop 0 emptyDict self
                        else
                            return! aggregationCompletedLoop (n - 1) replyTo
                }

            collectLoop 0 emptyDict mailbox.Self

        cont

    let generationDisplayActorCont (output:StringBuilder) =
        let cont (mailbox : Actor<Event>) =
            let composeOutput nGeneration cells (output:StringBuilder) =
                output.AppendLine(sprintf "Generation %d" nGeneration) |> ignore

                let log = mailbox.Context.System.Log

                if cells |> Seq.isEmpty then
                    ignore
                else
                    let firstCell = Seq.head cells

                    let (minX, minY) = 
                        cells
                        |> Seq.skip 1
                        |> Seq.fold (fun (minX, minY) (x, y) -> 
                                    (min x minX, min y minY))
                                    firstCell

                    let (maxX, maxY) = 
                        cells
                        |> Seq.skip 1
                        |> Seq.fold (fun (maxX, maxY) (x, y) -> 
                                    (max x maxX, max y maxY))
                                    firstCell

                    let (width, height) = 
                        (maxX - minX, maxY - minY)

                    let printable = 
                        seq { minY .. maxY }
                        |> Seq.map(fun _ -> seq { minX .. maxX } 
                                            |> Seq.map(fun _ -> ' ')
                                            |> Seq.toArray)
                        |> Seq.toArray
    
                    for (x, y) in cells do
                        printable.[y - minY].[x - minX] <- '#'

                    for line in printable do
                        output.AppendLine(new String(line)) |> ignore

                    log.Debug(output.ToString())

                    ignore

            let rec generationLoop nGeneration cells =
                actor { 
                    let! message = mailbox.Receive()
                    let sender = mailbox.Sender()
                    let self = mailbox.Self

                    match message with
                    | Generation(nGeneration) ->
                        composeOutput nGeneration cells output |> ignore
                        return! generationLoop nGeneration ImmutableHashSet.Empty
                    | LivingCell(xy) ->
                        return! generationLoop nGeneration (cells.Add(xy))
                }
            
            generationLoop 0 ImmutableHashSet.Empty

        cont