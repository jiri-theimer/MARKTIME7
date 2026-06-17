using BO;

namespace UI.Models
{
    public class ReminderItemViewModel:BaseViewModel
    {
        public string message { get; set; }
        public bool issubmit { get; set; }
        public string rowguid { get; set; }
        public bool is_static_date { get; set; }
        public string record_prefix { get; set; }
        public int o24Count { get; set; }
        public string o24Unit { get; set; }
        public o24MediumFlagEnum o24MediumFlag { get; set; }
        public DateTime? o24StaticDate { get; set; }
        public string o24Memo { get; set; }
        public string BindPrefix { get; set; }
        public int j02ID { get; set; }
        public string ComboJ02ID { get; set; }
        public int j11ID { get; set; }
        public string ComboJ11ID { get; set; }
        public int p28ID { get; set; }
        public string ComboP28Name { get; set; }
        public int p24ID { get; set; }
        public string ComboP24Name { get; set; }

        public int x67ID { get; set; }
        public string ComboX67ID { get; set; }
        public string TempGuid { get; set; }
        public bool IsTempDeleted { get; set; }
        public int o24ID { get; set; }
    }
}
