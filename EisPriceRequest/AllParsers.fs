namespace ParserFsharp

module Parsers =

    let parserExec (p : Iparser) = p.Parsing()

    let parserRequest (dir : string) =
        Logging.Log.logger "Начало парсинга"
        try
            parserExec (ParserPriceRequest44(dir))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger (sprintf "Добавили запросов %d" AbstractDocumentFtpEis.Add)
        Logging.Log.logger (sprintf "Обновили запросов %d" AbstractDocumentFtpEis.Upd)