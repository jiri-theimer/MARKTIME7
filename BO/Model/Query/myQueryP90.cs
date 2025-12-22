
namespace BO
{
    public class myQueryP90:baseQuery
    {
        public int p28id { get; set; }
        public int p91id { get; set; }
        public int j27id { get; set; }
        public int o51id { get; set; }
        
        public myQueryP90()
        {
            this.Prefix = "p90";
        }

        public override List<QRow> GetRows()
        {

            if (this.p91id > 0)
            {
                AQ("a.p90ID IN (SELECT zb.p90ID FROM p99Invoice_Proforma za INNER JOIN p82Proforma_Payment zb ON za.p82ID=zb.p82ID WHERE za.p91ID=@p91id)", "p91id", this.p91id);
            }
            if (this.p28id > 0)
            {
                AQ("a.p28ID=@p28id", "p28id", this.p28id);
            }
            if (this.j27id > 0)
            {
                AQ("a.j27ID=@j27id)", "j27id", this.j27id);
            }
            if (this.o51id > 0)
            {
                AQ("a.p90ID IN (select o52RecordPid FROM o52TagBinding where o52RecordEntity='p90' AND o51ID=@o51id)", "o51id", this.o51id);
            }

            if (_searchstring != null && _searchstring.Length > 2)
            {
                AQ("(a.p90Code like '%'+@expr+'%' OR a.p90Text1 LIKE '%'+@expr+'%' OR a.p28ID IN (select p28ID FROM p28Contact WHERE p28Name like '%'+@expr+'%') OR a.p90ID IN (select p90ID FROM p82Proforma_Payment WHERE p82Code LIKE '%'+@expr+'%'))", "expr", _searchstring);

            }

            if (this.MyRecordsDisponible )
            {
                Handle_MyDisponible();
            }
            

            return this.InhaleRows();

        }

        private void Handle_MyDisponible()
        {
            if ((this.CurrentUser.IsAdmin || this.CurrentUser.TestPermission(PermValEnum.GR_p90_Owner) || this.CurrentUser.TestPermission(PermValEnum.GR_p90_Reader)))
            {
                return; //přístup ke všem zálohám v systému
            }
            

            string s = "a.j02ID_Owner=@j02id_query";
            s += " OR EXISTS (SELECT 1 FROM x69EntityRole_Assign xa inner join x67EntityRole xb ON xa.x67ID=xb.x67ID";
            s += " WHERE xb.x67Entity='p90' and (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1";
            
            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                s += " OR xa.j11ID IN (" + this.CurrentUser.j11IDs + ")";
            }
            s += ")";
            s += " AND xa.x69RecordEntity='p90' AND xa.x69RecordPid=a.p90ID";
            s += ")";



           
            
            AQ(s, "j02id_query", get_real_j02id_query());
        }
    }
}
