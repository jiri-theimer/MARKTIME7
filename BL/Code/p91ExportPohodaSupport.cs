using BO.Integrace;
using BO.Model.Integrace.FakturaExport;
using DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Code
{
    public static class p91ExportPohodaSupport
    {

        public static string GeneratePohodaXmlByService(List<BO.Integrace.InputInvoice> recs, HttpClient hp, string strDestFolder)
        {
            var url = "https://mas.marktime.net/Pohoda/Pack";
            var recjson = BO.Code.basJson.SerializeObject(recs);
            var requestContent = new StringContent(recjson, Encoding.UTF8, "application/json");
            var response1 = hp.PostAsync(url, requestContent).Result;
            string strXML = response1.Content.ReadAsStringAsync().Result;
            strXML = strXML.Replace("encoding=\"utf-16\"", "encoding=\"Windows-1250\"");
            BO.Code.File.WriteText2File($"{strDestFolder}\\POHODA.xml", strXML, 1250);   //pohoda vyžaduje win1250

            return $"{strDestFolder}\\POHODA.xml";
        }

        public static string GeneratePohodaXml(List<BO.Integrace.InputInvoice> recs, string strDestFolder)
        {            
            var strXML = Pack(recs);
            strXML = strXML.Replace("encoding=\"utf-16\"", "encoding=\"Windows-1250\"");
            BO.Code.File.WriteText2File($"{strDestFolder}\\POHODA.xml", strXML, 1250);   //pohoda vyžaduje win1250

            return $"{strDestFolder}\\POHODA.xml";
        }
        public static string Pack(List<InputInvoice> recs)
        {
            string strMtPack = $"MT{DateTime.Now.ToString("ddMMyyyyHHmmss")}";
            //string strFileName = $"POHODA_EXPORT_{strMtPack}.xml";
            var packs = new dataPackDataPackItem[recs.Count()]; int x = 0;

            foreach (var rec in recs)
            {
                //if (recs.Count() == 1)
                //{
                //    strFileName = $"POHODA_EXPORT_{rec.p91Code.Replace(" ","")}.xml";
                //}
                var faktura = GetFaktura(rec);
                
                string ss = BO.Code.Bas.RightString($"0000{(x + 1)}", 4);
                packs[x] = new dataPackDataPackItem() { version = "2.0", invoice = faktura, id = $"{strMtPack}-{ss}" };
                x += 1;
            }

            var finalPack = new dataPack() { version = "2.0", id = strMtPack, ico = recs.First().p93RegID, application = "MARKTIME", programVersion = "7x", note = "MARKTIME Export", dataPackItem = packs };

            finalPack.key = finalPack.ico;

            //return BO.Code.basJson.SerializeObject(finalPack);
            return serialize_object(finalPack);
        }




        
        public static string Faktura(InputInvoice rec)
        {

            var faktura = GetFaktura(rec);
            //return BO.Code.basJson.SerializeObject(faktura);
            return serialize_object(faktura);
        }


        private static invoice GetFaktura(InputInvoice rec)
        {
            bool bolForeignInvoice = false; double dblExchangeRate = 1;

            var faktura = new invoice() { version = "2.0" };
            var hlavicka = new invoiceInvoiceHeader() { symVar = BO.Code.Bas.remove_alphacharacters(rec.p91Code), symConst = "0308", date = rec.p91Date };
            if (rec.j27ID != rec.j27ID_Domestic)
            {
                bolForeignInvoice = true;
                dblExchangeRate = rec.p91ExchangeRate;
            }
            if (rec.p92TypeFlag == 2)
            {
                hlavicka.invoiceType = "issuedCorrectiveTax";
            }
            else
            {
                hlavicka.invoiceType = "issuedInvoice";
            }
            hlavicka.number = new invoiceInvoiceHeaderNumber() { numberRequested = rec.p91Code };
            hlavicka.dateDue = rec.p91DateMaturity;
            hlavicka.dateTax = rec.p91DateSupply;
            hlavicka.dateAccounting = rec.p91DateSupply;
            if (rec.p91Text1 != null)
            {
                hlavicka.text = rec.p91Text1.Length > 240 ? rec.p91Text1.Substring(0, 240) : rec.p91Text1;
            }
            if (rec.p91Text2 != null)
            {
                hlavicka.note = rec.p91Text2.Length > 240 ? rec.p91Text2.Substring(0, 240) : rec.p91Text2;

            }
            //if (rec.p91Text2 != null)
            //{
            //    hlavicka.intNote = rec.p91Text2.Length > 240 ? rec.p91Text2.Substring(0, 240) : rec.p91Text2;

            //}
            if (rec.Implementace == "zch")
            {
                hlavicka.intNote = rec.ZchPartner; //'pro ZCH/NIRRIS, prasárna
            }

            if (rec.p86Account != null)
            {
                hlavicka.account = new invoiceInvoiceHeaderAccount() { accountNo = rec.p86Account, bankCode = rec.p86Code };
            }
            if (rec.p41ID_First > 0)
            {
                if (rec.p41Code.Length > 19)
                {
                    hlavicka.contract = new invoiceInvoiceHeaderContract() { ids = rec.p41Code.Substring(0, 19) };
                }
                else
                {
                    hlavicka.contract = new invoiceInvoiceHeaderContract() { ids = rec.p41Code };
                }

            }

            if (!string.IsNullOrEmpty(rec.PredkontaceIS))
            {
                hlavicka.accounting = new invoiceInvoiceHeaderAccounting() { ids = rec.PredkontaceIS };
            }
            if (!string.IsNullOrEmpty(rec.KlasifikaceDphIS))
            {
                hlavicka.classificationVAT = new invoiceInvoiceHeaderClassificationVAT() { ids = rec.KlasifikaceDphIS };
            }
            if (rec.j18ID > 0)
            {
                hlavicka.centre = new invoiceInvoiceHeaderCentre() { ids = rec.j18Code };

            }
            ;
            var adresa = new address() { company = rec.p91Client };
            if (rec.p91ClientAddress1_City != null) adresa.city = rec.p91ClientAddress1_City;
            if (rec.p91ClientAddress1_Street != null) adresa.street = rec.p91ClientAddress1_Street;
            if (rec.p91ClientAddress1_ZIP != null) adresa.zip = rec.p91ClientAddress1_ZIP;
            if (rec.p91Client_RegID != null) adresa.ico = rec.p91Client_RegID;
            if (rec.p91Client_VatID != null)
            {
                adresa.dic = rec.p91Client_VatID;
            }
            if (rec.p91Client_ICDPH_SK != null)
            {
                adresa.dic = rec.p91Client_ICDPH_SK;    //U SK firem se do DIČ doplní jejich ICDPH
            }



            var klient = new invoiceInvoiceHeaderPartnerIdentity() { address = adresa };
            hlavicka.partnerIdentity = klient;


            adresa = new address() { company = rec.p93Company };
            if (rec.p93City != null) adresa.city = rec.p93City;
            if (rec.p93Street != null) adresa.street = rec.p93Street;
            if (rec.p93Zip != null) adresa.zip = rec.p93Zip;
            if (rec.p93RegID != null) adresa.ico = rec.p93RegID;
            if (rec.p93VatID != null) adresa.dic = rec.p93VatID;
            if (rec.p93Referent != null)
            {
                adresa.surname = rec.p93Referent;
                if (adresa.surname.Length > 32)
                {
                    adresa.surname = adresa.surname.Substring(0, 32);
                }
            }
            var dodavatel = new invoiceInvoiceHeaderMyIdentity() { address = adresa };
            hlavicka.myIdentity = dodavatel;

            faktura.invoiceHeader = hlavicka;
            invoiceInvoiceItem[] polozky = new invoiceInvoiceItem[rec.InvoiceRows.Count()]; int x = 0;

            foreach (var row in rec.InvoiceRows)
            {
                string strVatType = "high";
                switch (row.x15ID)
                {
                    case 1:
                        strVatType = "none"; break;
                    case 2:
                        strVatType = "low"; break;
                    case 0:
                        if (row.DPHSazba == 0)
                        {
                            strVatType = "none";
                        }
                        if (row.DPHSazba < 20 && row.DPHSazba > 0)
                        {
                            strVatType = "low"; //odhad
                        }
                        break;

                }

                var polozka = new invoiceInvoiceItem() { coefficient = 1, quantity = 1, rateVAT = strVatType, unit = "ks" };

                var castka_home = new invoiceInvoiceItemHomeCurrency() { unitPrice = (decimal)row.BezDPH, price = (decimal)row.BezDPH, priceVAT = (decimal)row.DPH, priceSum = (decimal)row.VcDPH };
                if (bolForeignInvoice)
                {
                    castka_home.unitPrice = Math.Round(castka_home.unitPrice * (decimal)dblExchangeRate, 2);
                    castka_home.price = Math.Round(castka_home.price * (decimal)dblExchangeRate, 2);
                    castka_home.priceVAT = Math.Round(castka_home.priceVAT * (decimal)dblExchangeRate, 2);
                    castka_home.priceSum = Math.Round(castka_home.priceSum * (decimal)dblExchangeRate, 2);

                    var castka_foreign = new invoiceInvoiceItemForeignCurrency() { unitPrice = (decimal)row.BezDPH, price = (decimal)row.BezDPH, priceVAT = (decimal)row.DPH, priceSum = (decimal)row.VcDPH };

                    polozka.foreignCurrency = castka_foreign;
                }

                polozka.homeCurrency = castka_home;
                polozka.text = row.Oddil != null && row.Oddil.Length > 90 ? row.Oddil.Substring(0, 90) : row.Oddil;
                if (!string.IsNullOrEmpty(row.PredkontaceIS))
                {
                    polozka.accounting = new invoiceInvoiceItemAccounting() { ids = row.PredkontaceIS };
                }
                if (rec.Implementace == "zch" && string.IsNullOrEmpty(row.PredkontaceIS))
                {
                    //do předkontace položky opakovat předkontaci hlavičky
                    polozka.accounting = new invoiceInvoiceItemAccounting() { ids = rec.PredkontaceIS };

                }



                if (rec.Implementace == "zch")    //do pohoda zakázky exportovat kód dokladu - zch specifikum
                {
                    var contract = new invoiceInvoiceItemContract();
                    if (row.p31Code != null)
                    {
                        contract.ids = row.p31Code;

                    }
                    polozka.contract = contract;
                }
                //else
                //{
                //    contract.ids = rec.p41Code;
                //}



                if (rec.Implementace == "zch")
                {
                    //v ZCH se středisko projektu exportuje do činnosti -> prasárna největší
                    var cinnost = new invoiceInvoiceItemActivity();
                    cinnost.ids = rec.j18Code;
                    polozka.activity = cinnost;
                }
                else
                {
                    if (!string.IsNullOrEmpty(row.CinnostIS))
                    {
                        polozka.activity = new invoiceInvoiceItemActivity() { ids = row.CinnostIS };
                    }
                    if (row.p31ID > 0 && row.p31Code != null)
                    {
                        polozka.code = row.p31Code;
                    }
                }


                polozky[x] = polozka;
                x += 1;
            }


            faktura.invoiceDetail = polozky;
            var sumar = new invoiceInvoiceSummary();
            if (bolForeignInvoice)
            {
                var mena_cizi = new invoiceInvoiceSummaryForeignCurrency();
                var mena = new currency() { ids = rec.j27Code };
                mena_cizi.currency = mena;
                mena_cizi.rate = (decimal)dblExchangeRate;
                mena_cizi.amount = 1;
                mena_cizi.priceSum = (decimal)rec.p91Amount_TotalDue;
                sumar.foreignCurrency = mena_cizi;
            }
            faktura.invoiceSummary = sumar;

            
            return faktura;
        }


        private static string serialize_object(object c)
        {
            var xml = new System.Xml.Serialization.XmlSerializer(c.GetType());
            var settings = new System.Xml.XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Indent = true;
            //settings.NamespaceHandling = System.Xml.NamespaceHandling.OmitDuplicates;

            var sb = new System.Text.StringBuilder();
            var wr = System.Xml.XmlWriter.Create(sb, settings);

            xml.Serialize(wr, c);
            wr.Close();

            return sb.ToString();
        }
    }
}
