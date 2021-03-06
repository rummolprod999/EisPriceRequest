namespace ParserFsharp
open System.Text.RegularExpressions

module rec RegexpExt =

    let (|RegexMatch1|_|) (pattern : string) (input : string) =
        let result = Regex.Match(input, pattern)
        if result.Success then
            match (List.tail [ for g in result.Groups -> g.Value ]) with
            | fst :: [] -> Some(fst)
            | _ -> None
        else None