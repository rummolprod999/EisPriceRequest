namespace ParserFsharp
open System
open System.Linq
open System.Collections.Generic
open System.IO
open Microsoft.EntityFrameworkCore
open System.Text
open System.Xml
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type ParserPcontr223(dir : string) =
      inherit AbstractParserFtpEis()
      interface Iparser with

            override __.Parsing() = ()