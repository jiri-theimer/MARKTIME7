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

        /// <summary>Odhad útraty v USD z tokenů v j98OpenAILog × ceník (BL.AI.Core.AiPricing); null = nic k odhadu.</summary>
        public decimal? AiCost30 { get; set; }
        public decimal? AiCostQuarter { get; set; }
        public DateTime AiQuarterStart { get; set; }
        /// <summary>Modely, které v ceníku nejsou (jejich spotřeba v odhadu chybí).</summary>
        public List<string> AiCostUnknownModels { get; set; } = new();
    }

    /// <summary>Součty tokenů za model: *30 = posledních 30 dní, *Q = aktuální čtvrtletí.</summary>
    public class AdminAiModelTokens
    {
        public string Model { get; set; }
        public long In30 { get; set; }
        public long Cw30 { get; set; }
        public long Cr30 { get; set; }
        public long Out30 { get; set; }
        public long InQ { get; set; }
        public long CwQ { get; set; }
        public long CrQ { get; set; }
        public long OutQ { get; set; }
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
