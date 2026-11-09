using System.Globalization;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;

namespace BL.Code
{
    /// <summary>
    /// Načtení přijatého dokladu ve formátu ISDOC (import výdaje do úkonu, p31/Record).
    /// Čte se podle lokálních názvů prvků – funguje pro ISDOC 6.x (http://isdoc.cz/namespace/2013) i 5.x (http://isdoc.cz/namespace/invoice);
    /// chybějící nepovinné prvky nevadí. Dříve služba https://mas.marktime.net/IsdocImport (faktura opouštěla instalaci).
    /// </summary>
    public static class IsdocImportSupport
    {
        /// <summary>ISDOC/XML dokument → údaje pro výdaj. Při nečitelném souboru výjimka InvalidDataException s českou zprávou.</summary>
        public static BO.Integrace.IsdocImport Parse(byte[] data)
        {
            XDocument doc;
            try
            {
                var settings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
                using (var ms = new MemoryStream(data))
                using (var reader = XmlReader.Create(ms, settings))      //kódování podle XML hlavičky
                {
                    doc = XDocument.Load(reader);
                }
            }
            catch (XmlException ex)
            {
                throw new InvalidDataException("Soubor není platný XML dokument: " + ex.Message, ex);
            }

            var inv = doc.Root;
            if (inv == null || inv.Name.LocalName != "Invoice")
            {
                throw new InvalidDataException("Soubor není doklad ve formátu ISDOC (chybí kořenový prvek Invoice).");
            }

            var c = new BO.Integrace.IsdocImport();
            c.doklad_id = Str(Child(inv, "ID"));
            c.doklad_typ = (int)Num(Child(inv, "DocumentType"));
            c.platce_dph = !string.Equals(Str(Child(inv, "VATApplicable")), "false", StringComparison.OrdinalIgnoreCase);
            c.datum_vystaveni = Date(Child(inv, "IssueDate"));
            c.datum_duzp = Date(Child(inv, "TaxPointDate"));
            c.datum_splatnost = Date(Path(inv, "PaymentMeans", "Payment", "Details", "PaymentDueDate"));
            c.variabilni_symbol = Str(Path(inv, "PaymentMeans", "Payment", "Details", "VariableSymbol"));

            //dodavatel
            var party = Path(inv, "AccountingSupplierParty", "Party");
            if (party != null)
            {
                c.dodavatel_ico = Str(Path(party, "PartyIdentification", "ID"));
                c.dodavatel_nazev = Str(Path(party, "PartyName", "Name"));
                var adr = Child(party, "PostalAddress");
                if (adr != null)
                {
                    c.dodavatel_ulice = $"{Str(Child(adr, "StreetName"))} {Str(Child(adr, "BuildingNumber"))}".Trim();
                    c.dodavatel_mesto = Str(Child(adr, "CityName"));
                    c.dodavatel_psc = Str(Child(adr, "PostalZone"));
                }
                var schemes = party.Elements().Where(e => e.Name.LocalName == "PartyTaxScheme").ToList();
                var vat = schemes.FirstOrDefault(e => Str(Child(e, "TaxScheme")) == "VAT") ?? schemes.FirstOrDefault();
                if (vat != null) c.dodavatel_dic = Str(Child(vat, "CompanyID"));
            }

            //měna: při cizí měně jsou hlavní částky v tuzemské měně a částky v cizí měně v prvcích …Curr
            string strForeign = Str(Child(inv, "ForeignCurrencyCode"));
            bool bolCurr = !string.IsNullOrEmpty(strForeign);
            c.mena = bolCurr ? strForeign : (Str(Child(inv, "LocalCurrencyCode")) ?? "CZK");
            string sfx = bolCurr ? "Curr" : "";

            var total = Child(inv, "LegalMonetaryTotal");
            if (total != null)
            {
                c.castka_bezdph = Num(Child(total, "TaxExclusiveAmount" + sfx));
                c.castka_vcdphh = Num(Child(total, "TaxInclusiveAmount" + sfx));
                c.castka_dph = Math.Round(c.castka_vcdphh - c.castka_bezdph, 2);
            }

            //sazby DPH: rekapitulace po sazbách (TaxTotal/TaxSubTotal), sazba s největším základem
            var subs = inv.Elements().Where(e => e.Name.LocalName == "TaxTotal").SelectMany(e => e.Elements().Where(x => x.Name.LocalName == "TaxSubTotal"))
                .Select(e => new { rate = Num(Path(e, "TaxCategory", "Percent")), basis = Num(Child(e, "TaxableAmount" + sfx)), tax = Num(Child(e, "TaxAmount" + sfx)) })
                .Where(e => e.basis != 0 || e.tax != 0)
                .ToList();
            c.sazby = subs.Select(e => e.rate).Distinct().OrderByDescending(r => r).ToList();
            if (subs.Count > 0)
            {
                c.sazba_dph = subs.GroupBy(e => e.rate).OrderByDescending(g => Math.Abs(g.Sum(x => x.basis))).First().Key;
            }
            if (!c.platce_dph)
            {
                c.sazba_dph = 0;
            }

            //text: poznámka dokladu, jinak popisy položek
            c.doklad_text = Str(Child(inv, "Note"));
            if (string.IsNullOrEmpty(c.doklad_text))
            {
                var popisy = (Child(inv, "InvoiceLines")?.Elements() ?? Enumerable.Empty<XElement>())
                    .Select(line => Str(Path(line, "Item", "Description")) ?? Str(Child(line, "Note")))
                    .Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
                if (popisy.Count > 0)
                {
                    c.doklad_text = string.Join("; ", popisy.Take(10)) + (popisy.Count > 10 ? $" … (+{popisy.Count - 10})" : "");
                }
            }
            if (c.doklad_text != null && c.doklad_text.Length > 2000) c.doklad_text = c.doklad_text.Substring(0, 2000);

            //upozornění pro uživatele
            if (c.sazby.Count > 1)
            {
                c.upozorneni.Add($"Doklad má více sazeb DPH ({string.Join(", ", c.sazby.Select(r => r.ToString("0.##", CultureInfo.InvariantCulture) + " %"))}). Převzaty celkové částky a sazba s největším základem ({c.sazba_dph.ToString("0.##", CultureInfo.InvariantCulture)} %) – zkontrolujte, případně rozdělte na více výdajů.");
            }
            switch (c.doklad_typ)
            {
                case 2:
                case 3:
                case 6:
                    c.upozorneni.Add("Doklad je opravný daňový doklad (dobropis / vrubopis) – zkontrolujte znaménko částek.");
                    break;
                case 4:
                    c.upozorneni.Add("Doklad je zálohová faktura (není daňový doklad).");
                    break;
            }
            if (c.datum_duzp == null && c.datum_vystaveni != null)
            {
                c.upozorneni.Add("Doklad nemá DUZP – jako datum výdaje použito datum vystavení.");
            }

            return c;
        }

        /// <summary>ISDOCX (ZIP: ISDOC + přílohy) → obsah ISDOC souboru, nebo null.</summary>
        public static byte[] ExtractFromIsdocx(byte[] zip)
        {
            try
            {
                using (var ms = new MemoryStream(zip))
                using (var za = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    var entry = za.Entries.FirstOrDefault(e => e.FullName.EndsWith(".isdoc", StringComparison.OrdinalIgnoreCase));
                    if (entry == null) return null;
                    if (entry.Length > 20 * 1024 * 1024) throw new InvalidDataException("ISDOC soubor v ISDOCX je příliš velký.");
                    using (var es = entry.Open())
                    using (var outms = new MemoryStream())
                    {
                        es.CopyTo(outms);
                        return outms.ToArray();
                    }
                }
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Soubor není platný ISDOCX (ZIP) dokument.", ex);
            }
        }

        // ------------------------------------------------------------------ pomocné

        private static XElement Child(XElement e, string localName)
        {
            return e?.Elements().FirstOrDefault(x => x.Name.LocalName == localName);
        }

        private static XElement Path(XElement e, params string[] localNames)
        {
            foreach (var n in localNames)
            {
                e = Child(e, n);
                if (e == null) return null;
            }
            return e;
        }

        private static string Str(XElement e)
        {
            var s = e?.Value?.Trim();
            return string.IsNullOrEmpty(s) ? null : s;
        }

        private static double Num(XElement e)
        {
            var s = Str(e);
            if (s == null) return 0;
            return double.TryParse(s.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? d : 0;
        }

        private static DateTime? Date(XElement e)
        {
            var s = Str(e);
            if (s == null) return null;
            if (DateTime.TryParseExact(s.Length >= 10 ? s.Substring(0, 10) : s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d)) return d;
            return null;
        }
    }
}
