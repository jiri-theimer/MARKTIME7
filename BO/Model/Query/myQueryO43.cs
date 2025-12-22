using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class myQueryO43: baseQuery
    {
        public int j02id { get; set; }
        public int j02id_owner { get; set; }
        public int j40id { get; set; }
        public int p28id { get; set; }
        public int p41id { get; set; }
        public int o23id { get; set; }
        public int p56id { get; set; }
        public int p91id { get; set; }
        public bool? mavazbu { get; set; }
        public myQueryO43()
        {
            this.Prefix = "o43";
        }

        public override List<QRow> GetRows()
        {
            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "o43DateInsert":
                        AQ("a.o43DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    default:
                        AQ("a.o43DateMessage BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                }
            }

            if (this.j40id > 0)
            {
                AQ("a.j40ID = @j40id", "j40id", this.j40id);
            }
            if (this.j02id_owner > 0)
            {
                AQ("a.j02ID_Owner = @ownerid", "ownerid", this.j02id_owner);
            }
            if (this.j02id > 0)
            {
                AQ("a.j02ID = @j02id", "j02id", this.j02id);
            }
            if (this.p41id > 0)
            {
                AQ("a.p41ID = @p41id", "p41id", this.p41id);
            }
            if (this.p28id > 0)
            {
                AQ("a.p28ID = @p28id", "p28id", this.p28id);
            }
            if (this.o23id > 0)
            {
                AQ("a.o23ID = @o23id", "o23id", this.o23id);
            }
            if (this.p56id > 0)
            {
                AQ("a.p56ID = @p56id", "p56id", this.p56id);
            }
            if (this.p91id > 0)
            {
                AQ("a.p91ID = @p91id", "p91id", this.p91id);
            }
            if (this.mavazbu == true)
            {
                AQ("(a.p28ID IS NOT NULL OR a.b05ID IS NOT NULL OR a.o23ID IS NOT NULL OR a.p91ID IS NOT NULL OR a.p41ID IS NOT NULL OR a.p56ID IS NOT NULL OR a.o22ID IS NOT NULL)", null, null);
            }
            if (this.mavazbu == false)
            {
                AQ("(a.p28ID IS NULL AND a.b05ID IS NULL AND a.o23ID IS NULL AND a.p91ID IS NULL AND a.p41ID IS NULL AND a.p56ID IS NULL AND a.o22ID IS NULL)", null, null);
            }

            if (this.MyRecordsDisponible)
            {
                Handle_MyDisponible();
            }

            return this.InhaleRows();

        }

        private void Handle_MyDisponible()
        {
            if (this.CurrentUser.TestPermission(PermValEnum.GR_o43_Reader) || this.CurrentUser.TestPermission(PermValEnum.GR_o43_Owner))
            {
                return; //přístup ke všem inbox záznamům
            }

            if (this.p41id>0 || this.p28id>0 || this.p56id > 0 || this.p91id>0)
            {
                return; //přístupné, protože uživatel se na inbox dívá z přístupného projektu/kontaktu/úkolu
            }

            string s = "(a.j02ID_Owner=@j02id_query OR a.j02ID=@j02id_query)";
           

            AQ(s, "j02id_query", get_real_j02id_query());
        }
    }
}
