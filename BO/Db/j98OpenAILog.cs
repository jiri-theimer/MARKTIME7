using System;

namespace BO
{
    /// <summary>Log AI volání – struktura po SQL/j98OpenAILog_upgrade.sql.</summary>
    public class j98OpenAILog
    {
        public int j98ID { get; set; }
        public DateTime j98Date { get; set; }
        public int? j02ID { get; set; }
        public string j98Login { get; set; }
        public string j98Feature { get; set; }
        public string j98ContextPage { get; set; }
        public string j98Guid { get; set; }            // identifikátor konverzace
        public short? j98TurnIndex { get; set; }
        public string j98Question { get; set; }
        public string j98Answer { get; set; }
        public string j98AnswerJson { get; set; }
        public string j98Status { get; set; }          // O / V / E
        public string j98ErrorText { get; set; }
        public byte j98RepairCount { get; set; }
        public string j98AiModel { get; set; }
        public string j98StopReason { get; set; }
        public int? j98InputTokens { get; set; }
        public int? j98OutputTokens { get; set; }
        public int? j98CacheReadTokens { get; set; }
        public int? j98CacheWriteTokens { get; set; }
        public int? j98DurationMs { get; set; }
        public string j98ResultEntity { get; set; }
        public int? j98ResultPid { get; set; }
        public short? j98Rating { get; set; }
        public string j98RatingComment { get; set; }
        public string j98ExtraJson { get; set; }
    }
}
