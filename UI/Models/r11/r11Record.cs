namespace UI.Models.r11
{
    //Karta stropu hodin osoby na projektu (r11/Record) včetně bloků rozplánování (r12)
    public class r11Record : BaseRecordViewModel
    {
        public BO.r11CapacityLimit Rec { get; set; }
        public string ComboPerson { get; set; }
        public string ComboProject { get; set; }
        public List<r11RecordBlock> lisR12 { get; set; }

        //jen pro čtení: skutečnost
        public double ActFa { get; set; }
        public double ActNefa { get; set; }
        public DateTime? ProjectPlanFrom { get; set; }
        public DateTime? ProjectPlanUntil { get; set; }
    }

    public class r11RecordBlock
    {
        public int pid { get; set; }
        public DateTime? r12Start { get; set; }
        public DateTime? r12End { get; set; }
        public double r12Hours { get; set; }
        public string r12Text { get; set; }
        public bool IsTempDeleted { get; set; }     //řádek odstraněný v kartě: smaže se až uložením
    }
}
