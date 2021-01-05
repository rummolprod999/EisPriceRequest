namespace ParserFsharp
open System

module Executor =
    let arguments = "peq44 last, req44 curr, req44 prev"

    let parserArgs = function
                     | [| "peq44"; "last" |] -> S.argTuple <- Argument.Request44("last")
                     | [| "req44"; "curr" |] -> S.argTuple <- Argument.Request44("curr")
                     | [| "req44"; "prev" |] -> S.argTuple <- Argument.Request44("prev")
                     | _ -> printf "Bad arguments, use %s" arguments
                            Environment.Exit(1)
    let parser = function
                 | Request44 d ->
                     Parsers.parserRequest d
                 | _ -> ()