namespace MO.Models
{
    public class AiViewModel : BaseViewModel
    {
        public bool IsEnabled { get; set; }
        public string DisabledReason { get; set; }
        public int MyID { get; set; }
        /// <summary>Uživatel má modul úkonů - agent umí připravit vykázání hodin.</summary>
        public bool CanWriteHours { get; set; }
    }
}
