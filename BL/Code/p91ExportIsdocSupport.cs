using BO.Integrace;
using BO.ISDOC;
using System.Text;

namespace BL.Code
{
    public static class p91ExportIsdocSupport
    {
        private static System.Text.StringBuilder _sb { get; set; }


        public static string GenerateIsdoc(InputInvoice rec, string strDestFolder, string strExplicitFileName = null)
        {
            // vrátí plnou cestu na vygenerovaný ISDOC soubor
            bool bolForeignInvoice = false; double dblExchangeRate = 1;

            var c = new BO.Model.Integrace.FakturaExport.Invoice
            {
                version = "6.0.2",
                EgovFlag = false,
                IssuingSystem = "MARKTIME 7",
                UUID = rec.p91Guid,
                ElectronicPossibilityAgreementReference = new BO.Model.Integrace.FakturaExport.NoteType(),
                DocumentType = BO.Model.Integrace.FakturaExport.DocumentTypeType.Item1,
                IssueDate = rec.p91Date,
                TaxPointDate = rec.p91DateSupply,
                VATApplicable = true,
                ID = rec.p91Code
            };


            c.TaxPointDateSpecified = true;

            if (!string.IsNullOrEmpty(rec.p91Text1))
            {
                var poznamka = new BO.Model.Integrace.FakturaExport.NoteType() { Value = rec.p91Text1 };
                c.Note = poznamka;
            }

            c.LocalCurrencyCode = rec.j27Code_Domestic;

            if (rec.j27ID_Domestic == rec.j27ID)
            {
                c.CurrRate = 1;
            }
            else
            {
                bolForeignInvoice = true;
                c.ForeignCurrencyCode = rec.j27Code;
                c.CurrRate = (decimal)rec.p91ExchangeRate;
                dblExchangeRate = rec.p91ExchangeRate;
            }
            c.RefCurrRate = 1;


            var dodavatel = new BO.Model.Integrace.FakturaExport.AccountingSupplierPartyType();
            dodavatel.Party = new BO.Model.Integrace.FakturaExport.PartyType();
            dodavatel.Party.PartyIdentification = new BO.Model.Integrace.FakturaExport.PartyIdentificationType() { UserID = rec.p93ID.ToString(), ID = rec.p93RegID };
            dodavatel.Party.PartyName = new BO.Model.Integrace.FakturaExport.PartyNameType() { Name = rec.p93Company };
            dodavatel.Party.PostalAddress = new BO.Model.Integrace.FakturaExport.PostalAddressType() { StreetName = rec.p93Street, CityName = rec.p93City, PostalZone = rec.p93Zip, BuildingNumber = "" };
            dodavatel.Party.PostalAddress.Country = new BO.Model.Integrace.FakturaExport.CountryType() { Name = rec.p93Country, IdentificationCode = rec.p93CountryCode };

            var taxScheme = new BO.Model.Integrace.FakturaExport.PartyTaxSchemeType() { CompanyID = rec.p93VatID, TaxScheme = "VAT" };
            if (rec.p93CountryCode == "SK")
            {
                taxScheme.TaxScheme = "TIN";    //na slovensku používají TIN, pokud není vyplněno IČ DPH
            }
            dodavatel.Party.PartyTaxScheme = new List<BO.Model.Integrace.FakturaExport.PartyTaxSchemeType>();
            dodavatel.Party.PartyTaxScheme.Add(taxScheme);

            if (!string.IsNullOrEmpty(rec.p93ICDPH_SK))
            {
                taxScheme = new BO.Model.Integrace.FakturaExport.PartyTaxSchemeType() { CompanyID = rec.p93ICDPH_SK, TaxScheme = "VAT" };
                dodavatel.Party.PartyTaxScheme.Add(taxScheme);
            }

            dodavatel.Party.Contact = new BO.Model.Integrace.FakturaExport.ContactType() { Name = rec.p93Referent, Telephone = rec.p93Contact, ElectronicMail = rec.p93Email };
            c.AccountingSupplierParty = dodavatel;

            var seller = new BO.Model.Integrace.FakturaExport.SellerSupplierPartyType() { Party = dodavatel.Party };
            c.SellerSupplierParty = seller;

            var odberatel = new BO.Model.Integrace.FakturaExport.AccountingCustomerPartyType();
            odberatel.Party = new BO.Model.Integrace.FakturaExport.PartyType();
            odberatel.Party.PartyIdentification = new BO.Model.Integrace.FakturaExport.PartyIdentificationType() { UserID = rec.p28ID.ToString(), ID = (string.IsNullOrEmpty(rec.p91Client_RegID) ? rec.p91Client_VatID : rec.p91Client_RegID) };
            odberatel.Party.PartyName = new BO.Model.Integrace.FakturaExport.PartyNameType() { Name = rec.p91Client };
            odberatel.Party.PostalAddress = new BO.Model.Integrace.FakturaExport.PostalAddressType() { StreetName = rec.p91ClientAddress1_Street, CityName = rec.p91ClientAddress1_City, PostalZone = rec.p91ClientAddress1_ZIP, BuildingNumber = "" };
            odberatel.Party.PostalAddress.Country = new BO.Model.Integrace.FakturaExport.CountryType() { Name = (!string.IsNullOrEmpty(rec.p91ClientAddress1_Country) ? rec.p91ClientAddress1_Country : "Česká republika"), IdentificationCode = (!string.IsNullOrEmpty(rec.p28CountryCode) ? rec.p28CountryCode : "CZ") };

            taxScheme = new BO.Model.Integrace.FakturaExport.PartyTaxSchemeType() { CompanyID = rec.p91Client_VatID, TaxScheme = "VAT" };
            if (rec.p93CountryCode == "SK")
            {
                taxScheme.TaxScheme = "TIN";    //na slovensku se používá TIN, pokud není vyplněno IČ DPH
            }

            odberatel.Party.PartyTaxScheme = new List<BO.Model.Integrace.FakturaExport.PartyTaxSchemeType>();
            odberatel.Party.PartyTaxScheme.Add(taxScheme);
            if (!string.IsNullOrEmpty(rec.p91Client_ICDPH_SK))
            {
                taxScheme = new BO.Model.Integrace.FakturaExport.PartyTaxSchemeType() { CompanyID = rec.p91Client_ICDPH_SK, TaxScheme = "VAT" };
                odberatel.Party.PartyTaxScheme.Add(taxScheme);
            }



            c.Items = new List<object>();
            c.Items.Add(odberatel);

            var buyer = new BO.Model.Integrace.FakturaExport.BuyerCustomerPartyType() { Party = odberatel.Party };
            c.BuyerCustomerParty = buyer;

            c.TaxTotal = new BO.Model.Integrace.FakturaExport.TaxTotalType() { TaxAmount = (decimal)rec.p91Amount_Vat };
            c.TaxTotal.TaxSubTotal = new List<BO.Model.Integrace.FakturaExport.TaxSubTotalType>();



            var taxsubtotal = new BO.Model.Integrace.FakturaExport.TaxSubTotalType()    //základní sazba DPH
            {
                TaxableAmount = (decimal)rec.p91Amount_WithoutVat_Standard,
                TaxAmount = (decimal)rec.p91Amount_Vat_Standard,
                TaxInclusiveAmount = (decimal)rec.p91Amount_WithVat_Standard,
                DifferenceTaxableAmount = (decimal)rec.p91Amount_WithoutVat_Standard,
                DifferenceTaxAmount = (decimal)rec.p91Amount_Vat_Standard,
                DifferenceTaxInclusiveAmount = (decimal)rec.p91Amount_WithVat_Standard
            };
            taxsubtotal.TaxCategory = new BO.Model.Integrace.FakturaExport.TaxCategoryType() { Percent = (decimal)rec.p91VatRate_Standard, VATApplicable = true, LocalReverseChargeFlag = false };
            if (bolForeignInvoice)
            {
                Handle_TaxSubtotal(ref taxsubtotal, rec, dblExchangeRate);
            }
            c.TaxTotal.TaxSubTotal.Add(taxsubtotal);

            taxsubtotal = new BO.Model.Integrace.FakturaExport.TaxSubTotalType()    //snížená sazba DPH
            {
                TaxableAmount = (decimal)rec.p91Amount_WithoutVat_Low,
                TaxAmount = (decimal)rec.p91Amount_Vat_Low,
                TaxInclusiveAmount = (decimal)rec.p91Amount_WithVat_Low,
                DifferenceTaxableAmount = (decimal)rec.p91Amount_WithoutVat_Low,
                DifferenceTaxAmount = (decimal)rec.p91Amount_Vat_Low,
                DifferenceTaxInclusiveAmount = (decimal)rec.p91Amount_WithVat_Low
            };
            taxsubtotal.TaxCategory = new BO.Model.Integrace.FakturaExport.TaxCategoryType() { Percent = (decimal)rec.p91VatRate_Low, VATApplicable = true, LocalReverseChargeFlag = false };
            if (bolForeignInvoice)
            {
                Handle_TaxSubtotal(ref taxsubtotal, rec, dblExchangeRate);
            }
            c.TaxTotal.TaxSubTotal.Add(taxsubtotal);

            taxsubtotal = new BO.Model.Integrace.FakturaExport.TaxSubTotalType()    //nulová sazba DPH
            {
                TaxableAmount = (decimal)rec.p91Amount_WithoutVat_None,
                TaxAmount = 0,
                TaxInclusiveAmount = (decimal)rec.p91Amount_WithoutVat_None,
                DifferenceTaxableAmount = (decimal)rec.p91Amount_WithoutVat_None,
                DifferenceTaxAmount = 0,
                DifferenceTaxInclusiveAmount = (decimal)rec.p91Amount_WithoutVat_None
            };
            taxsubtotal.TaxCategory = new BO.Model.Integrace.FakturaExport.TaxCategoryType() { Percent = 0, VATApplicable = true, LocalReverseChargeFlag = false };
            if (bolForeignInvoice)
            {
                Handle_TaxSubtotal(ref taxsubtotal, rec, dblExchangeRate);
            }
            c.TaxTotal.TaxSubTotal.Add(taxsubtotal);

            c.LegalMonetaryTotal = new BO.Model.Integrace.FakturaExport.LegalMonetaryTotalType()
            {
                TaxExclusiveAmount = (decimal)rec.p91Amount_WithoutVat,
                TaxInclusiveAmount = (decimal)rec.p91Amount_WithVat,
                DifferenceTaxExclusiveAmount = (decimal)rec.p91Amount_WithoutVat - (decimal)rec.p91ProformaBilledAmount,
                DifferenceTaxInclusiveAmount = (decimal)rec.p91Amount_WithVat - (decimal)rec.p91ProformaBilledAmount,
                PayableRoundingAmount = (decimal)rec.p91RoundFitAmount,
                PayableAmount = (decimal)rec.p91Amount_TotalDue,
                PaidDepositsAmount = (decimal)rec.p91ProformaBilledAmount

            };
            if (bolForeignInvoice)
            {
                c.LegalMonetaryTotal.TaxExclusiveAmountCurr = c.LegalMonetaryTotal.TaxExclusiveAmount;
                c.LegalMonetaryTotal.TaxExclusiveAmount = c.LegalMonetaryTotal.TaxExclusiveAmount * (decimal)dblExchangeRate;
                c.LegalMonetaryTotal.TaxInclusiveAmountCurr = c.LegalMonetaryTotal.TaxInclusiveAmount;
                c.LegalMonetaryTotal.TaxInclusiveAmount = c.LegalMonetaryTotal.TaxInclusiveAmount * (decimal)dblExchangeRate;
                c.LegalMonetaryTotal.DifferenceTaxExclusiveAmountCurr = c.LegalMonetaryTotal.DifferenceTaxExclusiveAmount;
                c.LegalMonetaryTotal.DifferenceTaxExclusiveAmount = c.LegalMonetaryTotal.DifferenceTaxExclusiveAmount * (decimal)dblExchangeRate;
                c.LegalMonetaryTotal.DifferenceTaxInclusiveAmountCurr = c.LegalMonetaryTotal.DifferenceTaxInclusiveAmount;
                c.LegalMonetaryTotal.DifferenceTaxInclusiveAmount = c.LegalMonetaryTotal.DifferenceTaxInclusiveAmount * (decimal)dblExchangeRate;
                c.LegalMonetaryTotal.PayableRoundingAmountCurr = c.LegalMonetaryTotal.PayableRoundingAmount;
                c.LegalMonetaryTotal.PayableRoundingAmount = c.LegalMonetaryTotal.PayableRoundingAmount * (decimal)dblExchangeRate;
                c.LegalMonetaryTotal.PayableAmountCurr = c.LegalMonetaryTotal.PayableAmount;
                c.LegalMonetaryTotal.PayableAmount = c.LegalMonetaryTotal.PayableAmount * (decimal)dblExchangeRate;
                c.LegalMonetaryTotal.PaidDepositsAmountCurr = c.LegalMonetaryTotal.PaidDepositsAmount;
                c.LegalMonetaryTotal.PaidDepositsAmount = c.LegalMonetaryTotal.PaidDepositsAmount * (decimal)dblExchangeRate;

            }

            var payment = new BO.Model.Integrace.FakturaExport.PaymentType() { PaidAmount = (decimal)rec.p91Amount_TotalDue, PaymentMeansCode = BO.Model.Integrace.FakturaExport.PaymentMeansCodeType.Item42 };
            payment.Details = new BO.Model.Integrace.FakturaExport.DetailsType();

            c.PaymentMeans = new BO.Model.Integrace.FakturaExport.PaymentMeansType() { Payment = new List<BO.Model.Integrace.FakturaExport.PaymentType>() };
            c.PaymentMeans.Payment.Add(payment);


            Handle_InvoiceRows(ref c, rec, bolForeignInvoice, dblExchangeRate);


            var xml = new System.Xml.Serialization.XmlSerializer(c.GetType());
            var settings = new System.Xml.XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Indent = true;

            _sb = new System.Text.StringBuilder();
            var wr = System.Xml.XmlWriter.Create(_sb, settings);

            xml.Serialize(wr, c);
            wr.Close();


            var strXML = Handle_FinishResult(rec, bolForeignInvoice);

            if (strExplicitFileName == null)
            {
                strExplicitFileName = $"{BO.Code.File.PrepareFileName(rec.p91Code, true)}.ISDOC";
            }

            var fullPath = Path.Combine(strDestFolder, strExplicitFileName);

            File.WriteAllText(fullPath, strXML, Encoding.UTF8);

            return fullPath;

        }

