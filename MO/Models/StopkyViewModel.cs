namespace MO.Models
{
    public class StopkyViewModel : BaseViewModel
    {
        public List<StopwatchRowViewModel> Rows { get; set; } = new List<StopwatchRowViewModel>();

        /// <summary>Nabídka projektů (platných pro vykazování hodin) - sdílená pro všechny řádky.</summary>
        public IEnumerable<ComboItem> ProjectComboItems { get; set; } = new List<ComboItem>();

        /// <summary>Součet naměřeného času všech časovačů včetně běžícího (sekundy, v okamžiku načtení).</summary>
        public int TotalSeconds { get; set; }

        /// <summary>Některý časovač právě běží.</summary>
        public bool IsRunning { get; set; }

        /// <summary>pid řádky, na kterou se má po postbacku odscrollovat / zaostřit.</summary>
        public int JumpToPid { get; set; }
    }
}
