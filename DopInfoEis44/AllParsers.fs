namespace ParserFsharp

module Parsers =

    let parserExec (p : Iparser) = p.Parsing()

    let parserRequest (dir : string) =
        Logging.Log.logger "Начало парсинга"
        try
            parserExec (ParserAddInfo44(dir))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger $"Добавили запросов %d{AbstractDocumentFtpEis.Add}"
        Logging.Log.logger $"Обновили запросов %d{AbstractDocumentFtpEis.Upd}"