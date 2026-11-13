using System.Text;
using System.Text.Json.Nodes;
using BL.AI.Core;

namespace BL.AI.Tools
{
    /// <summary>
    /// Nástroj „ukon_nabidka“: příprava na zápis hodin. Bez projekt_id hledá projekty, do kterých smí uživatel vykazovat hodiny
    /// (bez hledaného textu vrátí naposledy vykazované). S projekt_id vrátí časové sešity a aktivity projektu a otevřené úkoly uživatele.
    /// Vrací ID, která pak model předá nástroji ukon_zapis. Jen čte.
    /// </summary>
    public class UkonNabidkaTool : IAiTool
    {
        private readonly BL.Factory _f;

        public UkonNabidkaTool(BL.Factory f) { _f = f; }

        public string Name => "ukon_nabidka";

        public string Description =>
            "MARKTIME: nabídka pro vykázání hodin (příprava pro nástroj ukon_zapis). " +
            "Krok 1 – najdi projekt: zadej `projekt` (část názvu nebo kódu projektu, nebo název klienta); bez něj vrátí projekty, do kterých uživatel vykazoval naposledy. " +
            "Krok 2 – zadej `projekt_id`: vrátí aktivity, které lze v projektu vykázat (aktivita_id), a otevřené úkoly uživatele v projektu (ukol_id). " +
            "Vrací jen projekty, do kterých smí uživatel vykazovat hodiny. Data nemění.";

