namespace ParserFsharp
open System
open System.IO
open Newtonsoft.Json.Linq
open NewtonExt
open Microsoft.EntityFrameworkCore
open System.Linq
open System.Collections.Generic
open MySql.Data.MySqlClient
open System.Data

type DocumentPriceRequest44() =
      static member val AddProd : int = 0 with get, set
      [<DefaultValue>] val mutable file : FileInfo
      [<DefaultValue>] val mutable item : JToken
      [<DefaultValue>] val mutable region : Region
      new(f, i, r) as this = DocumentPriceRequest44()
                             then
                                 this.file <- f
                                 this.item <- i
                                 this.region <- r
      inherit AbstractDocumentFtpEis()
      interface IDocument with
        override __.Worker() =
            __.WorkerMysql()
            
      
      member __.WorkerMysql() =
          let eis_id = GetStringFromJtoken __.item "id"
          let registryNum = GetStringFromJtoken __.item "registryNum"
          if registryNum = "" then failwith <| sprintf "id or registryNum not found %s" __.file.Name
          let versionNumber = match __.item.SelectToken("versionNumber") with
                                | null -> 1
                                | x -> (int) x
          let mutable cancel = 0
          let mutable updated = false
          use con = new MySqlConnection(S.Settings.ConS)
          con.Open()
          let selectDoc =
            sprintf "SELECT id FROM %srequest_for_prices WHERE registryNum = @registryNum AND versionNumber = @versionNumber" S.Settings.Pref
          let cmd : MySqlCommand = new MySqlCommand(selectDoc, con)
          cmd.Prepare()
          cmd.Parameters.AddWithValue("@registryNum", registryNum) |> ignore
          cmd.Parameters.AddWithValue("@versionNumber", versionNumber) |> ignore
          let reader : MySqlDataReader = cmd.ExecuteReader()
          if reader.HasRows then reader.Close()
          else
              reader.Close()
              let mutable maxNum = 0
              let selectMax = sprintf "SELECT IFNULL(MAX(versionNumber), 0) AS m FROM %srequest_for_prices WHERE registryNum = @registryNum" S.Settings.Pref
              let cmd1 : MySqlCommand = new MySqlCommand(selectMax, con)
              cmd1.Prepare()
              cmd1.Parameters.AddWithValue("@registryNum", registryNum) |> ignore
              match cmd1.ExecuteReader() with
              | x when x.HasRows -> x.Read() |> ignore
                                    maxNum <- x.GetInt32("m")
                                    x.Close()
              | y -> y.Close()
              if maxNum <> 0 then updated <- true
              if versionNumber >= maxNum then
                    let selectDocs = sprintf "SELECT id, cancel FROM %srequest_for_prices WHERE registryNum = @registryNum" S.Settings.Pref
                    let cmd2 : MySqlCommand = new MySqlCommand(selectDocs, con)
                    cmd2.Prepare()
                    cmd2.Parameters.AddWithValue("@registryNum", registryNum) |> ignore
                    let adapter = new MySqlDataAdapter()
                    adapter.SelectCommand <- cmd2
                    let dt = new DataTable()
                    adapter.Fill(dt) |> ignore
                    for row in dt.Rows do
                        row.["cancel"] <- 1
                    let commandBuilder = new MySqlCommandBuilder(adapter)
                    commandBuilder.ConflictOption <- ConflictOption.OverwriteChanges
                    adapter.Update(dt) |> ignore
              else cancel <- 1
              let docPublishDate = GetExactDateTimeStringFromJtoken __.item "docPublishDate"
              let request_startDate = GetExactDateTimeStringFromJtoken __.item "procedureInfo.request.startDate"
              let request_endDate  = GetExactDateTimeStringFromJtoken __.item "procedureInfo.request.endDate"
              let purchase_startDate  = GetExactDateTimeStringFromJtoken __.item "procedureInfo.purchase.startDate"
              let purchase_endDate   = GetExactDateTimeStringFromJtoken __.item "procedureInfo.purchase.endDate"
              let eis_state = GetStringFromJtoken __.item "state"
              let pubOrg_regNum = GetStringFromJtoken __.item "publishOrg.regNum"
              let pubOrg_consRegistryNum = GetStringFromJtoken __.item "publishOrg.consRegistryNum"
              let pubOrg_respRole = GetStringFromJtoken __.item "publishOrg.responsibleRole"
              let href  = GetStringFromJtoken __.item "href"
              let printForm_url = GetStringFromJtoken __.item "printForm.url"
              let requestObjectInfo  = GetStringFromJtoken __.item "requestObjectInfo"
              let responsibleInfo_place  = GetStringFromJtoken __.item "responsibleInfo.place"
              let contactPerson_LastName  = GetStringFromJtoken __.item "responsibleInfo.contactPerson.lastName"
              let contactPerson_FirstName = GetStringFromJtoken __.item "responsibleInfo.contactPerson.firstName"
              let contactPerson_MiddleName = GetStringFromJtoken __.item "responsibleInfo.contactPerson.middleName"
              let contactPerson_FIO = sprintf "%s %s %s" contactPerson_LastName contactPerson_FirstName contactPerson_MiddleName
              let contactPerson_FIO = contactPerson_FIO.Trim()
              let contactEMail  = GetStringFromJtoken __.item "responsibleInfo.contactEMail"
              let contactPhone = GetStringFromJtoken __.item "responsibleInfo.contactPhone"
              let addInfo = GetStringFromJtoken __.item "conditions.addInfo"
              let insertPriceRequest =
                String.Format ("INSERT INTO {0}request_for_prices SET eis_id = @eis_id, docPublishDate = @docPublishDate, request_startDate = @request_startDate, request_endDate = @request_endDate, purchase_startDate = @purchase_startDate, purchase_endDate = @purchase_endDate, registryNum = @registryNum, versionNumber = versionNumber, eis_state = @eis_state, pubOrg_regNum = @pubOrg_regNum, pubOrg_consRegistryNum = @pubOrg_consRegistryNum, pubOrg_respRole = @pubOrg_respRole, href = @href, printForm_url = @printForm_url, requestObjectInfo = @requestObjectInfo, responsibleInfo_place = @responsibleInfo_place, contactPerson_FIO = @contactPerson_FIO, contactEMail = @contactEMail, contactPhone = @contactPhone, addInfo = @addInfo, cancel = @cancel", S.Settings.Pref)
              let cmdInsertPR = new MySqlCommand(insertPriceRequest, con)
              cmdInsertPR.Prepare()
              ()
          ()