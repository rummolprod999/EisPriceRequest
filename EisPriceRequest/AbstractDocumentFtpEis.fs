namespace ParserFsharp

[<AbstractClass>]
type AbstractDocumentFtpEis() =
    static member val Add : int = 0 with get, set
    static member val Upd : int = 0 with get, set

    member __.GetXml(s : string) =
        let xmlt = s.Split('/')
        let t = xmlt.Length
        if t >= 2 then
            let ret = sprintf "%s/%s" xmlt.[t - 2] xmlt.[t - 1]
            ret
        else ""
