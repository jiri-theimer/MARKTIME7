namespace UI.Models.Record
{
    public class p40Record: BaseRecordViewModel
    {
        public BO.p40WorkSheet_Recurrence Rec { get; set; }

        public string ComboJ02 { get; set; }
        public string ComboP34 { get; set; }
        public string ComboP32 { get; set; }
        
        public string ComboJ27Code { get; set; }

        public ProjectComboViewModel ProjectCombo { get; set; }

        public string ValueLabel { get; set; }
    }
}
