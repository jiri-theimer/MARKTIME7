using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BO.Integrace;

namespace BL.Code
{
    /// <summary>
    /// Export faktury / dobropisu do formátu Peppol BIS Billing 3.0 (UBL 2.1).
    ///
    /// Volá se stejně jako stávající exporty:
    ///     var xml = p91ExportPeppolSupport.GeneratePeppol(rec, folder, fileName);
    ///
    /// Specifikace:  https://docs.peppol.eu/poacc/billing/3.0/
    /// Norma:        EN 16931
    /// Pro SK e-fakturaci povinné od 1.1.2027 (tuzemské B2B).
    /// </summary>
    public static class p91ExportPeppolSupport
    {
        // ── UBL jmenné prostory ──────────────────────────────────────────────
        private static readonly XNamespace NsInv =
            "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        private static readonly XNamespace NsCN =
            "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";
        private static readonly XNamespace Cac =
            "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        private static readonly XNamespace Cbc =
            "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

        // ── Identifikátory profilu Peppol BIS Billing 3.0 ───────────────────
        private const string CustomizationId =
            "urn:cen.eu:en16931:2017#compliant#urn:fdc:peppol.eu:2017:poacc:billing:3.0";
        private const string ProfileId =
            "urn:fdc:peppol.eu:2017:poacc:billing:01:1.0";

        // p92TypeFlag: 1 = ClientInvoice, 2 = CreditNote
        private const int TYPEFLAG_CREDITNOTE = 2;

        // x15ID kategorie DPH řádku: 1=0%, 2=snížená, 3=základní, 4=speciální
        private const int X15_ZERO = 1;

        // ─────────────────────────────────────────────────────────────────────
        //  Veřejné API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Vygeneruje Peppol BIS Billing 3.0 XML a uloží ho do souboru.
        /// </summary>
        /// <param name="rec">Integrační záznam faktury</param>
        /// <param name="folder">Cílová složka (vytvoří se, pokud neexistuje)</param>
        /// <param name="fileName">Název souboru, např. "FAK-2027-001.xml"</param>
        /// <returns>Vygenerovaný XML jako string, nebo null při chybě</returns>
        public static string GeneratePeppol(InputInvoice rec, string folder, string fileName)
        {
            if (rec == null) return null;

            bool isCreditNote = rec.p92TypeFlag == TYPEFLAG_CREDITNOTE;
            var doc = isCreditNote ? BuildCreditNote(rec) : BuildInvoice(rec);

            if (!string.IsNullOrEmpty(folder))
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // DŮLEŽITÉ: zapisujeme přímo do souboru s UTF-8 (bez BOM).
                // Pozn.: NIKDY neserializovat přes StringBuilder a pak File.WriteAllText –
                // StringBuilder je vždy UTF-16, deklarace by pak řekla utf-16,
                // ale bajty by byly UTF-8 → Peppol/AP soubor odmítne.
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    Encoding = new UTF8Encoding(false), // UTF-8 bez BOM
                    OmitXmlDeclaration = false
                };
                using (var w = XmlWriter.Create(Path.Combine(folder, fileName), settings))
                    doc.Save(w);
            }

