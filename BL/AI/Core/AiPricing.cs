namespace BL.AI.Core
{
    /// <summary>
    /// Orientační ceník modelů (USD za 1 milion tokenů) pro ODHAD útraty z j98OpenAILog.
    /// Skutečnou útratu ukazuje jen Claude Console (Usage / Cost). Ceník platný k 9. 11. 2026
    /// (https://platform.claude.com/docs/en/about-claude/pricing) – při změně ceníku upravit tabulku.
    /// Pozn.: input_tokens z API NEobsahují tokeny z cache; cache write = 5min cache (1,25× vstup), cache read dle modelu.
    /// Haiku 5.5 počítá s cenou pro prompty do 100K tokenů.
    /// </summary>
    public static class AiPricing
    {
        public const string PriceListDate = "9. 11. 2026";

        public class Price
        {
            public decimal Input { get; init; }
            public decimal CacheWrite { get; init; }
            public decimal CacheRead { get; init; }
            public decimal Output { get; init; }
        }

        // klíč = prefix ID modelu; hledá se nejdelší shoda (claude-opus-5-5 před claude-opus-5)
        private static readonly Dictionary<string, Price> _prices = new()
        {
            ["claude-fable-5-1"] = new Price { Input = 10m, CacheWrite = 12.5m, CacheRead = 0.25m, Output = 50m },
            ["claude-mythos-5-1"] = new Price { Input = 10m, CacheWrite = 12.5m, CacheRead = 0.25m, Output = 50m },
            ["claude-opus-5-5"] = new Price { Input = 4m, CacheWrite = 5m, CacheRead = 0.20m, Output = 20m },
            ["claude-sonnet-5-5"] = new Price { Input = 2m, CacheWrite = 2.5m, CacheRead = 0.10m, Output = 10m },
            ["claude-haiku-5-5"] = new Price { Input = 0.10m, CacheWrite = 0.125m, CacheRead = 0.01m, Output = 0.50m },
            ["claude-fable-5"] = new Price { Input = 10m, CacheWrite = 12.5m, CacheRead = 1m, Output = 50m },
            ["claude-mythos-5"] = new Price { Input = 10m, CacheWrite = 12.5m, CacheRead = 1m, Output = 50m },
            ["claude-opus-5"] = new Price { Input = 5m, CacheWrite = 6.25m, CacheRead = 0.50m, Output = 25m },
            ["claude-sonnet-5"] = new Price { Input = 2m, CacheWrite = 2.5m, CacheRead = 0.20m, Output = 10m },
            ["claude-opus-4"] = new Price { Input = 5m, CacheWrite = 6.25m, CacheRead = 0.50m, Output = 25m },        // 4.5–4.8
            ["claude-opus-4-1"] = new Price { Input = 15m, CacheWrite = 18.75m, CacheRead = 1.50m, Output = 75m },    // legacy
            ["claude-opus-4-2"] = new Price { Input = 15m, CacheWrite = 18.75m, CacheRead = 1.50m, Output = 75m },    // legacy Opus 4 (claude-opus-4-2025…)
            ["claude-sonnet-4"] = new Price { Input = 3m, CacheWrite = 3.75m, CacheRead = 0.30m, Output = 15m },
            ["claude-haiku-4-5"] = new Price { Input = 1m, CacheWrite = 1.25m, CacheRead = 0.10m, Output = 5m },
        };

        /// <summary>Cena pro model, null = model v ceníku není.</summary>
        public static Price Find(string model)
        {
            if (string.IsNullOrWhiteSpace(model)) return null;
            string m = model.Trim().ToLowerInvariant();
            return _prices.Where(p => m.StartsWith(p.Key)).OrderByDescending(p => p.Key.Length).Select(p => p.Value).FirstOrDefault();
        }

        /// <summary>Odhad ceny v USD, null = model v ceníku není.</summary>
        public static decimal? EstimateUsd(string model, long inputTokens, long cacheWriteTokens, long cacheReadTokens, long outputTokens)
        {
            var p = Find(model);
            if (p == null) return null;
            return (inputTokens * p.Input + cacheWriteTokens * p.CacheWrite + cacheReadTokens * p.CacheRead + outputTokens * p.Output) / 1_000_000m;
        }
    }
}
