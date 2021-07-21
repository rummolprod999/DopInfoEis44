namespace ParserFsharp
open System
open System.IO
open Newtonsoft.Json.Linq
open NewtonExt
open StringExt
open MySql.Data.MySqlClient
open System.Data

type DocumentAddInfo44() =
      static member val AddProd : int = 0 with get, set
      [<DefaultValue>] val mutable file : FileInfo
      [<DefaultValue>] val mutable item : JToken
      [<DefaultValue>] val mutable region : Region
      new(f, i, r) as this = DocumentAddInfo44()
                             then
                                 this.file <- f
                                 this.item <- i
                                 this.region <- r
      inherit AbstractDocumentFtpEis()
      interface IDocument with
        override __.Worker() =
            __.WorkerMysql()
            
      
      member __.WorkerMysql() =
          printfn $"%s{__.item.ToString()}"
          ()