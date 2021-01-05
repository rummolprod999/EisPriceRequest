namespace ParserFsharp

module S =
    type T =
        {
          TmpP : string
          Pref : string
          ConS : string
        }

    type FtpUser =
        {
            User : string
            Pass : string
        }
    let mutable argTuple = Argument.Nan
    let mutable logFile = ""
    let mutable Settings = {
        TmpP = ""
        Pref = ""
        ConS = ""
    }
    let F44 = {
        User = "free"
        Pass = "free"
    }