        private static string Handle_FinishResult(InputInvoice rec, bool bolForeignInvoice)
        {
            
            var findreplace = new List<ReplaceXmlItem>();
            findreplace.Add(new ReplaceXmlItem($"<Details />{System.Environment.NewLine}", get_details(rec)));
            findreplace.Add(new ReplaceXmlItem($"encoding=\"utf-16\"", ""));
            findreplace.Add(new ReplaceXmlItem($"encoding=\"utf-8\"", ""));

        
            string s = _sb.ToString();

            foreach (var fr in findreplace)
            {
                s = s.Replace(fr.FindString, fr.ReplaceWith);
            }
            //System.IO.File.WriteAllText("c:\\temp\\hovado.txt", s);

            //BO.Code.File.LogInfo($"{rec.p93Company}, faktura: {rec.p91Code}", "ISDOC", "Handle_FinishResult");

            return s;
        }


        private static void Handle_TaxSubtotal(ref BO.Model.Integrace.FakturaExport.TaxSubTotalType taxsubtotal, InputInvoice rec, double dblExchangeRate)
        {
            taxsubtotal.TaxableAmountCurr = taxsubtotal.TaxableAmount;
            taxsubtotal.TaxableAmount = taxsubtotal.TaxableAmount * (decimal)dblExchangeRate;
            taxsubtotal.TaxAmountCurr = taxsubtotal.TaxAmount;
            taxsubtotal.TaxAmount = taxsubtotal.TaxAmount * (decimal)dblExchangeRate;
            taxsubtotal.TaxInclusiveAmountCurr = taxsubtotal.TaxInclusiveAmount;
            taxsubtotal.TaxInclusiveAmount = taxsubtotal.TaxInclusiveAmount * (decimal)dblExchangeRate;
            taxsubtotal.DifferenceTaxableAmountCurr = taxsubtotal.DifferenceTaxableAmount;
            taxsubtotal.DifferenceTaxInclusiveAmount = taxsubtotal.DifferenceTaxInclusiveAmount * (decimal)dblExchangeRate;
            taxsubtotal.DifferenceTaxInclusiveAmountCurr = taxsubtotal.DifferenceTaxInclusiveAmount;
            taxsubtotal.DifferenceTaxInclusiveAmount = taxsubtotal.DifferenceTaxInclusiveAmount * (decimal)dblExchangeRate;
            taxsubtotal.DifferenceTaxInclusiveAmountCurr = taxsubtotal.DifferenceTaxInclusiveAmount;

        }

