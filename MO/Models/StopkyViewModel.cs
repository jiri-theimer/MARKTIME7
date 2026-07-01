namespace MO.Models
{
    public class StopkyViewModel : BaseViewModel
    {
        public List<StopwatchRowViewModel> Rows { get; set; } = new List<StopwatchRowViewModel>();

        /// <summary>Nabídka projektů (platných pro vykazování hodin) - sdílená pro všechny řádky.</summary>
        public IEnumerable<ComboItem> ProjectComboItems { get; set; } = new List<ComboItem>();

        /// <summary>ISO (UTC) čas, od kdy běží aktuálně spuštěné stopky - pro JS tikání v prohlížeči.</summary>
        public string RunningStartUtc { get; set; }

        /// <summary>Kolik sekund už měla běžící řádka načteno před posledním startem (aby JS navázal na správnou hodnotu).</summary>
        public int RunningBaseDuration { get; set; }

        /// <summary>pid řádky, na kterou se má po postbacku odscrollovat / zaostřit.</summary>
        public int JumpToPid { get; set; }
    }
}
