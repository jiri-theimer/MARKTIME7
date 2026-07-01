namespace MO.Models
{
    public class StopwatchRowViewModel
    {
        public int pid { get; set; }
        public int p68Ordinary { get; set; }
        public bool p68IsRunning { get; set; }
        public DateTime? p68LastStart { get; set; }

        /// <summary>Dosud načtený čas v sekundách (bez aktuálně běžícího úseku).</summary>
        public int p68Duration { get; set; }

        /// <summary>Čas ve formátu HH:MM - editovatelné, jen když stopky neběží.</summary>
        public string DurationHHMM { get; set; }

        public int p41ID { get; set; }
        public string SelectedProjectText { get; set; }

        public int p32ID { get; set; }
        public string SelectedActivityText { get; set; }
        public IEnumerable<ComboItem> ActivityComboItems { get; set; } = new List<ComboItem>();

        public string p68Text { get; set; }
    }
}
