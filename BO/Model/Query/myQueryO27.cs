using System;
using System.Collections.Generic;
using System.Text;

namespace BO
{
    public class myQueryO27:baseQuery
    {
        
        public int x40id { get; set; }
        public int x31id { get; set; }
        public int o23id { get; set; }
        public List<int> o23ids { get; set; }

        public string entity { get; set; }
        public int recpid { get; set; }
        public string tempguid { get; set; }
        public string notepadguid { get; set; }

        public myQueryO27()
        {
            this.Prefix = "o27";
        }

        public override List<QRow> GetRows()
        {
            

            
            if (!string.IsNullOrEmpty(this.entity) && this.recpid>0)
            {
                AQ("a.o27Entity=@entity AND a.o27RecordPid=@recpid", "entity", this.entity,"AND",null,null,"recpid",this.recpid);
            }
          
            if (this.x40id > 0)
            {
                AQ("a.o27Entity='x40' AND a.o27RecordPid=@x40id", "x40id", this.x40id);
            }
            
            if (this.x31id > 0)
            {
                AQ("a.o27Entity='x31' AND a.o27RecordPid=@x31id", "x31id", this.x31id);
            }
            if (this.o23id > 0)
            {
                AQ("a.o27Entity='o23' AND a.o27RecordPid=@o23id", "o23id", this.o23id);
            }
            if (this.o23ids !=null && this.o23ids.Count > 0)
            {
                AQ("a.o27Entity='o23' AND a.o27RecordPid IN (" + string.Join(",",this.o23ids)+")",null,null);
            }

            if (this.tempguid != null)
            {
                AQ("a.o27ID NOT IN (select p85DataPid FROM p85Tempbox WHERE p85Guid=@tempguid)", "tempguid", this.tempguid);

            }
            if (this.notepadguid != null)
            {
                AQ("a.o27NotepadGuid=@notepadguid", "notepadguid", this.notepadguid);

            }

            return this.InhaleRows();

        }
    }
}
