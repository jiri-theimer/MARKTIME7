namespace UI.Models.Record
{
    public class x54Record:BaseRecordViewModel
    {
        public BO.x54WidgetGroup Rec { get; set; }
        public List<x57Repeater> lisX57 { get; set; }

    }

    public class x57Repeater
    {
        public string ComboX55 { get; set; }
        public int x55ID { get; set; }
        public bool x57IsDefault { get; set; }
        public int x57Ordinary { get; set; }
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted == true)
                {
                    return "display:none;";
                }
                else
                {
                    return "display:table-row;";
                }
            }
        }
    }
}
