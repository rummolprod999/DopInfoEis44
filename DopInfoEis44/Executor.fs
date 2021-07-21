namespace ParserFsharp
open System

module Executor =
    let arguments = "addinfo last, addinfo curr, addinfo prev"

    let parserArgs = function
                     | [| "addinfo"; "last" |] -> S.argTuple <- Argument.DopInfo44("last")
                     | [| "addinfo"; "curr" |] -> S.argTuple <- Argument.DopInfo44("curr")
                     | [| "addinfo"; "prev" |] -> S.argTuple <- Argument.DopInfo44("prev")
                     | _ -> printf $"Bad arguments, use %s{arguments}"
                            Environment.Exit(1)
    let parser = function
                 | DopInfo44 d ->
                     Parsers.parserRequest d
                 | _ -> ()