        public JsonObject InputSchema => new()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["projekt"] = new JsonObject { ["type"] = "string", ["description"] = "Hledaný text: část názvu nebo kódu projektu, nebo název klienta. Jedno slovo je lepší než celá věta." },
                ["projekt_id"] = new JsonObject { ["type"] = "integer", ["description"] = "ID projektu z kroku 1 – vrátí aktivity a úkoly projektu." }
            },
            ["additionalProperties"] = false
        };

        public AiToolOutput Execute(JsonNode input, AiToolContext ctx)
        {
            if (!UkonSupport.HasModule(_f)) return AiToolOutput.Error("Uživatel nemá v MARKTIME přístup k úkonům (vykazování).");

            int p41id = AiToolJson.Int(input, "projekt_id") ?? 0;
            return p41id > 0 ? ProjectDetail(p41id) : ProjectSearch(AiToolJson.Str(input, "projekt"));
        }

        private AiToolOutput ProjectSearch(string search)
        {
            var sb = new StringBuilder();
            if (search == null)
            {
                var top = _f.p41ProjectBL.GetList_MyTop10(_f.CurrentUser.pid, 15)?.ToList() ?? new List<BO.p41MyTop10>();
                if (top.Count == 0) return new AiToolOutput { Content = "Uživatel v posledních 12 měsících nevykázal žádný úkon. Zeptej se ho na projekt a zavolej nástroj znovu s parametrem projekt." };
                sb.AppendLine("# NAPOSLEDY VYKAZOVANÉ PROJEKTY (projekt_id | klient - projekt | naposledy)");
                foreach (var p in top) sb.AppendLine($"{p.p41ID} | {p.Project} | {p.LastDate:d.M.yyyy}");
                sb.AppendLine();
                sb.Append("Jiný projekt najdeš parametrem projekt. Pro aktivity projektu zavolej nástroj znovu s projekt_id.");
                return new AiToolOutput { Content = sb.ToString(), Rows = top.Count };
            }

            var lis = UkonSupport.Projects(_f, search, UkonSupport.ProjectLimit + 1);
            if (lis.Count == 0)
                return new AiToolOutput { Content = $"Hledanému textu „{search}“ neodpovídá žádný projekt, do kterého smí uživatel vykazovat hodiny. Zkus kratší text (jedno slovo z názvu projektu nebo klienta), nebo se zeptej uživatele." };

            sb.AppendLine("# PROJEKTY PRO VYKAZOVÁNÍ HODIN (projekt_id | kód | projekt | klient)");
            foreach (var p in lis.Take(UkonSupport.ProjectLimit)) sb.AppendLine($"{p.pid} | {p.p41Code} | {UkonSupport.ProjectName(p)} | {p.Client}");
            sb.AppendLine();
            if (lis.Count > UkonSupport.ProjectLimit) sb.AppendLine($"Zobrazeno prvních {UkonSupport.ProjectLimit} projektů – upřesni hledaný text.");
            sb.Append(lis.Count == 1
                ? "Pro aktivity projektu zavolej nástroj znovu s projekt_id."
                : "Když není jisté, který projekt uživatel myslí, zeptej se ho. Pro aktivity projektu zavolej nástroj znovu s projekt_id.");
            return new AiToolOutput { Content = sb.ToString(), Rows = Math.Min(lis.Count, UkonSupport.ProjectLimit) };
        }

        private AiToolOutput ProjectDetail(int p41id)
        {
            var recP41 = UkonSupport.Project(_f, p41id);
            if (recP41 == null) return AiToolOutput.Error($"Projekt s ID {p41id} neexistuje, nebo do něj uživatel nesmí vykazovat hodiny. Najdi projekt parametrem projekt.");

            if (UkonSupport.IsHoursForbidden(recP41)) return AiToolOutput.Error($"V projektu „{UkonSupport.ProjectName(recP41)}“ platí zákaz vykazovat hodiny.");

            var sheets = UkonSupport.TimeSheets(_f, recP41);
            if (sheets.Count == 0) return AiToolOutput.Error($"Projekt „{UkonSupport.ProjectName(recP41)}“ nenabízí žádný sešit pro vykazování hodin.");
            var acts = UkonSupport.Activities(_f, recP41, sheets).Where(p => !UkonSupport.IsManualFee(p)).ToList();     // ruční honorář zadá uživatel ve formuláři úkonu

            var sb = new StringBuilder();
            sb.AppendLine($"# PROJEKT {recP41.pid}: {UkonSupport.ProjectName(recP41)} ({recP41.p41Code}), klient: {recP41.Client}");
            if (UkonSupport.IsTaskRequired(recP41)) sb.AppendLine("V tomto projektu lze hodiny vykázat jen přes úkol: ukol_id je povinný.");
            sb.AppendLine();
            sb.AppendLine("# AKTIVITY (aktivita_id | aktivita | sešit | fakturovatelná | text úkonu)");
            foreach (var s in sheets)
            {
                if (s.p34ActivityEntryFlag == BO.p34ActivityEntryFlagENUM.AktivitaSeNezadava) continue;
                foreach (var a in acts.Where(p => p.p34ID == s.pid))
                    sb.AppendLine($"{a.pid} | {a.p32Name} | {s.p34Name} | {(a.p32IsBillable ? "ano" : "ne")} | {(a.p32IsTextRequired ? "povinný" : "nepovinný")}");
            }
            var noAct = sheets.Where(p => p.p34ActivityEntryFlag != BO.p34ActivityEntryFlagENUM.AktivitaJePovinna).ToList();
            if (noAct.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("# SEŠITY, KDE LZE VYKÁZAT BEZ AKTIVITY (sesit_id | sešit)");
                foreach (var s in noAct) sb.AppendLine($"{s.pid} | {s.p34Name}");
            }

            if (_f.CurrentUser.j04IsModule_p56)
            {
                var tasks = _f.p56TaskBL.GetList(new BO.myQueryP56 { p41id = recP41.pid, j02id = _f.CurrentUser.pid })?.Take(30).ToList();
                if (tasks != null && tasks.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine($"# OTEVŘENÉ ÚKOLY UŽIVATELE V PROJEKTU (ukol_id | úkol | termín) {(UkonSupport.IsTaskRequired(recP41) ? "" : " – nepovinné, jen když uživatel úkol zmíní")}");
                    foreach (var t in tasks) sb.AppendLine($"{t.pid} | {t.p56Name} | {(t.p56PlanUntil == null ? "-" : t.p56PlanUntil.Value.ToString("d.M.yyyy"))}");
                }
            }

            sb.AppendLine();
            sb.Append("Aktivitu vyber podle toho, co uživatel dělal. Když jich připadá v úvahu víc a z popisu práce to nejde poznat, zeptej se.");
            return new AiToolOutput { Content = sb.ToString(), Rows = acts.Count };
        }
    }
}
