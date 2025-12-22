namespace UI.Models
{
    public class HlidacViewModel:BaseRecordViewModel
    {
        public IEnumerable<BO.b20Hlidac> lisB20 { get; set; }
        public bool ShowB20Combo { get; set; }
        public int SelectedB20ID { get; set; }
        public string SelectedB20Name { get; set; }
        public List<b21RepeaterItem> lisItems { get; set; }
        public string b21_postback_par { get; set; }
        public void SaveChanges(BL.Factory f,int rec_pid)
        {
            if (this.lisItems == null || this.lisItems.Count() == 0) return;
            var lis = new List<BO.b21HlidacBinding>();
            foreach(var c in this.lisItems)
            {
                var rec = new BO.b21HlidacBinding() { pid = c.b21ID, IsSetAsDeleted = c.IsTempDeleted, b21RecordEntity = this.rec_entity, b20ID = c.b20ID, b21Par1 = c.b21Par1, b21Par2 = c.b21Par2, b21NotifyReceivers = c.b21NotifyReceivers };
                rec.b21RecordPid = rec_pid;
                lis.Add(rec);                
                
            }
            f.b20HlidacBL.SaveB21List(this.rec_entity, rec_pid, lis);
        }
    }

    public class b21RepeaterItem
    {
        public int b21ID { get; set; }
        public int b20ID { get; set; }
        public string b20Name { get; set; }
        public string b20Par1Name { get; set; }
        public string b20Par2Name { get; set; }
        public bool IsCyklus { get; set; }


        public double b21Par1 { get; set; }
        public double b21Par2 { get; set; }
        public string b21NotifyReceivers { get; set; }
        public string TempGuid { get; set; }
        public bool IsTempDeleted { get; set; }


        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted)
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