            // Pro návratovou hodnotu (náhled/testy) vrátíme string s deklarací utf-8
            return Serialize(doc);
        }

        /// <summary>
        /// Vygeneruje pouze XML string (bez zápisu na disk) – vhodné pro testy a náhled.
        /// </summary>
        public static string BuildXml(InputInvoice rec)
        {
            bool isCreditNote = rec.p92TypeFlag == TYPEFLAG_CREDITNOTE;
            var doc = isCreditNote ? BuildCreditNote(rec) : BuildInvoice(rec);
            return Serialize(doc);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Faktura (Invoice, TypeCode 380)
        // ─────────────────────────────────────────────────────────────────────

        private static XDocument BuildInvoice(InputInvoice rec)
        {
            string cur = Currency(rec);

            var root = new XElement(NsInv + "Invoice",
                new XAttribute(XNamespace.Xmlns + "cac", Cac.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "cbc", Cbc.NamespaceName),

                E(Cbc, "CustomizationID", CustomizationId),
                E(Cbc, "ProfileID", ProfileId),
                E(Cbc, "ID", rec.p91Code),
                E(Cbc, "IssueDate", Date(rec.p91Date)),
                E(Cbc, "DueDate", Date(rec.p91DateMaturity)),
                E(Cbc, "InvoiceTypeCode", "380"),
                NoteOrNull(rec.p91Text1),
                E(Cbc, "DocumentCurrencyCode", cur),
                E(Cbc, "BuyerReference", rec.p91Code),

                InvoicePeriod(rec),
                Supplier(rec),
                Customer(rec),
                PaymentMeans(rec),
                PaymentTerms(rec),
                TaxTotal(rec, cur),
                LegalMonetaryTotal(rec, cur, abs: false)
            );

            int i = 1;
            foreach (var row in OrderedRows(rec))
                root.Add(InvoiceLine(i++, row, cur, abs: false));

            return Wrap(root);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Dobropis (CreditNote, TypeCode 381)
        // ─────────────────────────────────────────────────────────────────────

        private static XDocument BuildCreditNote(InputInvoice rec)
        {
            string cur = Currency(rec);

            var root = new XElement(NsCN + "CreditNote",
                new XAttribute(XNamespace.Xmlns + "cac", Cac.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "cbc", Cbc.NamespaceName),

                E(Cbc, "CustomizationID", CustomizationId),
                E(Cbc, "ProfileID", ProfileId),
                E(Cbc, "ID", rec.p91Code),
                E(Cbc, "IssueDate", Date(rec.p91Date)),
                E(Cbc, "CreditNoteTypeCode", "381"),
                NoteOrNull(rec.p91Text1),
                E(Cbc, "DocumentCurrencyCode", cur),
                E(Cbc, "BuyerReference", rec.p91Code),

                // Odkaz na původní fakturu, ke které se dobropis vztahuje
                BillingReference(rec),

                InvoicePeriod(rec),
                Supplier(rec),
                Customer(rec),
                PaymentMeans(rec),
                PaymentTerms(rec),
                TaxTotal(rec, cur),
                LegalMonetaryTotal(rec, cur, abs: true)
            );

            int i = 1;
            foreach (var row in OrderedRows(rec))
                root.Add(CreditNoteLine(i++, row, cur));

            return Wrap(root);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Hlavičkové bloky
        // ─────────────────────────────────────────────────────────────────────

        private static XElement InvoicePeriod(InputInvoice rec)
        {
            // DUZP jako jednodenní období (BT-73/BT-74)
            return new XElement(Cac + "InvoicePeriod",
                E(Cbc, "StartDate", Date(rec.p91DateSupply)),
                E(Cbc, "EndDate", Date(rec.p91DateSupply))
            );
        }

        private static XElement BillingReference(InputInvoice rec)
        {
            // Pokud máte k dispozici číslo původní faktury, doplňte ho sem.
            // V InputInvoice je k dispozici p91ID_CreditNoteBind (ID), proto ID,
            // ale ideálně předávejte p91Code původní faktury.
            string origRef = rec.p91ID_CreditNoteBind > 0
                ? rec.p91ID_CreditNoteBind.ToString()
                : rec.p91Code;

            return new XElement(Cac + "BillingReference",
                new XElement(Cac + "InvoiceDocumentReference",
                    E(Cbc, "ID", origRef)
                )
            );
        }

        // ── Dodavatel (AccountingSupplierParty) – p93* ─────────────────────

        private static XElement Supplier(InputInvoice rec)
        {
            string vat = NormVat(rec.p93ICDPH_SK_OrVat(), rec.p93CountryCode);
            string country = Country(rec.p93CountryCode);
            var (epScheme, epValue) = PeppolEndpoint(vat);

            var party = new XElement(Cac + "Party");

            if (epValue != null)
                party.Add(new XElement(Cbc + "EndpointID",
                    new XAttribute("schemeID", epScheme), epValue));

            // IČO
            if (!string.IsNullOrWhiteSpace(rec.p93RegID))
                party.Add(new XElement(Cac + "PartyIdentification",
                    E(Cbc, "ID", rec.p93RegID)));

            party.Add(new XElement(Cac + "PartyName",
                E(Cbc, "Name", rec.p93Company)));

            party.Add(new XElement(Cac + "PostalAddress",
                StreetOrNull(rec.p93Street),
                E(Cbc, "CityName", rec.p93City ?? ""),
                E(Cbc, "PostalZone", rec.p93Zip ?? ""),
                new XElement(Cac + "Country",
                    E(Cbc, "IdentificationCode", country))));

            if (vat != null)
                party.Add(new XElement(Cac + "PartyTaxScheme",
                    E(Cbc, "CompanyID", vat),
                    new XElement(Cac + "TaxScheme", E(Cbc, "ID", "VAT"))));

            party.Add(new XElement(Cac + "PartyLegalEntity",
                E(Cbc, "RegistrationName", rec.p93Company),
                string.IsNullOrWhiteSpace(rec.p93RegID)
                    ? null : E(Cbc, "CompanyID", rec.p93RegID)));

            // Kontakt
            if (!string.IsNullOrWhiteSpace(rec.p93Email) ||
                !string.IsNullOrWhiteSpace(rec.p93Contact))
            {
                party.Add(new XElement(Cac + "Contact",
                    string.IsNullOrWhiteSpace(rec.p93Contact)
                        ? null : E(Cbc, "Name", rec.p93Contact),
                    string.IsNullOrWhiteSpace(rec.p93Email)
                        ? null : E(Cbc, "ElectronicMail", rec.p93Email)));
            }

            return new XElement(Cac + "AccountingSupplierParty", party);
        }

        // ── Odběratel (AccountingCustomerParty) – p91Client* ───────────────

        private static XElement Customer(InputInvoice rec)
        {
            string vat = NormVat(
                !string.IsNullOrWhiteSpace(rec.p91Client_ICDPH_SK)
                    ? rec.p91Client_ICDPH_SK
                    : rec.p91Client_VatID,
                rec.p91ClientAddress1_Country);
            string country = Country(rec.p91ClientAddress1_Country);
            var (epScheme, epValue) = PeppolEndpoint(vat);

            var party = new XElement(Cac + "Party");

            if (epValue != null)
                party.Add(new XElement(Cbc + "EndpointID",
                    new XAttribute("schemeID", epScheme), epValue));

            if (!string.IsNullOrWhiteSpace(rec.p91Client_RegID))
                party.Add(new XElement(Cac + "PartyIdentification",
                    E(Cbc, "ID", rec.p91Client_RegID)));

            party.Add(new XElement(Cac + "PartyName",
                E(Cbc, "Name", rec.p91Client)));

            party.Add(new XElement(Cac + "PostalAddress",
                StreetOrNull(rec.p91ClientAddress1_Street),
                E(Cbc, "CityName", rec.p91ClientAddress1_City ?? ""),
                E(Cbc, "PostalZone", rec.p91ClientAddress1_ZIP ?? ""),
                new XElement(Cac + "Country",
                    E(Cbc, "IdentificationCode", country))));

            if (vat != null)
                party.Add(new XElement(Cac + "PartyTaxScheme",
                    E(Cbc, "CompanyID", vat),
                    new XElement(Cac + "TaxScheme", E(Cbc, "ID", "VAT"))));

            party.Add(new XElement(Cac + "PartyLegalEntity",
                E(Cbc, "RegistrationName", rec.p91Client),
                string.IsNullOrWhiteSpace(rec.p91Client_RegID)
                    ? null : E(Cbc, "CompanyID", rec.p91Client_RegID)));

            return new XElement(Cac + "AccountingCustomerParty", party);
        }

        // ── Platební údaje – p86* (IBAN/SWIFT) ─────────────────────────────

        private static XElement PaymentMeans(InputInvoice rec)
        {
            var pm = new XElement(Cac + "PaymentMeans",
                E(Cbc, "PaymentMeansCode", "30"),   // 30 = bezhotovostní převod (SEPA credit transfer = 58)
                E(Cbc, "PaymentID", rec.p91Code));   // VS = číslo faktury

            if (!string.IsNullOrWhiteSpace(rec.p86IBAN))
            {
                var acct = new XElement(Cac + "PayeeFinancialAccount",
                    E(Cbc, "ID", rec.p86IBAN.Replace(" ", "")));

                if (!string.IsNullOrWhiteSpace(rec.p93Company))
                    acct.Add(E(Cbc, "Name", rec.p93Company));

                if (!string.IsNullOrWhiteSpace(rec.p86SWIFT))
                    acct.Add(new XElement(Cac + "FinancialInstitutionBranch",
                        E(Cbc, "ID", rec.p86SWIFT.Replace(" ", ""))));

                pm.Add(acct);
            }

            return pm;
        }

        private static XElement PaymentTerms(InputInvoice rec)
        {
            if (string.IsNullOrWhiteSpace(rec.p91Text2)) return null;
            return new XElement(Cac + "PaymentTerms",
                E(Cbc, "Note", rec.p91Text2));
        }

        // ── Rekapitulace DPH ───────────────────────────────────────────────

        private static XElement TaxTotal(InputInvoice rec, string cur)
        {
            var subtotals = VatSubtotals(rec);
            double totalVat = subtotals.Sum(s => s.Vat);

            var tt = new XElement(Cac + "TaxTotal",
                new XElement(Cbc + "TaxAmount",
                    new XAttribute("currencyID", cur), Amt(Math.Abs(totalVat))));

            foreach (var s in subtotals)
            {
                tt.Add(new XElement(Cac + "TaxSubtotal",
                    new XElement(Cbc + "TaxableAmount",
                        new XAttribute("currencyID", cur), Amt(Math.Abs(s.Base))),
                    new XElement(Cbc + "TaxAmount",
                        new XAttribute("currencyID", cur), Amt(Math.Abs(s.Vat))),
                    new XElement(Cac + "TaxCategory",
                        E(Cbc, "ID", s.CategoryCode),
                        E(Cbc, "Percent", Amt(s.Rate)),
                        new XElement(Cac + "TaxScheme", E(Cbc, "ID", "VAT")))));
            }

            return tt;
        }

        // ── Celkové částky ─────────────────────────────────────────────────

        private static XElement LegalMonetaryTotal(InputInvoice rec, string cur, bool abs)
        {
            // BR-CO-10: LineExtensionAmount = součet čistých částek řádků
            // BR-CO-13: TaxExclusiveAmount = LineExtension (bez slev/přirážek na úrovni dokladu)
            // BR-CO-15: TaxInclusiveAmount MUSÍ = TaxExclusiveAmount + součet DPH (fatální pravidlo!)
            // BT-113:   PayableAmount = TaxInclusiveAmount - prepaid + PayableRoundingAmount
            var rows = OrderedRows(rec).ToList();
            var subtotals = VatSubtotals(rec);

            double lineSum = rows.Sum(r => r.BezDPH);                 // základ
            double vatSum = subtotals.Sum(s => s.Vat);              // DPH
            double inclusive = Math.Round(lineSum + vatSum, 2);        // MUSÍ být přesný součet

            // Skutečná částka k úhradě (může obsahovat haléřové zaokrouhlení)
            double payable = rec.p91Amount_TotalDue != 0
                ? rec.p91Amount_TotalDue
                : inclusive;

            // Rozdíl dáme do PayableRoundingAmount (BT-114), aby seděl BR-CO-16
            double rounding = Math.Round(payable - inclusive, 2);

            Func<double, double> f = abs ? Math.Abs : (x => x);

            var lmt = new XElement(Cac + "LegalMonetaryTotal",
                Money(Cbc + "LineExtensionAmount", f(lineSum), cur),
                Money(Cbc + "TaxExclusiveAmount", f(lineSum), cur),
                Money(Cbc + "TaxInclusiveAmount", f(inclusive), cur));

            // PayableRoundingAmount jen pokud opravdu existuje zaokrouhlení
            if (Math.Abs(rounding) >= 0.01)
                lmt.Add(Money(Cbc + "PayableRoundingAmount", f(rounding), cur));

            lmt.Add(Money(Cbc + "PayableAmount", f(payable), cur));

            return lmt;
        }

        // ── Řádky ──────────────────────────────────────────────────────────

        private static XElement InvoiceLine(int num, InputInvoiceRow row, string cur, bool abs)
        {
            Func<double, double> f = abs ? Math.Abs : (x => x);

            return new XElement(Cac + "InvoiceLine",
                E(Cbc, "ID", num.ToString()),
                new XElement(Cbc + "InvoicedQuantity",
                    new XAttribute("unitCode", "C62"), Qty(1)),
                Money(Cbc + "LineExtensionAmount", f(row.BezDPH), cur),
                ItemBlock(row),
                PriceBlock(row, cur, f));
        }

        private static XElement CreditNoteLine(int num, InputInvoiceRow row, string cur)
        {
            return new XElement(Cac + "CreditNoteLine",
                E(Cbc, "ID", num.ToString()),
                new XElement(Cbc + "CreditedQuantity",
                    new XAttribute("unitCode", "C62"), Qty(1)),
                Money(Cbc + "LineExtensionAmount", Math.Abs(row.BezDPH), cur),
                ItemBlock(row),
                PriceBlock(row, cur, Math.Abs));
        }

        private static XElement ItemBlock(InputInvoiceRow row)
        {
            string name = string.IsNullOrWhiteSpace(row.Oddil)
                ? "Fakturované plnění"
                : row.Oddil;

            return new XElement(Cac + "Item",
                E(Cbc, "Name", name),
                new XElement(Cac + "ClassifiedTaxCategory",
                    E(Cbc, "ID", VatCategory(row)),
                    E(Cbc, "Percent", Amt(row.DPHSazba)),
                    new XElement(Cac + "TaxScheme", E(Cbc, "ID", "VAT"))));
        }

        private static XElement PriceBlock(InputInvoiceRow row, string cur, Func<double, double> f)
        {
            // Jednotková cena = cena bez DPH za 1 ks (množství je 1)
            return new XElement(Cac + "Price",
                Money(Cbc + "PriceAmount", f(row.BezDPH), cur));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Pomocné funkce – DPH
        // ─────────────────────────────────────────────────────────────────────

        private class VatLine
        {
            public double Base;
            public double Vat;
            public double Rate;
            public string CategoryCode;
        }

        /// <summary>
        /// Seskupí řádky podle sazby DPH a sestaví rekapitulaci.
        /// </summary>
        private static List<VatLine> VatSubtotals(InputInvoice rec)
        {
            return OrderedRows(rec)
                .GroupBy(r => Math.Round(r.DPHSazba, 2))
                .Select(g => new VatLine
                {
                    Rate = g.Key,
                    Base = g.Sum(x => x.BezDPH),
                    Vat = g.Sum(x => x.DPH),
                    CategoryCode = VatCategory(g.First())
                })
                .OrderByDescending(x => x.Rate)
                .ToList();
        }

        /// <summary>
        /// Mapuje x15ID / sazbu na kód kategorie DPH dle UNCL5305.
        /// x15ID: 1=0%, 2=snížená, 3=základní, 4=speciální
        /// S = standardní/snížená sazba, Z = nulová sazba
        /// </summary>
        private static string VatCategory(InputInvoiceRow row)
        {
            if (row.x15ID == X15_ZERO || row.DPHSazba == 0)
                return "Z";
            return "S";
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Pomocné funkce – formátování a utility
        // ─────────────────────────────────────────────────────────────────────

        private static IEnumerable<InputInvoiceRow> OrderedRows(InputInvoice rec)
        {
            if (rec.InvoiceRows == null) return Enumerable.Empty<InputInvoiceRow>();
            return rec.InvoiceRows.OrderBy(r => r.Poradi);
        }

        private static string Currency(InputInvoice rec)
            => string.IsNullOrWhiteSpace(rec.j27Code) ? "EUR" : rec.j27Code.ToUpperInvariant();

        private static string Date(DateTime dt)
            => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        private static string Amt(double v)
            => Math.Round(v, 2).ToString("0.00", CultureInfo.InvariantCulture);

        private static string Qty(double v)
            => v.ToString("0.######", CultureInfo.InvariantCulture);

        private static XElement Money(XName name, double v, string cur)
            => new XElement(name, new XAttribute("currencyID", cur), Amt(v));

        private static XElement E(XNamespace ns, string name, string value)
        {
            // Ořežeme koncové/úvodní mezery a newline (data z DB je často obsahují).
            // Vnitřní zalomení v textu (např. Note) zůstávají zachována.
            value = value?.Trim() ?? "";
            return new XElement(ns + name, value);
        }

        private static XElement NoteOrNull(string note)
            => string.IsNullOrWhiteSpace(note) ? null : E(Cbc, "Note", note);

        private static XElement StreetOrNull(string street)
            => string.IsNullOrWhiteSpace(street) ? null : E(Cbc, "StreetName", street);

        private static string NormVat(string vat)
        {
            return NormVat(vat, null);
        }

        /// <summary>
        /// Normalizuje DIČ pro DPH: odstraní mezery/pomlčky a doplní předponu země,
        /// pokud chybí (BR-CO-09 vyžaduje DIČ s kódem země, např. "SK2122305911").
        /// </summary>
        /// <param name="vat">Surové DIČ z databáze</param>
        /// <param name="countryCode">Kód země strany (SK/CZ) pro doplnění předpony</param>
        private static string NormVat(string vat, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(vat)) return null;
            vat = vat.Replace(" ", "").Replace("-", "").ToUpperInvariant();

            // Už má dvoupísmennou předponu země?
            if (vat.Length >= 2 && char.IsLetter(vat[0]) && char.IsLetter(vat[1]))
                return vat;

            // Doplnit předponu podle země strany (fallback SK pro slovenský profil)
            string prefix = Country(countryCode);
            return prefix + vat;
        }

        /// <summary>
        /// Vráti správnu dvojicu (schemeID, hodnota) pre cbc:EndpointID podľa DIČ.
        ///
        /// DÔLEŽITÉ – slovenské špecifikum:
        /// Slovenská Peppol Authority (PASR) nariaďuje pre SK firmy JEDINÉ kanonické
        /// schéma 0245 s DIČ BEZ prefixu "SK". Forma 9950:SK... NIE je u SK poštárov
        /// (vrátane ePošťáka) podporovaná.
        ///   SK:  "SK2122305911"  ->  scheme "0245", hodnota "2122305911"
        ///   CZ:  "CZ25722034"     ->  scheme "9929", hodnota "CZ25722034"
        ///   DE:  "DE123456789"    ->  scheme "9930", hodnota "DE123456789"
        /// </summary>
        private static (string scheme, string value) PeppolEndpoint(string vat)
        {
            if (string.IsNullOrWhiteSpace(vat))
                return ("0245", null);

            if (vat.StartsWith("SK"))
                return ("0245", vat.Substring(2)); // DIČ bez "SK"

            // ostatné krajiny: scheme podľa VAT, hodnota vrátane prefixu krajiny
            return (Scheme(vat), vat);
        }

        private static string Scheme(string vat)
        {
            // Peppol EAS schémata (ISO 6523 / číselník Peppol):
            //   9950 = Slovakia VAT number (IČ DPH)
            //   9929 = Czech Republic VAT number (DIČ)
            //   9930 = Germany VAT number
            //   9944 = Netherlands, 9945 = Poland, 9908 = Austria ...
            // POZOR: 0184 = dánské CVR, 9922 = Bosna! Nepoužívat pro SK/CZ.
            if (vat == null) return "9950";              // default SK
            if (vat.StartsWith("SK")) return "9950";
            if (vat.StartsWith("CZ")) return "9929";
            if (vat.StartsWith("DE")) return "9930";
            if (vat.StartsWith("AT")) return "9914";     // AT VAT
            if (vat.StartsWith("PL")) return "9945";
            if (vat.StartsWith("HU")) return "9910";     // HU VAT
            return "9950";                                // fallback SK
        }

        private static string Country(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "SK";
            code = code.Trim();
            if (code.Length == 2) return code.ToUpperInvariant();
            return code.ToUpperInvariant() switch
            {
                "SLOVENSKO" or "SLOVAKIA" => "SK",
                "ČESKÁ REPUBLIKA" or "CESKA REPUBLIKA" or "CZECH REPUBLIC" => "CZ",
                _ => "SK"
            };
        }

        private static string Serialize(XDocument doc)
        {
            // Vlastní StringWriter, který hlásí UTF-8 (jinak by deklarace byla utf-16,
            // protože StringWriter standardně pracuje s UTF-16).
            var sw = new Utf8StringWriter();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = false
            };
            using (var w = XmlWriter.Create(sw, settings))
                doc.Save(w);
            return sw.ToString();
        }

        /// <summary>StringWriter, který deklaruje UTF-8 místo výchozího UTF-16.</summary>
        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => new UTF8Encoding(false);
        }

        private static XDocument Wrap(XElement root)
            => new XDocument(new XDeclaration("1.0", "UTF-8", null), root);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
//  Pomocná extension metoda – preferuje skutečné IČ DPH dodavatele (p93ICDPH_SK),
//  fallback na p93VatID jen pokud by IČ DPH chybělo.
// ─────────────────────────────────────────────────────────────────────────────
namespace BL.Code
{
    internal static class InputInvoiceVatExtensions
    {
        public static string p93ICDPH_SK_OrVat(this BO.Integrace.InputInvoice rec)
        {
            // Přednostně skutečné IČ DPH (např. "SK2122305911")
            if (!string.IsNullOrWhiteSpace(rec.p93ICDPH_SK))
                return rec.p93ICDPH_SK;
            // Fallback pro neslovenské dodavatele nebo chybějící IČ DPH
            return rec.p93VatID;
        }
    }
}