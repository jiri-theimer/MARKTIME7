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

        public int j02id { get; set; }
        public int j02id_owner { get; set; }
        public int j40id { get; set; }
        public int p28id { get; set; }
        public int p41id { get; set; }
        
        public int p56id { get; set; }
        public int p91id { get; set; }

        public string entity { get; set; }
        public int recpid { get; set; }
        public string tempguid { get; set; }
        public string notepadguid { get; set; }

        public bool? mavazbu { get; set; }

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

            if (this.j02id > 0)
            {
                AQ("a.o27RecPid = @j02id AND a.o27Entity='j02'", "j02id", this.j02id);
            }
            if (this.p41id > 0)
            {
                AQ("a.o27RecPid = @p41id AND a.o27Entity='p41'", "p41id", this.p41id);
            }
            if (this.p28id > 0)
            {
                AQ("a.o27RecPid = @p28id AND a.o27Entity='p28'", "p28id", this.p28id);
            }
            if (this.o23id > 0)
            {
                AQ("a.o27RecPid = @o23id AND a.o27Entity='o23'", "o23id", this.o23id);
            }
            if (this.p56id > 0)
            {
                AQ("a.o27RecPid = @p56id AND a.o27Entity='p26'", "p56id", this.p56id);
            }
            if (this.p91id > 0)
            {
                AQ("a.o27RecPid = @p91id AND a.o27Entity='p91'", "p91id", this.p91id);
            }
            if (this.mavazbu == true)
            {
                AQ("(a.o27Entity IS NOT NULL AND o27RecPid IS NOT NULL)", null, null);
            }
            if (this.mavazbu == false)
            {
                AQ("a.o27Entity IS NULL AND o27RecPid IS NULL", null, null);
            }

            if (this.MyRecordsDisponible)
            {
                Handle_MyDisponible();
            }

            return this.InhaleRows();

        }

        private void Handle_MyDisponible()
        {
            if (this.CurrentUser.TestPermission(PermValEnum.GR_o27_Reader) || this.CurrentUser.TestPermission(PermValEnum.GR_o27_Owner))
            {
                return; //přístup ke všem inbox záznamům
            }

            if (this.o23id>0 || this.p41id > 0 || this.p28id > 0 || this.p56id > 0 || this.p91id > 0 || this.j02id > 0)
            {
                return; //přístupné, protože uživatel se na filebox dívá z přístupného projektu/kontaktu/úkolu/dokumentu
            }

            string s = "(a.j02ID_Owner=@j02id_query OR a.j02ID=@j02id_query)";


            AQ(s, "j02id_query", get_real_j02id_query());
        }
    }
}
