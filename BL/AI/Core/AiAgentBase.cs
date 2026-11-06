using System.Text.Json;
using System.Text.Json.Nodes;

namespace BL.AI.Core
{
    /// <summary>Společný výsledek dotazu na agenta. Agent si jej rozšíří o vlastní návrh.</summary>
    public class AiAskResult
    {
        public bool Ok { get; set; }
        public string Error { get; set; }
        /// <summary>Text pro uživatele.</summary>
        public string Reply { get; set; }
        /// <summary>Odpověď modelu (JSON) – prohlížeč ji vrací v historii konverzace.</summary>
        public string Raw { get; set; }
        /// <summary>j98ID – pro hodnocení a vazbu na uložený výsledek.</summary>
        public long LogId { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Základ AI agenta: sestavení požadavku, volání modelu se strukturovaným výstupem,
    /// jedna automatická oprava nevalidní odpovědi a zápis do j98OpenAILog.
    /// Konkrétní agent dodá prompt, JSON schéma a zpracování/validaci odpovědi.
    /// </summary>
    /// <typeparam name="TAnswer">Třída odpovídající JSON schématu agenta.</typeparam>
    /// <typeparam name="TResult">Výsledek vracený do UI.</typeparam>
    public abstract class AiAgentBase<TAnswer, TResult> where TResult : AiAskResult, new()
    {
        protected readonly BL.Factory F;
        protected readonly ILlmClient Llm;

        protected AiAgentBase(BL.Factory f, ILlmClient llm) { F = f; Llm = llm; }

        /// <summary>Kód agenta do j98Feature, např. "ai-totals".</summary>
        protected abstract string Feature { get; }

        /// <summary>Max. počet zpráv historie posílaných modelu.</summary>
        protected virtual int MaxHistory => 20;

        public virtual bool IsEnabled => Llm.Options.Enabled;

        /// <summary>System prompt: statická část s Cache = true na posledním cachovaném bloku, dynamická část za ním.</summary>
        protected abstract List<LlmSystemBlock> BuildSystem();

        protected abstract JsonObject BuildSchema();

        /// <summary>Možnost doplnit poslední zprávu uživatele o kontext (např. aktuální stav formuláře).</summary>
        protected virtual string DecorateLastUserMessage(string content, BO.AI.AiAskInput input) => content;

        /// <summary>
        /// Zpracuje odpověď modelu a naplní výsledek (Reply, návrh…).
        /// Vrací validační chyby – prázdný seznam = odpověď je v pořádku. Při chybách proběhne jedna oprava.
        /// </summary>
        protected abstract List<string> ProcessAnswer(TAnswer answer, TResult result);

        public async Task<TResult> AskAsync(BO.AI.AiAskInput input, CancellationToken ct = default)
        {
            var res = new TResult();
            if (!IsEnabled) { res.Error = "AI není zapnuté."; return res; }
            if (input?.Messages == null || !input.Messages.Any(p => p.Role == "user")) { res.Error = "Chybí dotaz."; return res; }

            var req = new LlmRequest { Schema = BuildSchema(), System = BuildSystem() };

            var msgs = input.Messages.TakeLast(MaxHistory).ToList();
            for (int i = 0; i < msgs.Count; i++)
            {
                string content = msgs[i].Content;
                if (i == msgs.Count - 1 && msgs[i].Role == "user") content = DecorateLastUserMessage(content, input);
                req.Messages.Add(new LlmMessage { Role = msgs[i].Role, Content = content });
            }

            var log = new AiLogEntry
            {
                Feature = Feature,
                ContextPage = input.ContextPage,
                ConversationGuid = input.ConversationGuid,
                TurnIndex = input.Messages.Count(p => p.Role == "user"),
                QueryText = msgs.Last(p => p.Role == "user").Content
            };

            await RunAsync(req, res, log, ct);

            log.AnswerText = res.Reply;
            if (!res.Ok) { log.Status = 'E'; log.ErrorText = res.Error; }
            res.LogId = AiLog.Insert(F, log);
            return res;
        }

        private async Task RunAsync(LlmRequest req, TResult res, AiLogEntry log, CancellationToken ct)
        {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                var r = await Llm.CompleteJsonAsync(req, ct);
                log.Add(r);
                log.AnswerJson = r.Json;
                if (!r.Ok) { res.Error = r.Error; return; }

                TAnswer ans;
                try { ans = JsonSerializer.Deserialize<TAnswer>(r.Json); }
                catch (Exception ex) { res.Error = "Nelze zpracovat odpověď AI: " + ex.Message; return; }

                res.Raw = r.Json;
                res.Warnings = new();
                var errors = ProcessAnswer(ans, res) ?? new();
                res.Ok = true;
                if (errors.Count == 0) return;

                if (attempt == 1)
                {
                    // vrátit aspoň částečný výsledek, uživatel jej opraví ve formuláři
                    res.Warnings = errors.Concat(res.Warnings).ToList();
                    log.Status = 'V';
                    log.ErrorText = string.Join("; ", errors);
                    return;
                }

                log.RepairCount++;
                req.Messages.Add(new LlmMessage { Role = "assistant", Content = r.Json });
                req.Messages.Add(new LlmMessage { Role = "user", Content = "Odpověď obsahuje chyby, oprav je a vrať celou odpověď znovu:\n- " + string.Join("\n- ", errors) });
            }
        }

        /// <summary>Zapíše k dotazu, co uživatel z návrhu uložil (např. "j79", ID šablony).</summary>
        protected void LogResult(long logId, string entity, int pid) => AiLog.SetResult(F, logId, entity, pid);
    }
}
