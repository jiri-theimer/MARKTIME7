namespace UI.Models.Record
{
    public class b11RepeaterItem
    {
        public b11RepeaterItem()
        {
            this.TempGuid = BO.Code.Bas.GetGuid();
        }
        public int x67ID { get; set; }
        public string x67Name { get; set; }
        public int j61ID { get; set; }
        public string j61Name { get; set; }
        public int j11ID { get; set; }
        public string j11Name { get; set; }
        public int j04ID { get; set; }
        public string j04Name { get; set; }
        public string b11Subject { get; set; }
        public int RecordUserRole { get; set; }

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
                    return "display:table-row";
                }
            }
        }
    }
    public class b06Record:BaseRecordViewModel
    {
        public BO.b06WorkflowStep Rec { get; set; }

        public BO.b02WorkflowStatus RecB02 { get; set; }
        
        public IEnumerable<BO.b02WorkflowStatus> lisTargetB02 { get; set; }
        public IEnumerable<BO.p60TaskTemplate> lisP60 { get; set; }
        public IEnumerable<BO.p83UpominkaType> lisP83 { get; set; }

        public string Receiver_j11IDs { get; set; }
        public string Receiver_j11Names { get; set; }

        public List<int> Receiver_x67IDs { get; set; }
        public List<BO.x67EntityRole> Receiver_AllX67 { get; set; }
        public string Receiver_j04IDs { get; set; }
        public string Receiver_j04Names { get; set; }

        public bool Receiver_IsRecordOwner { get; set; }
        public bool Receiver_IsRecordCreator { get; set; }

        public List<b11RepeaterItem> lisB11 { get; set; }
        public List<BO.x67EntityRole> lisAllX67_Nominee { get; set; }
        //public List<BO.x67EntityRole> lisAllX67_Nominee_Source { get; set; }

        public string Nominee_j04Name { get; set; }
        
        
    }
}
