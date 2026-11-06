namespace BL.AI.Core
{
    /// <summary>Jeden záznam do j98OpenAILog (jeden dotaz uživatele včetně případných opravných volání).</summary>
    public class AiLogEntry
    {
        public string Feature { get; set; }
        public string ContextPage { get; set; }
        public string ConversationGuid { get; set; }      // → j98Guid
        public int? TurnIndex { get; set; }
        public string QueryText { get; set; }             // → j98Question
        public string AnswerText { get; set; }            // → j98Answer
        public string AnswerJson { get; set; }
        public char Status { get; set; } = 'O';           // O ok, V validační chyby, E chyba
        public string ErrorText { get; set; }
        public int RepairCount { get; set; }
        public string AiModel { get; set; }
        public string StopReason { get; set; }
        public LlmUsage Usage { get; set; } = new();
        public long DurationMs { get; set; }
        public string ExtraJson { get; set; }

        /// <summary>Připočte tokeny a čas z dalšího volání modelu.</summary>
        public void Add(LlmResult r)
        {
            if (r == null) return;
            AiModel = r.Model;
            StopReason = r.StopReason;
            Usage.InputTokens += r.Usage.InputTokens;
            Usage.OutputTokens += r.Usage.OutputTokens;
            Usage.CacheReadTokens += r.Usage.CacheReadTokens;
            Usage.CacheWriteTokens += r.Usage.CacheWriteTokens;
            DurationMs += r.ElapsedMs;
        }
    }

    /// <summary>Zápis do tabulky j98OpenAILog (rozšířené skriptem SQL/j98OpenAILog_upgrade.sql). Chyba logování nikdy neshodí AI funkci.</summary>
    public static class AiLog
    {
        /// <returns>j98ID nebo 0</returns>
        public static long Insert(BL.Factory f, AiLogEntry e)
        {
            try
            {
                return f.db.Load<long>(@"INSERT INTO j98OpenAILog(j98Date,j02ID,j98Login,j98Feature,j98ContextPage,j98Guid,j98TurnIndex,
j98Question,j98Answer,j98AnswerJson,j98Status,j98ErrorText,j98RepairCount,j98AiModel,j98StopReason,
j98InputTokens,j98OutputTokens,j98CacheReadTokens,j98CacheWriteTokens,j98DurationMs,j98ExtraJson)
VALUES(GETDATE(),@j02id,@login,@feature,@page,@conv,@turn,@q,@a,@json,@status,@err,@repair,@model,@stop,@tin,@tout,@tcr,@tcw,@ms,@extra);
SELECT CAST(SCOPE_IDENTITY() AS bigint)",
                    new
                    {
                        j02id = f.CurrentUser.pid,
                        login = Cut(f.CurrentUser.j02Login, 100),
                        feature = Cut(e.Feature, 50),
                        page = Cut(e.ContextPage, 200),
                        conv = Cut(e.ConversationGuid, 50),
                        turn = e.TurnIndex,
                        q = Cut(e.QueryText ?? "", 2000),
                        a = e.AnswerText,
                        json = e.AnswerJson,
                        status = e.Status.ToString(),
                        err = Cut(e.ErrorText, 1000),
                        repair = (byte)Math.Min(e.RepairCount, 255),
                        model = Cut(e.AiModel, 100),
                        stop = Cut(e.StopReason, 30),
                        tin = e.Usage.InputTokens,
                        tout = e.Usage.OutputTokens,
                        tcr = e.Usage.CacheReadTokens,
                        tcw = e.Usage.CacheWriteTokens,
                        ms = (int)Math.Min(e.DurationMs, int.MaxValue),
                        extra = e.ExtraJson
                    });
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>Zaznamená, co uživatel z návrhu uložil (míra přijetí návrhů).</summary>
        public static void SetResult(BL.Factory f, long logId, string entity, int pid)
        {
            if (logId <= 0) return;
            try
            {
                f.db.RunSql("UPDATE j98OpenAILog SET j98ResultEntity=@entity,j98ResultPid=@pid WHERE j98ID=@id AND j02ID=@j02id",
                    new { entity, pid, id = logId, j02id = f.CurrentUser.pid });
            }
            catch { }
        }

        /// <summary>Hodnocení odpovědi uživatelem (1 / -1). Uživatel může hodnotit jen vlastní záznamy.</summary>
        public static bool Rate(BL.Factory f, long logId, int rating, string comment)
        {
            if (logId <= 0 || (rating != 1 && rating != -1)) return false;
            try
            {
                return f.db.RunSql("UPDATE j98OpenAILog SET j98Rating=@rating,j98RatingComment=@comment WHERE j98ID=@id AND j02ID=@j02id",
                    new { rating = (short)rating, comment = Cut(comment, 1000), id = logId, j02id = f.CurrentUser.pid });
            }
            catch { return false; }
        }

        private static string Cut(string s, int len) => s == null ? null : (s.Length > len ? s.Substring(0, len) : s);
    }
}
