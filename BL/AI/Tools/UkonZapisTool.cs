using System.Globalization;
using System.Text.Json.Nodes;
using BL.AI.Core;

namespace BL.AI.Tools
{
    /// <summary>
    /// Nástroj „ukon_zapis“: založí jeden časový úkon (vykázané hodiny) přihlášeného uživatele.
    /// Ukládá stejnou cestou jako formulář úkonu – p31WorksheetBL.SaveOrigRecord (validace p31_test_beforesave: oprávnění,
    /// uzamčená období, limity, zaokrouhlení; sazby dopočítá uložení). Nástroj jen převádí vstup modelu a hlídá to,
    /// co formulář řeší nabídkou: projekt a aktivita musí být z nabídky uživatele, sešit musí být časový.
    /// Neumí: úkon za jinou osobu, výdaje, odměny, kusovník, opravu ani smazání úkonu.
    /// Dva režimy: přímý zápis (MCP – potvrzuje AI klient uživatele) a návrh (agent „Zeptej se MARKTIME“): nástroj vše ověří,
    /// návrh uloží do p85TempBox a úkon zapíše až <see cref="Confirm"/> po kliknutí uživatele na „Zapsat“.
    /// </summary>
    public class UkonZapisTool : IAiTool
    {
        /// <summary>p85Prefix uloženého návrhu úkonu.</summary>
        public const string ProposalPrefix = "aiukon";
        /// <summary>Jak dlouho lze návrh potvrdit.</summary>
        public const int ProposalHours = 24;

        /// <summary>Datum úkonu dál než tolik dní od dneška bere nástroj jako omyl modelu v roce.</summary>
        public const int MaxDaysFromToday = 366;

        private readonly BL.Factory _f;
        private readonly bool _propose;

        /// <param name="propose">true = úkon jen připravit k potvrzení uživatelem (viz <see cref="Proposals"/>), false = rovnou zapsat</param>
        public UkonZapisTool(BL.Factory f, bool propose = false) { _f = f; _propose = propose; }

        /// <summary>Návrhy připravené tímto nástrojem (režim návrhu) – UI je zobrazí jako karty s tlačítkem „Zapsat“.</summary>
        public List<UkonCard> Proposals { get; } = new();

        public string Name => "ukon_zapis";

        public string Description => _propose ? DescriptionPropose : DescriptionDirect;

        private const string DescriptionPropose =
            "MARKTIME: PŘIPRAVÍ jeden časový úkon (vykázané hodiny) přihlášeného uživatele k zápisu. Úkon se tímto voláním NEZAPÍŠE: " +
            "MARKTIME ho uživateli ukáže jako kartu s tlačítky „Zapsat“ a „Zrušit“ a zapíše ho až po jeho kliknutí. " +
            "Použij jen na výslovný pokyn uživatele vykázat / zapsat práci. projekt_id a aktivita_id ber z nástroje ukon_nabidka – nikdy je neodhaduj. " +
            "Jedno volání = jeden úkon v jednom dni; více dní nebo projektů = více volání. " +
            "Když chybí datum, hodiny, projekt nebo (u aktivit s povinným textem) popis práce, zeptej se uživatele dřív, než nástroj zavoláš. " +
            "Nástroj ověří všechna pravidla MARKTIME (oprávnění, uzamčená období, limity); chybu vrátí hned.";

        private const string DescriptionDirect =
            "MARKTIME: ZAPÍŠE do MARKTIME jeden časový úkon (vykázané hodiny) uživatele, jehož jménem pracuješ. " +
            "Použij jen na výslovný pokyn uživatele vykázat / zapsat práci. projekt_id a aktivita_id ber z nástroje ukon_nabidka – nikdy je neodhaduj. " +
            "Jedno volání = jeden úkon v jednom dni; více dní nebo projektů = více volání. " +
            "Když chybí datum, hodiny, projekt nebo (u aktivit s povinným textem) popis práce, zeptej se uživatele dřív, než nástroj zavoláš. " +
            "Vrátí zapsaný úkon tak, jak je uložen (hodiny mohou být zaokrouhleny podle nastavení MARKTIME). " +
            "Existující úkony nemění ani nemaže – opravu udělá uživatel v MARKTIME.";

