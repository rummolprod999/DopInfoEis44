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

type ParserAddInfo44(dir : string) =
      inherit AbstractParserFtpEis()
      interface Iparser with

            override __.Parsing() =
                  let regions = __.GetRegions()

                  for r in regions do
                       let mutable arr = List<string * int>()
                       let mutable pathParse = ""
                       match dir with

                       | "last" -> pathParse <- $"/fcs_regions/%s{r.Path}/addinfo"
                                   arr <- __.GetArrLastFromFtp(pathParse, r.Path)
                       | "curr" -> pathParse <- $"/fcs_regions/%s{r.Path}/addinfo/currMonth"
                                   arr <- __.GetArrCurrFromFtp(pathParse, r.Path)
                       | "prev" -> pathParse <- $"/fcs_regions/%s{r.Path}/addinfo/prevMonth"
                                   arr <- __.GetArrPrevFromFtp(pathParse, r.Path)
                       | _ -> ()
                       if arr.Count = 0 then Logging.Log.logger $"empty array size %s{r.Path}"
                       for a in arr do
                             try
                                   __.GetListFA(fst a, pathParse, r)
                             with ex -> Logging.Log.logger ex
                       match dir with
                       | "curr" -> __.WriteArrToTable(arr)
                       | "last" -> __.WriteArrToTableLast(arr)
                       | "prev" -> __.WriteArrToTablePrev(arr)
                       | _ -> ()
      
      member private __.WriteArrToTable(lst : List<string * int>) =
          use context = new ArchiveAddInfo44Context()
          for l in lst do
                let arr = ArchiveAddInfo44()
                arr.Archive <- fst l
                arr.SizeArch <- int <| snd l
                context.Archives.Add(arr) |> ignore
                context.SaveChanges() |> ignore
                ()
          ()
      member private __.GetArrCurrFromFtp(pathParse : string, region : string) =
            let arch = __.GetListArrays(pathParse, S.F44)
            let yearsSeq = seq { 2015..DateTime.Now.Year }
            let _ = seq { for s in yearsSeq do yield $"_%s{region}_%d{s}" }
            let ret = query { for a in arch do
                              where (yearsSeq.Any(fun x -> (fst a).Contains(x.ToString())))
                              select a }
            use context = new ArchiveAddInfo44Context()
            let arr = List<string * int>()
            for r in ret do
                  match (snd r) with
                  | 0 | 22 -> Logging.Log.logger (sprintf "!!!archive is empty %s" <| fst r)
                  | _ -> let res = context.Archives.AsNoTracking() .Where(fun x -> x.Archive = (fst r) && (int x.SizeArch = (snd r) || int x.SizeArch = 0)) .Count()
                         if res = 0 then arr.Add(r)
                         ()
            arr
      member private __.WriteArrToTableLast(lst : List<string * int>) =
          use context = new ArchiveAddInfo44Context()
          for l in lst do
                let arr = ArchiveAddInfo44()
                let arr_last = $"last_%s{fst l}"
                arr.Archive <- arr_last
                arr.SizeArch <- int <| snd l
                context.Archives.Add(arr) |> ignore
                context.SaveChanges() |> ignore
                ()
          ()
      member private __.WriteArrToTablePrev(lst : List<string * int>) =
          use context = new ArchiveAddInfo44Context()
          for l in lst do
                let arr = ArchiveAddInfo44()
                let arr_last = $"prev_%s{fst l}"
                arr.Archive <- arr_last
                arr.SizeArch <- int <| snd l
                context.Archives.Add(arr) |> ignore
                context.SaveChanges() |> ignore
                ()
          ()  
      member private __.GetArrPrevFromFtp(pathParse : string, region : string) =
            let arch = __.GetListArrays(pathParse, S.F44)
            let yearsSeq = seq { 2015..DateTime.Now.Year }
            let _ = seq { for s in yearsSeq do yield $"_%s{region}_%d{s}" }
            let ret = query { for a in arch do
                              where (yearsSeq.Any(fun x -> (fst a).Contains(x.ToString())))
                              select a }
            use context = new ArchiveAddInfo44Context()
            let arr = List<string * int>()
            for r in ret do
                  match (snd r) with
                  | 0 | 22 -> Logging.Log.logger (sprintf "!!!archive is empty %s" <| fst r)
                  | _ -> let arr_prev = $"prev_%s{fst r}"
                         let res = context.Archives.AsNoTracking() .Where(fun x -> arr_prev = (fst r) && (int x.SizeArch = (snd r) || int x.SizeArch = 0)) .Count()
                         if res = 0 then arr.Add(r)
                         ()
            arr
      member private __.GetListFA(arch : string, pathParse : string, reg : Region) =
            let file = __.GetArch(arch, pathParse, S.F44)
            if file.Exists then
                  let dir = __.Unzipper(file)
                  if dir.Exists then
                        let fileList = dir.GetFiles().ToList()
                        for f in fileList do
                              try
                                    __.Revision(f, pathParse, reg)
                              with ex -> Logging.Log.logger ex
                        dir.Delete(true)
                  ()
            ()
      
      member private __.Revision(f : FileInfo, _ : string, reg : Region) =
            match f with
            | _ when (not (f.Name.ToLower().EndsWith(".xml"))) || f.Length = 0L -> ()
            | _ -> use sr = new StreamReader(f.FullName, Encoding.Default)
                   let str = __.DeleteBadSymbols(sr.ReadToEnd())
                   __.ParsingXml(str, reg, f)
            ()
      
      member private __.ParsingXml(s : string, reg : Region, f : FileInfo) =
            let doc = XmlDocument()
            doc.LoadXml(s)
            let json = JsonConvert.SerializeXmlNode(doc)
            let j = JObject.Parse(json)
            match j.SelectToken("$..fcsAddInfo") with
            | null -> Logging.Log.logger $"fcsAddInfo tag was not found in %s{f.FullName}"
            | x -> __.Worker(DocumentAddInfo44(f, x, reg))
            ()
      member private __.Worker(d : IDocument) =
            try
                  d.Worker()
            with ex -> Logging.Log.logger ex
      
      member private __.GetArrLastFromFtp(pathParse : string, region : string) =
            let arch = __.GetListArrays(pathParse, S.F44)
            let yearsSeq = seq { 2015..DateTime.Now.Year }
            let _ = seq { for s in yearsSeq do yield $"_%s{region}_%d{s}" }
            let ret = query { for a in arch do
                              where (yearsSeq.Any(fun x -> (fst a).Contains(x.ToString())))
                              select a }
            use context = new ArchiveAddInfo44Context()
            let arr = List<string * int>()
            for r in ret do
                  match (snd r) with
                  | 0 | 22 -> Logging.Log.logger (sprintf "!!!archive is empty %s" <| fst r)
                  | _ -> let arr_last = $"last_%s{fst r}"
                         let res = context.Archives.AsNoTracking().Where(fun x -> x.Archive = arr_last && (int x.SizeArch = (snd r) || int x.SizeArch = 0)).Count()
                         if res = 0 then arr.Add(r)
                         ()
            arr