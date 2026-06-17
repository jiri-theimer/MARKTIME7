

namespace BO
{
    public class myQueryP78: baseQuery
    {
        public int p28id { get; set; }
        public int p91id { get; set; }
        public int p84id { get; set; }
        public int j27id { get; set; }
       
        public myQueryP78()
        {
            this.Prefix = "p78";
        }

        public override List<QRow> GetRows()
        {

            if (this.p91id > 0)
            {
                AQ("a.p78ID IN (select xa.p78ID FROM p79UpominkaSdruzenaBinding xa INNER JOIN p84Upominka xb ON xa.p84ID=xb.p84ID WHERE xb.p91ID = @p91id)", "p91id", this.p91id);
            }
            if (this.p84id > 0)
            {
                AQ("a.p78ID IN (select p78ID FROM p79UpominkaSdruzenaBinding WHERE p84ID=@p84id)", "p84id", this.p84id);
            }
          
            if (this.p28id > 0)
            {
                AQ("a.p28ID=@p28id", "p28id", this.p28id);
            }
            if (this.j27id > 0)
            {
                AQ("a.j27ID=@j27id", "j27id", this.j27id);
            }

            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "p78DateInsert":
                        AQ("a.p78DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p91DateMaturity":
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("a.p78ID IN (select xa.p78ID FROM p79UpominkaSdruzenaBinding xa INNER JOIN p84Upominka xb ON xa.p84ID=xb.p84ID INNER JOIN p91Invoice xc ON xb.p91ID=xc.p91ID WHERE xc.p91DateMaturity BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("a.p78ID IN (select xa.p78ID FROM p79UpominkaSdruzenaBinding xa INNER JOIN p84Upominka xb ON xa.p84ID=xb.p84ID INNER JOIN p91Invoice xc ON xb.p91ID=xc.p91ID WHERE xc.p91DateMaturity = @d1)", "d1", this.global_d1_query);
                        }
                        break;
                    case "p91DateSupply":
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("a.p78ID IN (select xa.p78ID FROM p79UpominkaSdruzenaBinding xa INNER JOIN p84Upominka xb ON xa.p84ID=xb.p84ID INNER JOIN p91Invoice xc ON xb.p91ID=xc.p91ID WHERE xc.p91DateSupply BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("a.p78ID IN (select xa.p78ID FROM p79UpominkaSdruzenaBinding xa INNER JOIN p84Upominka xb ON xa.p84ID=xb.p84ID INNER JOIN p91Invoice xc ON xb.p91ID=xc.p91ID WHERE xc.p91DateSupply = @d1)", "d1", this.global_d1_query);
                        }

                        
                        break;                   
                    case "p78Date":
                    default:
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("a.p78Date BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("a.p78Date = @d1", "d1", this.global_d1_query);
                        }
                        break;
                }
            }


            if (_searchstring != null && _searchstring.Length > 2)
            {
                AQ("(a.p78Code like '%'+@expr+'%' OR a.p78TextA LIKE '%'+@expr+'%' OR p28x.p28Name LIKE '%'+@expr+'%')", "expr", _searchstring);

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
            s += " WHERE xb.x67Entity='p78' and (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1";

            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                s += " OR xa.j11ID IN (" + this.CurrentUser.j11IDs + ")";
            }
            s += ")";
            s += " AND xa.x69RecordEntity='p78' AND xa.x69RecordPid=a.p78ID";
            s += ")";





            AQ(s, "j02id_query", get_real_j02id_query());
        }
    }
}
