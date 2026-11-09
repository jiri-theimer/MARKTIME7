using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Admin
{
    public class AdminHome: BaseViewModel
    {
        public string IISBinding { get; set; }
        public IEnumerable<BO.p07ProjectLevel> lisP07 { get; set; }

        public BO.x01License RecX01 { get; set; }

        /// <summary>Efektivní AI konfigurace pro tuto licenci (tatáž, kterou používají AI funkce).</summary>
        public BL.AI.Core.AiOptions AiOptions { get; set; }
        /// <summary>Využití AI za posledních 30 dní z j98OpenAILog; null = tabulka nedostupná.</summary>
        public AdminAiUsage AiUsage { get; set; }
        public IEnumerable<AdminAiFeature> AiFeatures { get; set; }
    }

    public class AdminAiUsage
    {
        public int Pocet { get; set; }
        public int Chyby { get; set; }
        public int Uzivatele { get; set; }
        public long InputTokens { get; set; }
        public long OutputTokens { get; set; }
        public long CacheReadTokens { get; set; }
        public DateTime? Posledni { get; set; }
    }
    public class AdminAiFeature
    {
        public string Feature { get; set; }
        public int Pocet { get; set; }

    }
}
