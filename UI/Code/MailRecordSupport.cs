using System.Text;
using System.Text.RegularExpressions;

namespace UI.Code
{
    /// <summary>
    /// Nový záznam z e-mailu uloženého ve FILEBOXu (o27): předvyplnění formuláře úkolu (p56/Record?o27id=) a úkonu (p31/Record?o27id=).
    /// E-mail se přečte z údajů uložených v o27 (předmět, tělo, odesílatel, datum), soubor se znovu neotevírá.
    /// </summary>
    public static class MailRecordSupport
    {
        private const int BodyMaxChars = 20000;
        private static readonly Regex RxPrefix = new Regex(@"^\s*((re|fw|fwd|aw|wg|odp|sv)\s*(\[\d+\])?\s*:\s*)+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>E-mail, ze kterého smí aktuální uživatel zakládat záznam; jinak null.</summary>
        public static BO.o27Attachment LoadMail(BL.Factory f, int o27id)
        {
            if (o27id <= 0) return null;
            var rec = f.o27AttachmentBL.Load(o27id);
            if (rec == null || !rec.IsMail) return null;
            if (f.CurrentUser.x01ID > 0 && rec.x01ID > 0 && rec.x01ID != f.CurrentUser.x01ID) return null;     //cizí licence (TotalCloud)
            return rec;
        }

        /// <summary>Guid fronty přetažených souborů z prohlížeče (BO.Code.Bas.GetGuid) - jen bezpečné znaky.</summary>
        public static bool IsTempGuid(string guid)
        {
            return !string.IsNullOrEmpty(guid) && Regex.IsMatch(guid, @"^[A-Za-z0-9\-]{16,64}$");
        }

        /// <summary>Předmět bez úvodních RE:/FW: a zkrácený na danou délku.</summary>
        public static string Subject(BO.o27Attachment rec, int maxlen)
        {
            string s = (rec.o27MailSubject ?? rec.o27Name ?? rec.o27OriginalFileName ?? "").Trim();
            s = RxPrefix.Replace(s, "").Trim();
            s = Regex.Replace(s, @"\s+", " ");
            if (s.Length > maxlen) s = s.Substring(0, maxlen - 1).TrimEnd() + "…";
            return s;
        }

        public static DateTime? MailDate(BO.o27Attachment rec)
        {
            var d = rec.o27MailDateMessage ?? rec.o27MailDateReceived;
            return d == null ? (DateTime?)null : d.Value.Date;
        }

        /// <summary>Tělo e-mailu jako HTML do popisu úkolu: hlavička (od, datum) + prostý text zprávy.</summary>
        public static string BodyAsHtml(BL.Factory f, BO.o27Attachment rec)
        {
            var sb = new StringBuilder();
            string from = string.IsNullOrWhiteSpace(rec.o27MailSenderName) || rec.o27MailSenderName == rec.o27MailSenderAddress
                ? rec.o27MailSenderAddress
                : $"{rec.o27MailSenderName} <{rec.o27MailSenderAddress}>";
            var d = rec.o27MailDateMessage ?? rec.o27MailDateReceived;
            var head = new List<string>();
            if (!string.IsNullOrWhiteSpace(from)) head.Add($"{f.tra("Od")}: {from}");
            if (d != null) head.Add($"{f.tra("Datum")}: {d.Value:dd.MM.yyyy HH:mm}");
            if (head.Count > 0) sb.Append("<p><em>" + System.Net.WebUtility.HtmlEncode(string.Join(", ", head)) + "</em></p>");

            string body = (rec.o27MailBodyText ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Trim();
            body = Regex.Replace(body, @"\n{3,}", "\n\n");
            if (body.Length > BodyMaxChars) body = body.Substring(0, BodyMaxChars) + "\n…";
            foreach (string par in body.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                sb.Append("<p>" + System.Net.WebUtility.HtmlEncode(par).Replace("\n", "<br>") + "</p>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Projekt pro nový záznam: podle vazby e-mailu ve FILEBOXu (projekt, úkol, klient s jediným otevřeným projektem),
        /// jinak podle adresy odesílatele (kontakt → jeho jediný otevřený projekt). Když to není jednoznačné, vrací 0.
        /// </summary>
        public static int FindProject(BL.Factory f, BO.o27Attachment rec)
        {
            try
            {
                switch (rec.o27Entity)
                {
                    case "p41":
                        return rec.o27RecordPid;
                    case "p56":
                        var recP56 = f.p56TaskBL.Load(rec.o27RecordPid);
                        return recP56 == null ? 0 : recP56.p41ID;
                    case "p28":
                        return SingleProject(f, new List<int> { rec.o27RecordPid });
                }

                string mail = (rec.o27MailSenderAddress ?? "").Trim().ToLowerInvariant();
                if (!mail.Contains("@")) return 0;
                //e-mail → kontakt (vlastní médium i kontaktní osoba firmy) - stejně jako „Vykázat z Outlooku“
                var hits = f.db.GetList<ContactHit>(@"SELECT p28.p28ID, ISNULL(p30.p28ID, 0) as p28ID_Company
                    FROM o32Contact_Medium o32 INNER JOIN p28Contact p28 ON o32.p28ID=p28.p28ID
                    LEFT OUTER JOIN p30ContactPerson p30 ON p30.p28ID_Person=p28.p28ID
                    WHERE o32.o33ID IN (2,21,22) AND LOWER(LTRIM(RTRIM(o32.o32Value)))=@mail AND p28.p28ValidUntil>GETDATE()", new { mail })?.ToList();
                if (hits == null || hits.Count == 0) return 0;
                var ids = hits.Select(p => p.p28ID).Concat(hits.Where(p => p.p28ID_Company > 0).Select(p => p.p28ID_Company)).Distinct().ToList();
                return SingleProject(f, ids);
            }
            catch
            {
                return 0;   //návrh projektu je jen pomůcka, formulář se otevře i bez něj
            }
        }

        private class ContactHit { public int p28ID { get; set; } public int p28ID_Company { get; set; } }

        // jediný otevřený projekt, kde je některý z kontaktů klientem nebo kontaktní osobou projektu
        private static int SingleProject(BL.Factory f, List<int> p28ids)
        {
            if (p28ids == null || p28ids.Count == 0) return 0;
            var lis = f.db.GetList<BO.GetInteger>(@"SELECT DISTINCT TOP 2 a.p41ID as Value
                FROM p41Project a INNER JOIN p07ProjectLevel p07 ON a.p07ID=p07.p07ID
                WHERE a.p41ValidUntil>GETDATE() AND p07.p07Level=5 AND p07.x01ID=@x01id
                AND (a.p28ID_Client IN @ids OR a.p41ID IN (SELECT p41ID FROM p26ProjectContact WHERE p28ID IN @ids))", new { ids = p28ids, x01id = f.CurrentUser.x01ID })?.ToList();
            return (lis != null && lis.Count == 1) ? lis[0].Value : 0;
        }
    }
}