        public JsonObject InputSchema => new()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["datum"] = new JsonObject { ["type"] = "string", ["format"] = "date", ["description"] = "Den, kdy byla práce odvedena (yyyy-MM-dd). „Dnes“, „včera“ přepočítej na konkrétní datum." },
                ["projekt_id"] = new JsonObject { ["type"] = "integer", ["description"] = "ID projektu z nástroje ukon_nabidka." },
                ["aktivita_id"] = new JsonObject { ["type"] = "integer", ["description"] = "ID aktivity z nástroje ukon_nabidka (pro daný projekt)." },
                ["sesit_id"] = new JsonObject { ["type"] = "integer", ["description"] = "Jen když se vykazuje bez aktivity: ID sešitu z nástroje ukon_nabidka. Jinak vynech." },
                ["hodiny"] = new JsonObject { ["type"] = "string", ["description"] = "Počet hodin: desetinně („1.5“) nebo jako HH:MM („1:30“). Lze vynechat, když jsou zadány cas_od a cas_do." },
                ["cas_od"] = new JsonObject { ["type"] = "string", ["description"] = "Nepovinný začátek práce HH:MM – jen když ho uživatel uvedl." },
                ["cas_do"] = new JsonObject { ["type"] = "string", ["description"] = "Nepovinný konec práce HH:MM – jen spolu s cas_od." },
                ["text"] = new JsonObject { ["type"] = "string", ["description"] = "Popis odvedené práce slovy uživatele (jde na výkaz a může jít klientovi na fakturu). Nic si nepřimýšlej." },
                ["text_interni"] = new JsonObject { ["type"] = "string", ["description"] = "Nepovinná interní poznámka (klient ji nevidí)." },
                ["ukol_id"] = new JsonObject { ["type"] = "integer", ["description"] = "Nepovinné: ID úkolu z nástroje ukon_nabidka, ke kterému práce patří." },
                ["povolit_duplicitu"] = new JsonObject { ["type"] = "boolean", ["description"] = "true jen tehdy, když nástroj ohlásil možnou duplicitu a uživatel výslovně potvrdil, že chce stejný úkon zapsat znovu." }
            },
            ["required"] = new JsonArray { "datum", "projekt_id" },
            ["additionalProperties"] = false
        };

        public AiToolOutput Execute(JsonNode input, AiToolContext ctx)
        {
            var u = _f.CurrentUser;
            if (!UkonSupport.HasModule(_f)) return AiToolOutput.Error("Uživatel nemá v MARKTIME přístup k úkonům (vykazování).");

            // ── vstup ──
            var errors = new List<string>();

            DateTime d = default;
            string sDate = AiToolJson.Str(input, "datum");
            if (sDate == null || !DateTime.TryParseExact(sDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                errors.Add("datum: zadej den ve tvaru yyyy-MM-dd.");
            else if (Math.Abs((d - DateTime.Today).TotalDays) > MaxDaysFromToday)
                errors.Add($"datum: {d:d.M.yyyy} je víc než rok od dneška ({DateTime.Today:d.M.yyyy}). Zkontroluj rok; úkon s takovým datem zapíše uživatel přímo v MARKTIME.");

            string from = null, until = null;
            string sFrom = AiToolJson.Str(input, "cas_od"), sUntil = AiToolJson.Str(input, "cas_do");
            if ((sFrom == null) != (sUntil == null)) errors.Add("cas_od a cas_do zadej oba, nebo žádný.");
            else if (sFrom != null)
            {
                if (!UkonSupport.TryParseClock(sFrom, out from)) errors.Add("cas_od: zadej čas ve tvaru HH:MM.");
                if (!UkonSupport.TryParseClock(sUntil, out until)) errors.Add("cas_do: zadej čas ve tvaru HH:MM.");
                if (from != null && from == until) errors.Add("cas_od a cas_do jsou stejné.");
            }

            int minutes = 0;
            string sHours = HoursText(input);
            if (sHours != null)
            {
                if (!UkonSupport.TryParseHours(sHours, out minutes)) errors.Add($"hodiny: „{sHours}“ není počet hodin. Zadej desetinně (1.5) nebo HH:MM (1:30), nejvýš 24 hodin.");
            }
            else if (sFrom == null && sUntil == null)
            {
                errors.Add("hodiny: chybí počet hodin (nebo cas_od a cas_do).");
            }
            if (errors.Count == 0 && from != null)
            {
                // čas od–do určuje hodiny (stejně jako formulář úkonu); přes půlnoc = do druhého dne
                int m1 = int.Parse(from.Substring(0, 2)) * 60 + int.Parse(from.Substring(3, 2));
                int m2 = int.Parse(until.Substring(0, 2)) * 60 + int.Parse(until.Substring(3, 2));
                int diff = m2 > m1 ? m2 - m1 : m2 + 24 * 60 - m1;
                if (minutes > 0 && minutes != diff) errors.Add($"hodiny ({UkonSupport.HHMM(minutes)}) neodpovídají času od–do ({from}–{until} = {UkonSupport.HHMM(diff)}). Zadej jen jedno z toho, nebo se zeptej uživatele, co platí.");
                minutes = diff;
            }

            int p41id = AiToolJson.Int(input, "projekt_id") ?? 0;
            int p32id = AiToolJson.Int(input, "aktivita_id") ?? 0;
            int p34id = AiToolJson.Int(input, "sesit_id") ?? 0;
            int p56id = AiToolJson.Int(input, "ukol_id") ?? 0;
            if (p41id <= 0) errors.Add("projekt_id: chybí. Najdi projekt nástrojem ukon_nabidka.");
            if (p32id <= 0 && p34id <= 0) errors.Add("aktivita_id: chybí. Aktivity projektu vrátí nástroj ukon_nabidka s projekt_id.");
            if (errors.Count > 0) return AiToolOutput.Error(errors);

            string text = AiToolJson.Str(input, "text");
            string textInternal = AiToolJson.Str(input, "text_interni");

            // ── projekt, aktivita, sešit: jen z nabídky uživatele ──
            var recP41 = UkonSupport.Project(_f, p41id);
            if (recP41 == null) return AiToolOutput.Error($"Projekt s ID {p41id} neexistuje, nebo do něj uživatel nesmí vykazovat hodiny. Najdi projekt nástrojem ukon_nabidka.");
            if (UkonSupport.IsHoursForbidden(recP41)) return AiToolOutput.Error($"V projektu „{UkonSupport.ProjectName(recP41)}“ platí zákaz vykazovat hodiny.");
            if (UkonSupport.IsTaskRequired(recP41) && p56id <= 0)
                return AiToolOutput.Error($"V projektu „{UkonSupport.ProjectName(recP41)}“ lze hodiny vykázat jen přes úkol (ukol_id). Úkoly uživatele vrátí nástroj ukon_nabidka s projekt_id {p41id}; zeptej se, ke kterému práce patří.");
            var sheets = UkonSupport.TimeSheets(_f, recP41);

            BO.p32Activity recP32 = null;
            if (p32id > 0)
            {
                recP32 = _f.p32ActivityBL.Load(p32id);
                if (recP32 == null) return AiToolOutput.Error($"Aktivita s ID {p32id} neexistuje. Aktivity projektu vrátí nástroj ukon_nabidka s projekt_id {p41id}.");
                if (p34id > 0 && p34id != recP32.p34ID) return AiToolOutput.Error("sesit_id neodpovídá sešitu aktivity. Parametr sesit_id vynech.");
                if (UkonSupport.IsManualFee(recP32)) return AiToolOutput.Error($"Aktivita „{recP32.p32Name}“ vyžaduje ručně zadanou částku honoráře – takový úkon zapíše uživatel přímo v MARKTIME.");
                p34id = recP32.p34ID;
            }
            var recP34 = sheets.FirstOrDefault(p => p.pid == p34id);
            if (recP34 == null)
                return AiToolOutput.Error($"{(p32id > 0 ? $"Aktivitu „{recP32.p32Name}“" : $"Sešit s ID {p34id}")} nelze v projektu „{UkonSupport.ProjectName(recP41)}“ vykázat jako hodiny. Nabídku aktivit vrátí nástroj ukon_nabidka s projekt_id {p41id}.");
            if (p32id == 0 && recP34.p34ActivityEntryFlag == BO.p34ActivityEntryFlagENUM.AktivitaJePovinna)
                return AiToolOutput.Error($"Sešit „{recP34.p34Name}“ vyžaduje aktivitu. Aktivity projektu vrátí nástroj ukon_nabidka s projekt_id {p41id}.");
            if (recP32 != null && recP32.p32IsTextRequired && text == null)
                return AiToolOutput.Error($"Aktivita „{recP32.p32Name}“ vyžaduje popis práce (text). Zeptej se uživatele, co má na výkazu být.");

            if (recP32 == null)
            {
                // sešit bez povinné aktivity: výchozí aktivita sešitu (první v pořadí) – stejně jako p31WorksheetBL.SaveOrigRecord
                recP32 = _f.p32ActivityBL.GetList(new BO.myQueryP32 { p34id = p34id, IsRecordValid = true })?.OrderBy(p => p.p32Ordinary).FirstOrDefault();
                if (recP32 != null) p32id = recP32.pid;
            }

            BO.p56Task recP56 = null;
            if (p56id > 0)
            {
                recP56 = _f.p56TaskBL.Load(p56id);
                if (recP56 == null || recP56.p41ID != p41id) return AiToolOutput.Error($"Úkol s ID {p56id} nepatří k projektu „{UkonSupport.ProjectName(recP41)}“. Parametr ukol_id vynech, nebo vyber úkol z nástroje ukon_nabidka.");
            }

            // ── stejný úkon už existuje? (model nebo klient mohl volání zopakovat) ──
            if (AiToolJson.Bool(input, "povolit_duplicitu") != true)
            {
                var same = FindSame(d, p41id, p32id, minutes, text);
                if (same != null)
                    return AiToolOutput.Error($"Stejný úkon už v MARKTIME je (ID {same.pid}: {UkonSupport.DateText(d)}, {UkonSupport.ProjectName(recP41)}, {same.p32Name}, {UkonSupport.HHMM(same.p31Minutes_Orig)}, založen {same.DateInsert:d.M.yyyy HH:mm}). " +
                                              "Nic nebylo zapsáno. Řekni to uživateli; znovu zapiš (povolit_duplicitu = true) jen když to výslovně potvrdí.");
            }

            // ── uložení ──
            bool allowDup = AiToolJson.Bool(input, "povolit_duplicitu") == true;
            var rec = new BO.p31WorksheetEntryInput
            {
                j02ID = u.pid,
                p41ID = p41id,
                p34ID = p34id,
                p32ID = p32id,
                p56ID = p56id,
                p31Text = text,
                p31TextInternal = textInternal,
                p31HoursEntryflag = BO.p31HoursEntryFlagENUM.Hodiny,
                Value_Orig = UkonSupport.HHMM(minutes),     // HH:MM – desetinný zápis by server četl podle svého národního nastavení
                TimeFrom = from ?? "",
                TimeUntil = until ?? ""
            };
            rec.Addp31Date(d);

            if (_propose)
                return Propose(rec, d, minutes, from, until, allowDup, recP41, recP32, recP34, recP56);

            u.Messages4Notify = null;
            int pid;
            try
            {
                pid = _f.p31WorksheetBL.SaveOrigRecord(rec, BO.p33IdENUM.Cas, null);
            }
            catch (Exception ex)
            {
                u.Messages4Notify = null;
                return AiToolOutput.Error("Úkon se nepodařilo uložit: " + ex.Message);
            }
            var messages = u.Messages4Notify?.Select(p => p.Value).Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList() ?? new List<string>();
            u.Messages4Notify = null;

            if (pid <= 0)
                return AiToolOutput.Error("MARKTIME úkon nezapsal: " + (messages.Count > 0 ? string.Join(" ", messages) : rec.ErrorMessage ?? "neznámá chyba.") +
                                          " Nic nebylo uloženo. Když jde o pravidlo MARKTIME (uzamčené období, limit hodin, oprávnění), vysvětli to uživateli a nezkoušej to obejít jiným datem nebo projektem.");

            // ── výsledek: úkon tak, jak je uložen ──
            var saved = _f.p31WorksheetBL.Load(pid);
            var o = new JsonObject
            {
                ["zapsano"] = true,
                ["ukon_id"] = pid,
                ["datum"] = UkonSupport.DateText(d),
                ["osoba"] = $"{u.j02FirstName} {u.j02LastName}".Trim(),
                ["projekt"] = UkonSupport.ProjectName(recP41),
                ["klient"] = recP41.Client,
                ["aktivita"] = saved?.p32Name ?? recP32?.p32Name,
                ["sesit"] = recP34.p34Name,
                ["hodiny"] = UkonSupport.HHMM(saved?.p31Minutes_Orig ?? minutes),
                ["text"] = saved?.p31Text ?? text
            };
            if (from != null) o["cas"] = $"{from}–{until}";
            if (saved != null && saved.p31Minutes_Orig != minutes) o["poznamka"] = $"MARKTIME hodiny zaokrouhlil: zadáno {UkonSupport.HHMM(minutes)}, uloženo {UkonSupport.HHMM(saved.p31Minutes_Orig)}.";
            if (messages.Count > 0) o["zpravy_marktime"] = new JsonArray(messages.Select(p => (JsonNode)p).ToArray());
            o["pokyn"] = "Potvrď uživateli, co bylo zapsáno (den, projekt, aktivita, hodiny, text). ukon_id mu neukazuj.";

            var card = Card(d, saved?.p31Minutes_Orig ?? minutes, from, until, saved?.p31Text ?? text, textInternal, recP41, saved?.p32Name ?? recP32?.p32Name, recP34, recP56);
            card.Pid = pid;
            card.Poznamka = o["poznamka"]?.GetValue<string>();
            return new AiToolOutput { Content = o.ToJsonString(_json), Rows = 1, SavedPid = pid, SavedEntity = "p31", Data = card };
        }

        // ------------------------------------------------------------------ návrh a jeho potvrzení (agent „Zeptej se MARKTIME“)

        /// <summary>
        /// Režim návrhu: ověří úkon pravidly MARKTIME (p31_test_beforesave), spočítá zaokrouhlení a návrh uloží do p85TempBox.
        /// Nic nezapisuje do p31.
        /// </summary>
        private AiToolOutput Propose(BO.p31WorksheetEntryInput rec, DateTime d, int minutes, string from, string until, bool allowDup,
            BO.p41Project recP41, BO.p32Activity recP32, BO.p34ActivityGroup recP34, BO.p56Task recP56)
        {
            int saveMinutes = minutes;
            try
            {
                var vld = _f.p31WorksheetBL.ValidateBeforeSaveOrigRecord(rec)?.FirstOrDefault();
                if (vld == null) return AiToolOutput.Error("Úkon se nepodařilo ověřit.");
                if (!string.IsNullOrEmpty(vld.ErrorMessage))
                    return AiToolOutput.Error("MARKTIME by úkon nezapsal: " + vld.ErrorMessage + " Návrh nevznikl. Když jde o pravidlo MARKTIME (uzamčené období, limit hodin, oprávnění), vysvětli to uživateli a nezkoušej to obejít jiným datem nebo projektem.");
                if (!rec.ValidateEntryTime(vld.Round2Minutes, d)) return AiToolOutput.Error("MARKTIME by úkon nezapsal: " + rec.ErrorMessage);
                saveMinutes = rec.p31Minutes_Orig;      // po zaokrouhlení podle nastavení projektu / klienta / licence
            }
            catch (Exception ex)
            {
                return AiToolOutput.Error("Úkon se nepodařilo ověřit: " + ex.Message);
            }

            // uložený návrh = vstup pro přímý zápis; Confirm ho provede znovu se všemi kontrolami
            var stored = new JsonObject
            {
                ["datum"] = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["projekt_id"] = rec.p41ID,
                ["hodiny"] = UkonSupport.HHMM(minutes)
            };
            if (rec.p32ID > 0) stored["aktivita_id"] = rec.p32ID; else stored["sesit_id"] = rec.p34ID;
            if (from != null) { stored["cas_od"] = from; stored["cas_do"] = until; }
            if (rec.p31Text != null) stored["text"] = rec.p31Text;
            if (rec.p31TextInternal != null) stored["text_interni"] = rec.p31TextInternal;
            if (rec.p56ID > 0) stored["ukol_id"] = rec.p56ID;
            if (allowDup) stored["povolit_duplicitu"] = true;

            string guid = BO.Code.Bas.GetGuid();
            int p85id = _f.p85TempboxBL.Save(new BO.p85Tempbox
            {
                p85GUID = guid,
                p85Prefix = ProposalPrefix,
                p85OtherKey1 = _f.CurrentUser.pid,
                p85FreeDate01 = DateTime.Now,
                p85Message = stored.ToJsonString(_json)
            });
            if (p85id <= 0) return AiToolOutput.Error("Návrh úkonu se nepodařilo uložit.");

            var card = Card(d, saveMinutes, from, until, rec.p31Text, rec.p31TextInternal, recP41, recP32?.p32Name, recP34, recP56);
            card.Guid = guid;
            if (saveMinutes != minutes) card.Poznamka = $"MARKTIME hodiny zaokrouhlí: zadáno {UkonSupport.HHMM(minutes)}, uloží se {UkonSupport.HHMM(saveMinutes)}.";
            Proposals.Add(card);

            var o = new JsonObject
            {
                ["zapsano"] = false,
                ["navrh_pripraven"] = true,
                ["datum"] = card.Datum,
                ["projekt"] = card.Projekt,
                ["klient"] = card.Klient,
                ["aktivita"] = card.Aktivita,
                ["hodiny"] = card.Hodiny,
                ["text"] = card.Text
            };
            if (card.Cas != null) o["cas"] = card.Cas;
            if (card.Poznamka != null) o["poznamka"] = card.Poznamka;
            o["pokyn"] = "Úkon ZATÍM NENÍ zapsán. MARKTIME ukáže uživateli pod tvou odpovědí kartu s návrhem a tlačítky „Zapsat“ a „Zrušit“. " +
                         "V odpovědi jednou větou řekni, že je návrh připraven a že ho potvrdí tlačítkem Zapsat; údaje z karty neopakuj a netvrď, že je zapsáno.";
            return new AiToolOutput { Content = o.ToJsonString(_json), Rows = 1, Data = card };
        }

        /// <summary>
        /// Uživatel potvrdil návrh tlačítkem „Zapsat“: úkon se zapíše přímým režimem nástroje (znovu se vším ověřením).
        /// Návrh patří uživateli, který ho vytvořil, a lze ho použít jednou. Při úspěchu je v Data zapsaný úkon (<see cref="UkonCard"/>).
        /// </summary>
        public static AiToolOutput Confirm(BL.Factory f, string guid)
        {
            var p85 = LoadProposal(f, guid);
            if (p85 == null) return AiToolOutput.Error("Návrh úkonu už neplatí (byl zapsán, zrušen, nebo vypršel). Požádejte asistenta o nový.");

            JsonNode input;
            try { input = JsonNode.Parse(p85.p85Message); }
            catch { return AiToolOutput.Error("Návrh úkonu nelze přečíst."); }

            f.p85TempboxBL.VirtualDeleteByGuid(guid);       // jedno použití – druhé kliknutí už návrh nenajde
            var o = new UkonZapisTool(f, false).Execute(input, new AiToolContext { F = f });
            if (o.IsError)
            {
                // nezapsáno → návrh zůstává k dispozici (uživatel může zkusit znovu, např. po odemčení období)
                try { f.db.RunSql("UPDATE p85Tempbox SET p85IsDeleted=0 WHERE p85GUID=@guid AND p85Prefix=@prefix", new { guid, prefix = ProposalPrefix }); } catch { }
            }
            return o;
        }

        /// <summary>Uživatel návrh zrušil.</summary>
        public static void Cancel(BL.Factory f, string guid)
        {
            if (LoadProposal(f, guid) != null) f.p85TempboxBL.VirtualDeleteByGuid(guid);
        }

        private static BO.p85Tempbox LoadProposal(BL.Factory f, string guid)
        {
            if (string.IsNullOrEmpty(guid) || guid.Length > 50) return null;
            var rec = f.p85TempboxBL.LoadByGuid(guid);
            if (rec == null || rec.p85IsDeleted || rec.p85Prefix != ProposalPrefix || rec.p85OtherKey1 != f.CurrentUser.pid || string.IsNullOrEmpty(rec.p85Message)) return null;
            if (rec.p85FreeDate01 == null || rec.p85FreeDate01.Value.AddHours(ProposalHours) < DateTime.Now) return null;
            return rec;
        }

        private UkonCard Card(DateTime d, int minutes, string from, string until, string text, string textInternal,
            BO.p41Project recP41, string activity, BO.p34ActivityGroup recP34, BO.p56Task recP56)
        {
            return new UkonCard
            {
                Datum = UkonSupport.DateText(d),
                Projekt = UkonSupport.ProjectName(recP41),
                Klient = recP41.Client,
                Aktivita = activity,
                Sesit = recP34.p34Name,
                Hodiny = UkonSupport.HHMM(minutes),
                Cas = from != null ? $"{from}–{until}" : null,
                Text = text,
                TextInterni = textInternal,
                Ukol = recP56?.p56Name
            };
        }

        /// <summary>Hodiny smí model poslat jako text i jako číslo.</summary>
        private static string HoursText(JsonNode input)
        {
            if (input?["hodiny"] is not JsonValue v) return null;
            if (v.TryGetValue<string>(out var s)) return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
            if (v.TryGetValue<double>(out var d)) return d.ToString(CultureInfo.InvariantCulture);
            return null;
        }

        /// <summary>Úkon uživatele ve stejný den, projektu a aktivitě se stejným textem a (po zaokrouhlení) stejnými hodinami.</summary>
        private BO.p31Worksheet FindSame(DateTime d, int p41id, int p32id, int minutes, string text)
        {
            IEnumerable<BO.p31Worksheet> lis;
            try
            {
                lis = _f.p31WorksheetBL.GetList(new BO.myQueryP31 { j02id = _f.CurrentUser.pid, p41id = p41id, global_d1 = d, global_d2 = d, period_field = "p31Date" });
            }
            catch
            {
                return null;    // kontrola duplicity nesmí zápis zablokovat
            }
            string t = Norm(text);
            return lis?.FirstOrDefault(p => p.p31Date.Date == d.Date && (p32id == 0 || p.p32ID == p32id) && Norm(p.p31Text) == t
                                            && p.p31Minutes_Orig >= minutes && p.p31Minutes_Orig - minutes < 60);
        }

        private static string Norm(string s) => (s ?? "").Trim().ToLowerInvariant();

        private static readonly System.Text.Json.JsonSerializerOptions _json = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()     // bez něj JsonNode.ToJsonString(options) v .NET 8 vyhodí výjimku
        };
    }

    /// <summary>Úkon pro zobrazení v UI: návrh k potvrzení (Guid) nebo zapsaný úkon (Pid).</summary>
    public class UkonCard
    {
        /// <summary>Návrh: klíč pro potvrzení (p85GUID).</summary>
        public string Guid { get; set; }
        /// <summary>Zapsaný úkon: p31ID.</summary>
        public int Pid { get; set; }
        public string Datum { get; set; }
        public string Projekt { get; set; }
        public string Klient { get; set; }
        public string Aktivita { get; set; }
        public string Sesit { get; set; }
        public string Hodiny { get; set; }
        public string Cas { get; set; }
        public string Text { get; set; }
        public string TextInterni { get; set; }
        public string Ukol { get; set; }
        public string Poznamka { get; set; }
    }
}
