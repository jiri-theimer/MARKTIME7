namespace UI.Models
{
    public class BarcodesViewModel
    {
        public bool IsBCScanner { get; set; }
        public bool IsPlayBeepSuccess { get; set; }
        public bool IsPlayBeepError { get; set; }
        public string record_prefix { get; set; }
        public int record_pid { get; set; }

        public string BarcodeValue { get; set; }
        public IEnumerable<BO.j28Barcode> lisJ28 { get; set; }

        public string postback_par { get; set; }
        public string postback_guid { get; set; }

        public string LastScannedValue { get; set; }
        public string UserMessage { get; set; }
        public string TempGuid { get; set; }    //předává se zvenku ze záznamu

        public void CommitChangesIfNewRecords(BL.Factory f,int record_pid)
        {
            if (this.lisJ28 == null)
            {
                this.lisJ28 = f.j28BarcodeBL.GetList(this.TempGuid).Where(p => p.j28RecordPid == 0);
            }
            
            if (this.lisJ28.Count() > 0)
            {
                foreach (var c in this.lisJ28)
                {
                    c.j28RecordPid = record_pid;                    
                    f.j28BarcodeBL.Save(c);
                }
            }
            
        }
    }
}