        private static void Handle_InvoiceRows(ref BO.Model.Integrace.FakturaExport.Invoice doc, InputInvoice rec, bool bolForeignInvoice, double dblExchangeRate)
        {
            if (rec.InvoiceRows == null)
            {
                return;
            }
            doc.InvoiceLines = new List<BO.Model.Integrace.FakturaExport.InvoiceLineType>();
            foreach (var c in rec.InvoiceRows)
            {
                var line = new BO.Model.Integrace.FakturaExport.InvoiceLineType()
                {
                    ID = c.RowPID.ToString(),
                    UnitPrice = (decimal)c.BezDPH,
                    InvoicedQuantity = new BO.Model.Integrace.FakturaExport.QuantityType() { Value = 1 },
                    LineExtensionAmount = (decimal)c.BezDPH,
                    LineExtensionAmountTaxInclusive = (decimal)c.VcDPH,
                    LineExtensionTaxAmount = (decimal)c.DPH,
                    UnitPriceTaxInclusive = (decimal)c.VcDPH
                };
                line.ClassifiedTaxCategory = new BO.Model.Integrace.FakturaExport.ClassifiedTaxCategoryType() { Percent = (decimal)c.DPHSazba, VATCalculationMethod = 0, VATApplicable = true };
                line.Item = new BO.Model.Integrace.FakturaExport.ItemType() { Description = c.Oddil };
                doc.InvoiceLines.Add(line);
            }

        }

