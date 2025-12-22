

namespace BO
{
    public class myQueryB05:baseQuery
    {
        public int j02id_sys { get; set; }
        public int record_pid { get; set; }
        public string record_prefix { get; set; }
        

        public myQueryB05()
        {
            this.Prefix = "b05";
        }
        public override List<QRow> GetRows()
        {
            if (this.j02id_sys > 0)
            {
                AQ("a.j02ID_Sys=@j02idsys", "j02idsys", this.j02id_sys);
            }
            if (!this.CurrentUser.TestPermission(PermValEnum.GR_AllowRates))
            {
                AQ("a.b05Tab1Flag & 4 = 4", null, null);    //nemá přístup k fakturačním poznámkám
            }
          


            if (this.record_prefix=="p91" && this.record_pid > 0)
            {
                AQ("(a.b05RecordPid=@p91id AND a.b05RecordEntity='p91') OR (a.b05RecordEntity='p84' AND a.b05RecordPid IN (select p84ID FROM p84Upominka WHERE p91ID=@p91id))", "p91id", this.record_pid);
            }
            else
            {
                if (this.record_prefix != null)
                {
                    AQ("a.b05RecordEntity=@record_prefix", "record_prefix", this.record_prefix);
                }
                if (this.record_pid > 0)
                {
                    AQ("a.b05RecordPid=@record_pid", "record_pid", this.record_pid);
                }
            }

            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    
                    case "b05Date":
                        AQ("a.b05Date BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    default:
                        AQ("a.b05DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;

                }
            }
            
            if (!this.CurrentUser.TestPermission(BO.PermValEnum.GR_b05ReadAll))
            {
                AQ("a.j02ID_Sys=@j02idsys", "j02idsys", this.CurrentUser.pid);
            }

            

            return this.InhaleRows();

        }
    }
}
