using System;
using System.Collections.Generic;
using System.Text;

namespace BO
{
    public class myQueryX40: baseQuery
    {
        public int j02id { get; set; }
        public int j02id_creator { get; set; }
        public int p91id { get; set; }
        public int p84id { get; set; }

        public myQueryX40()
        {
            this.Prefix = "x40";
        }

        public override List<QRow> GetRows()
        {
            
            if (this.j02id > 0)
            {
                AQ("a.j02ID_Creator=@j02id OR (a.x40RecordEntity='j02' AND a.x40RecordPid=@j02id)", "j02id", this.j02id);
            }
            if (this.j02id_creator > 0)
            {
                AQ("a.j02ID_Creator=@j02idc", "j02idc", this.j02id_creator);
            }
            if (this.p91id > 0)
            {
                AQ("(a.x40RecordEntity='p91' AND a.x40RecordPid=@p91id) OR (a.x40RecordEntity='p84' AND a.x40RecordPid IN (select p84ID FROM p84Upominka WHERE p91ID=@p91id))", "p91id", this.p91id);
            }
            if (this.p84id > 0)
            {
                AQ("a.x40RecordEntity='p91' AND a.x40RecordPid=@p91id", "p91id", this.p91id);
            }

            return this.InhaleRows();

        }
    }
}