        private static string get_details(InputInvoice rec)
        {
           
            var lis = new List<string>();
            lis.Add($"<PaymentDueDate>{rec.p91DateMaturity.ToString("yyyy-MM-dd")}</PaymentDueDate>");
            lis.Add($"<ID>{rec.p86Account}</ID>");
            lis.Add($"<BankCode>{rec.p86Code}</BankCode>");
            lis.Add($"<Name>{rec.p86BankName}</Name>");
            lis.Add($"<IBAN>{rec.p86IBAN}</IBAN>");
            lis.Add($"<BIC>{rec.p86SWIFT}</BIC>");
            lis.Add($"<VariableSymbol>{BO.Code.Bas.RemoveDiacritics(rec.p91Code)}</VariableSymbol>");
            lis.Add($"<ConstantSymbol></ConstantSymbol>");
            lis.Add($"<SpecificSymbol></SpecificSymbol>");

            return $"<Details>{System.Environment.NewLine}{string.Join(System.Environment.NewLine, lis)}{System.Environment.NewLine}</Details>";
        }

        
        public static string GenerateIsdocByService(BO.Integrace.InputInvoice rec,HttpClient hp,string strDestFolder,string strExplicitFileName=null)
        {
            //vrátí plnou cestu na vygenerovaný ISDOC soubor

            var recjson = BO.Code.basJson.SerializeObject(rec);
            var requestContent = new StringContent(recjson, Encoding.UTF8, "application/json");
            
            var url = "https://mas.marktime.net/Isdoc";
            url = "127.0.0.1 mas.marktime.net";
            var response1 = hp.PostAsync(url, requestContent).Result;
            string strXML = response1.Content.ReadAsStringAsync().Result;
            if (strExplicitFileName == null)
            {
                strExplicitFileName = $"{BO.Code.File.PrepareFileName(rec.p91Code,true)}.ISDOC";
            }
            BO.Code.File.WriteText2File($"{strDestFolder}\\{strExplicitFileName}", strXML);

            return $"{strDestFolder}\\{strExplicitFileName}";

        }

        public static string GeneratePohodaXml(List<BO.Integrace.InputInvoice> recs,HttpClient hp,string strDestFolder)
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
    }
}
