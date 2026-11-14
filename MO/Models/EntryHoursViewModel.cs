namespace MO.Models
{
    public class EntryHoursViewModel : EntryBaseViewModel
    {
        /// <summary>pid záznamu stopek, ze kterého úkon vznikl - po úspěšném uložení se stopky smažou.</summary>
        public int p68ID { get; set; }

        // Hodiny - povinné
        public string Hours { get; set; }

        // Čas od / do - volitelné
        public string TimeFrom { get; set; }
        public string TimeUntil { get; set; }

        // K hodinám vykázat i kusovníkové úkony ("navíc kusovník")
        public bool IsOfferNavicKusovnik { get; set; }
        public bool IsNavicKusovnik { get; set; }
        public int p34ID_Kusovnik { get; set; }
        public IEnumerable<ComboItem> KusovnikSesitComboItems { get; set; } = new List<ComboItem>();
        public IEnumerable<ComboItem> KusovnikActivityComboItems { get; set; } = new List<ComboItem>();
        public List<KusovnikRowViewModel> KusovnikRows { get; set; } = new List<KusovnikRowViewModel>();

        /// <summary>Již uložené kusovníkové úkony navázané na tento hodinový úkon (p31MasterID) - jen pro čtení.</summary>
        public IEnumerable<BO.p31Worksheet> ExistingKusovnikEntries { get; set; } = new List<BO.p31Worksheet>();

    }
}