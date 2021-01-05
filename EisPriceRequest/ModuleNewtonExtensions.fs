namespace ParserFsharp
open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System.Text.RegularExpressions
open System.Collections.Generic

module NewtonExt =

    let (|RegexMatch1|_|) (pattern : string) (input : string) =
        let result = Regex.Match(input, pattern)
        if result.Success then
            match (List.tail [ for g in result.Groups -> g.Value ]) with
            | fst :: [] -> Some(fst)
            | _ -> None
        else None

    let inline GetStringFromJtoken (x : ^a) (s : string) =
            match (^a : (member SelectToken : string -> JToken) (x, s)) with
            | null -> ""
            | r -> ((string) r).Trim()

    let inline GetIntFromJtoken (x : ^a) (s : string) =
            match (^a : (member SelectToken : string -> JToken) (x, s)) with
            | null -> 0
            | r -> ((int) r)

    let inline GetDecimalFromJtoken (x : ^a) (s : string) =
            match (^a : (member SelectToken : string -> JToken) (x, s)) with
            | null -> 0m
            | r -> ((decimal) r)

    let inline GetDateTimeFromJtoken (x : ^a) (s : string) =
            match (^a : (member SelectToken : string -> JToken) (x, s)) with
            | null -> DateTime.MinValue
            | r -> DateTime.Parse((string) r)

    let inline GetDateTimeStringFromJtoken (x : ^a) (s : string) =
            match (^a : (member SelectToken : string -> JToken) (x, s)) with
            | null -> ""
            | rr when (string) rr = "null" -> ""
            | r -> match JsonConvert.SerializeObject(r) with
                   | null -> ""
                   | t -> t.Trim('"')


    type JToken with
        member this.StDString (path : string) (err : string) =
            match this.SelectToken(path) with
            | null -> Error err
            | x -> Success(((string) x).Trim())

        member this.StDInt (path : string) (err : string) =
            match this.SelectToken(path) with
            | null -> Error err
            | x -> Success((int) x)

        member this.GetElements(path : string) =
            let els = List<JToken>()
            match this.SelectToken(path) with
            | null -> ()
            | x when x.Type = JTokenType.Object -> els.Add(x)
            | x when x.Type = JTokenType.Array -> els.AddRange(x)
            | _ -> ()
            els