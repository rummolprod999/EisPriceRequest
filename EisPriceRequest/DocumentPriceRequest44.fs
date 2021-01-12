namespace ParserFsharp
open System
open System.IO
open Newtonsoft.Json.Linq
open NewtonExt
open StringExt
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
          if registryNum = "" then failwith <| sprintf "registryNum not found %s" __.file.Name
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
              let xml = __.GetXml(__.file.FullName)
              let idPriceRequest = ref 0
              let insertPriceRequest =
                String.Format ("INSERT INTO {0}request_for_prices SET eis_id = @eis_id, docPublishDate = @docPublishDate, request_startDate = @request_startDate, request_endDate = @request_endDate, purchase_startDate = @purchase_startDate, purchase_endDate = @purchase_endDate, registryNum = @registryNum, versionNumber = @versionNumber, eis_state = @eis_state, pubOrg_regNum = @pubOrg_regNum, pubOrg_consRegistryNum = @pubOrg_consRegistryNum, pubOrg_respRole = @pubOrg_respRole, href = @href, printForm_url = @printForm_url, requestObjectInfo = @requestObjectInfo, responsibleInfo_place = @responsibleInfo_place, contactPerson_FIO = @contactPerson_FIO, contactEMail = @contactEMail, contactPhone = @contactPhone, addInfo = @addInfo, cancel = @cancel, xml = @xml, id_region = @id_region", S.Settings.Pref)
              let cmdInsertPR = new MySqlCommand(insertPriceRequest, con)
              cmdInsertPR.Prepare()
              cmdInsertPR.Parameters.AddWithValue("@eis_id", eis_id) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@docPublishDate", docPublishDate) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@request_startDate", request_startDate) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@request_endDate", request_endDate) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@purchase_startDate", purchase_startDate) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@purchase_endDate", purchase_endDate) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@registryNum", registryNum) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@versionNumber", versionNumber) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@eis_state", eis_state) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@pubOrg_regNum", pubOrg_regNum) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@pubOrg_consRegistryNum", pubOrg_consRegistryNum) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@pubOrg_respRole", pubOrg_respRole) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@href", href) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@printForm_url", printForm_url) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@requestObjectInfo", requestObjectInfo) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@responsibleInfo_place", responsibleInfo_place) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@contactPerson_FIO", contactPerson_FIO) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@contactEMail", contactEMail) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@contactPhone", contactPhone) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@addInfo", addInfo) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@cancel", cancel) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@xml", xml) |> ignore
              cmdInsertPR.Parameters.AddWithValue("@id_region", __.region.Id) |> ignore
              cmdInsertPR.ExecuteNonQuery() |> ignore
              idPriceRequest := int cmdInsertPR.LastInsertedId
              match updated with
              | true -> AbstractDocumentFtpEis.Upd <- AbstractDocumentFtpEis.Upd + 1
              | false -> AbstractDocumentFtpEis.Add <- AbstractDocumentFtpEis.Add + 1
              let products = __.item.GetElements("products.product")
              for product in products do
                  let objectInfo = GetStringFromJtoken __.item "products.objectInfo"
                  let product_OKPD2_code = GetStringFromJtoken product "OKPD2.code"
                  let product_OKPD2_name = GetStringFromJtoken product "OKPD2.name"
                  let product_name = GetStringFromJtoken product "name"
                  let product_OKEI_code =  GetStringFromJtoken product "OKEI.code"
                  let product_OKEI_name =  GetStringFromJtoken product "OKEI.name"
                  let product_quantity = GetStringFromJtoken product "quantity"
                  (*let product_quantity = match Decimal.TryParse(product_quantity.GetPriceFromString()) with
                                          | (true, y) -> y
                                          | _ -> 0m*)
                  let products_identity = GetStringFromJtoken product "identity"
                  let insertProduct =
                    String.Format ("INSERT INTO {0}request_for_prices_objects SET rfp_id = @rfp_id, objectInfo = @objectInfo, product_OKPD2_code = @product_OKPD2_code, product_OKPD2_name = @product_OKPD2_name, product_name = @product_name, product_OKEI_code = @product_OKEI_code, product_OKEI_name = @product_OKEI_name, product_quantity = @product_quantity, products_identity = @products_identity", S.Settings.Pref)
                  let cmdInsertProduct = new MySqlCommand(insertProduct, con)
                  cmdInsertProduct.Prepare()
                  cmdInsertProduct.Parameters.AddWithValue("@rfp_id", !idPriceRequest) |> ignore
                  cmdInsertProduct.Parameters.AddWithValue("@objectInfo", objectInfo) |> ignore
                  cmdInsertProduct.Parameters.AddWithValue("@product_OKPD2_code", product_OKPD2_code) |> ignore
                  cmdInsertProduct.Parameters.AddWithValue("@product_OKPD2_name", product_OKPD2_name) |> ignore
                  cmdInsertProduct.Parameters.AddWithValue("@product_name", product_name) |> ignore
                  cmdInsertProduct.Parameters.AddWithValue("@product_OKEI_code", product_OKEI_code) |> ignore
                  cmdInsertProduct.Parameters.AddWithValue("@product_OKEI_name", product_OKEI_name) |> ignore
                  cmdInsertProduct.Parameters.AddWithValue("@product_quantity", product_quantity) |> ignore
                  cmdInsertProduct.Parameters.AddWithValue("@products_identity", products_identity) |> ignore
                  cmdInsertProduct.ExecuteNonQuery() |> ignore
                  ()
              let attachments = __.item.GetElements("attachments.attachment")
              for attachment in attachments do
                  let fileName = GetStringFromJtoken attachment "fileName"
                  let fileSize  = GetStringFromJtoken attachment "fileSize"
                  let docDescription = GetStringFromJtoken attachment "docDescription"
                  let hrefAtt  = GetStringFromJtoken attachment "url"
                  let insertAttachment =
                    String.Format ("INSERT INTO {0}request_for_prices_attachments SET rfp_id = @rfp_id, fileName = @fileName, fileSize = @fileSize, docDescription = @docDescription, href = @href", S.Settings.Pref)
                  let cmdInsertAttachment = new MySqlCommand(insertAttachment, con)
                  cmdInsertAttachment.Prepare()
                  cmdInsertAttachment.Parameters.AddWithValue("@rfp_id", !idPriceRequest) |> ignore
                  cmdInsertAttachment.Parameters.AddWithValue("@fileName", fileName) |> ignore
                  cmdInsertAttachment.Parameters.AddWithValue("@fileSize", fileSize) |> ignore
                  cmdInsertAttachment.Parameters.AddWithValue("@docDescription", docDescription) |> ignore
                  cmdInsertAttachment.Parameters.AddWithValue("@href", hrefAtt) |> ignore
                  cmdInsertAttachment.ExecuteNonQuery() |> ignore
                  ()
              let eis_conditions_payment  = GetStringFromJtoken __.item "conditions.payment"
              let eis_conditions_main  = GetStringFromJtoken __.item "conditions.main"
              let eis_conditions_contractGuarantee  = GetStringFromJtoken __.item "conditions.contractGuarantee"
              let eis_conditions_warranty  = GetStringFromJtoken __.item "conditions.warranty"
              let eis_conditions_delivery  = GetStringFromJtoken __.item "conditions.delivery"
              let eis_conditions_addInfo  = GetStringFromJtoken __.item "conditions.addInfo"
              let insertConditions =
                    String.Format ("INSERT INTO {0}request_for_prices_conditions SET rfp_id = @rfp_id, eis_conditions_payment = @eis_conditions_payment, eis_conditions_main = @eis_conditions_main, eis_conditions_contractGuarantee = @eis_conditions_contractGuarantee, eis_conditions_warranty = @eis_conditions_warranty, eis_conditions_delivery = @eis_conditions_delivery, eis_conditions_addInfo = @eis_conditions_addInfo", S.Settings.Pref)
              let cmdInsertConditions = new MySqlCommand(insertConditions, con)
              cmdInsertConditions.Prepare()
              cmdInsertConditions.Parameters.AddWithValue("@rfp_id", !idPriceRequest) |> ignore
              cmdInsertConditions.Parameters.AddWithValue("@eis_conditions_payment", eis_conditions_payment) |> ignore
              cmdInsertConditions.Parameters.AddWithValue("@eis_conditions_main", eis_conditions_main) |> ignore
              cmdInsertConditions.Parameters.AddWithValue("@eis_conditions_contractGuarantee", eis_conditions_contractGuarantee) |> ignore
              cmdInsertConditions.Parameters.AddWithValue("@eis_conditions_warranty", eis_conditions_warranty) |> ignore
              cmdInsertConditions.Parameters.AddWithValue("@eis_conditions_delivery", eis_conditions_delivery) |> ignore
              cmdInsertConditions.Parameters.AddWithValue("@eis_conditions_addInfo", eis_conditions_addInfo) |> ignore
              cmdInsertConditions.ExecuteNonQuery() |> ignore
          ()