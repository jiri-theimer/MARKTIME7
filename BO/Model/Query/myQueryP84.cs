

namespace BO
{
    public class myQueryP84:baseQuery
    {
        public int p28id { get; set; }
        public int p91id { get; set; }
        public int p83id { get; set; }
        public int p78id { get; set; }  //sdružená upomínka

        public myQueryP84()
        {
            this.Prefix = "p84";
        }

        public override List<QRow> GetRows()
        {

            if (this.p91id > 0)
            {
                AQ("a.p91ID=@p91id", "p91id", this.p91id);
            }
            if (this.p83id > 0)
            {
                AQ("a.p83ID=@p83id", "p83id", this.p83id);
            }
            if (this.p78id > 0)
            {
                AQ("a.p84ID IN (select p84ID FROM p79UpominkaSdruzenaBinding WHERE p78ID=@p78id)", "p78id", this.p78id);
            }
            if (this.p28id > 0)
            {
                AQ("p91x.p28ID=@p28id", "p28id", this.p28id);
            }

            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "p84DateInsert":
                        AQ("a.p84DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p91DateMaturity":
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("p91x.p91DateMaturity BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("p91x.p91DateMaturity = @d1", "d1", this.global_d1_query);
                        }
                        break;
                    case "p91DateSupply":
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("p91x.p91DateSupply BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("p91x.p91DateSupply = @d1", "d1", this.global_d1_query);
                        }
                        break;
                    case "p91DateBilled":
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("p91x.p91DateBilled BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("p91x.p91DateBilled = @d1", "d1", this.global_d1_query);
                        }
                        break;
                    case "p84Date":
                    default:
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("a.p84Date BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("a.p84Date = @d1", "d1", this.global_d1_query);
                        }
                        break;
                }
            }


            if (_searchstring != null && _searchstring.Length > 2)
            {
                AQ("(a.p84Code like '%'+@expr+'%' OR a.p84TextA LIKE '%'+@expr+'%' OR p91x.p91Client LIKE '%'+@expr+'%')", "expr", _searchstring);

            }

            if (this.MyRecordsDisponible)
            {
                Handle_MyDisponible();
            }


            return this.InhaleRows();

        }

        private void Handle_MyDisponible()
        {
            if ((this.CurrentUser.IsAdmin || this.CurrentUser.TestPermission(PermValEnum.GR_P91_Owner) || this.CurrentUser.TestPermission(PermValEnum.GR_P91_Reader)))
            {
                return; //přístup ke všem zálohám v systému
            }


            string s = "a.j02ID_Owner=@j02id_query";
            s += " OR EXISTS (SELECT 1 FROM x69EntityRole_Assign xa inner join x67EntityRole xb ON xa.x67ID=xb.x67ID";
            s += " WHERE xb.x67Entity='p84' and (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1";

            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                s += " OR xa.j11ID IN (" + this.CurrentUser.j11IDs + ")";
            }
            s += ")";
            s += " AND xa.x69RecordEntity='p84' AND xa.x69RecordPid=a.p84ID";
            s += ")";





            AQ(s, "j02id_query", get_real_j02id_query());
        }
    }
}
