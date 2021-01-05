namespace ParserFsharp
open System.Collections.Generic
open System.Linq
open Microsoft.EntityFrameworkCore
open FluentFTP
open System.Threading
open System
open System.IO
open System.IO.Compression
open System.Diagnostics
open System.Text.RegularExpressions

[<AbstractClass>]
type AbstractParserFtpEis() =
    abstract GetRegions : unit -> IEnumerable<Region>
    override this.GetRegions() =
        use context = new RegionContext()
        let regions : IEnumerable<Region> = context.Regions.AsNoTracking().ToList() :> IEnumerable<Region>
        regions

    member __.GetListArrays(pathParse : string, s : S.FtpUser) =
        let mutable arch = List<string * int>()
        let mutable count = 1
        let mutable wh = true
        while wh do
            try
                let ftp = new FtpClient("ftp.zakupki.gov.ru", s.User, s.Pass)
                ftp.SetWorkingDirectory(pathParse)
                let ltmp = ftp.GetListing()
                ltmp |> Seq.iter (fun x -> arch.Add(x.Name, (int) x.Size))
                wh <- false
                if count > 1 then Logging.Log.logger (sprintf "Удалось получить список архивов после попытки %d" count)
                ftp.Disconnect()
            with
                | ex when ex.Message.Contains("Failed to change directory") -> Logging.Log.logger (sprintf "Не смогли найти директорию %s" pathParse)
                                                                               wh <- false
                | e when count > 3 -> Logging.Log.logger (sprintf "Не смогли найти директорию после попытки %d" count)
                                      Logging.Log.logger e
                                      wh <- false
                | _ -> count <- count + 1
                       Thread.Sleep(2000)
            ()
        arch

    member __.GetArch(arch : string, pathParse : string, s : S.FtpUser) =
        let mutable count = 1
        let mutable wh = true
        let fName = String.Format("{0}{1}{2}", S.Settings.TmpP, Path.DirectorySeparatorChar, arch)
        while wh do
            try
                let ftp = new FtpClient("ftp.zakupki.gov.ru", s.User, s.Pass)
                ftp.SetWorkingDirectory(pathParse)
                ftp.DownloadFile(fName, arch) |> ignore
                wh <- false
                if count > 1 then Logging.Log.logger (sprintf "Удалось скачать архив после попытки %d" count)
                ftp.Disconnect()
            with
                | ex when count > 50 -> Logging.Log.logger (sprintf "Не смогли скачать архив после попытки %d" count)
                                        Logging.Log.logger ex
                                        wh <- false
                | _ -> count <- count + 1
                       Thread.Sleep(5000)
        let file = FileInfo(fName)
        file

    member __.Unzipper(file : FileInfo) =
        let rPoint = file.FullName.LastIndexOf('.')
        let dirName = file.FullName.Substring(0, rPoint)
        let dir = Directory.CreateDirectory(dirName)
        try
            ZipFile.ExtractToDirectory(file.FullName, dir.FullName)
            file.Delete()
        with ex -> Logging.Log.logger (sprintf "Не удалось извлечь файл %s %s" file.Name ex.Message)
                   try
                       let proc = new Process()
                       proc.StartInfo <- ProcessStartInfo("unzip", String.Format("-B {0} -d {1}", file.FullName, dir.Name))
                       proc.Start() |> ignore
                       proc.WaitForExit()
                       Logging.Log.logger (sprintf "Извлекли файл альтернативным методом %s" file.Name)
                       ()
                   with e -> Logging.Log.logger (sprintf "Не удалось извлечь файл альтернативным методом %s %s" file.Name ex.Message)
        dir

    member __.DeleteBadSymbols(s : string) =
        let regex = Regex(@"ns\d{1,2}:")
        let mutable res = regex.Replace(s, "")
        res <- res.Replace("oos:", "").Replace("", "")
        res
