

namespace BO
{
    public class myQueryP28:baseQuery
    {
        public int j02id_owner { get; set; }
        public int p29id { get; set; }
        public int b02id { get; set; }
        public int p51id { get; set; }      
        public int o51id { get; set; }
        public int p24id { get; set; }
        public List<int> p24ids { get; set; }
        public int p28parentid { get; set; }
        public bool? p28iscompany { get; set; }
        public bool? canbe_supplier { get; set; }
        public bool? canbe_client { get; set; }
        public bool isportal { get; set; }
        public int j27id_query { get; set; }

        public bool? vyloucit_kontaktni_osoby { get; set; }

        public int treeprev { get; set; }
        public int treenext { get; set; }
        public myQueryP28()
        {
            this.Prefix = "p28";
        }

        public override List<QRow> GetRows()
        {
            if (this.p31statequery > 0) this.Handle_p31StateQuery();
            if (!string.IsNullOrEmpty(this.p31tabquery)) this.Handle_p31TabQuery();

            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "p28DateInsert":
                        AQ("a.p28DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;

                    case "p91Date":
                        AQ("(a.p28ID IN (select p28ID FROM p91Invoice WHERE p91Date BETWEEN @d1 AND @d2) OR a.p28ID IN (select xc.p28ID_Client FROM p31Worksheet xa INNER JOIN p91Invoice xb ON xa.p91ID=xb.p91ID INNER JOIN p41Project xc ON xa.p41ID=xc.p41ID WHERE xc.p28ID_Client IS NOT NULL AND xb.p91Date BETWEEN @d1 AND @d2))", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p91DateSupply":
                        AQ("(a.p28ID IN (select p28ID FROM p91Invoice WHERE p91DateSupply BETWEEN @d1 AND @d2) OR a.p28ID IN (select xc.p28ID_Client FROM p31Worksheet xa INNER JOIN p91Invoice xb ON xa.p91ID=xb.p91ID INNER JOIN p41Project xc ON xa.p41ID=xc.p41ID WHERE xc.p28ID_Client IS NOT NULL AND xb.p91DateSupply BETWEEN @d1 AND @d2))", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p31Date":
                    default:
                        AQ("EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID INNER JOIN p28Contact xc ON xb.p28ID_Client=xc.p28ID WHERE xc.p28ID = a.p28ID AND xa.p31Date BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                }
            }

            if (this.iswip != null)
            {
                if (this.iswip == true)
                {
                    AQ("EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xb.p28ID_Client=a.p28ID AND xa.p71ID IS NULL AND xa.p91ID IS NULL AND xa.p31ExcludeBillingFlag IS NULL AND xa.p31Date between @p31date1 AND @p31date2)", null, null);
                }
                else
                {
                    AQ("NOT EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xb.p28ID_Client=a.p28ID AND xa.p71ID IS NULL AND xa.p91ID IS NULL AND xa.p31Date between @p31date1 AND @p31date2)", null, null);
                }
            }
            if (this.isapproved_and_wait4invoice != null)
            {
                if (this.isapproved_and_wait4invoice == true)
                {
                    AQ("EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xb.p28ID_Client=a.p28ID AND xa.p71ID=1 AND xa.p91ID IS NULL AND xa.p31Date between @p31date1 AND @p31date2)", null, null);
                }
                //else
                //{
                //    AQ("NOT EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xb.p28ID_Client=a.p28ID AND xa.p71ID=1 AND xa.p72ID_AfterApprove=4 AND xa.p91ID IS NULL AND xa.p31Date between @p31date1 AND @p31date2)", null, null);
                //}
            }

            if (this.j27id_query > 0)
            {
                AQ("EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xb.p28ID_Client=a.p28ID AND xa.j27ID_Billing_Orig=@j27id_query)", "j27id_query", this.j27id_query);
            }
            
            if (this.j02id_owner > 0)
            {
                AQ("a.j02ID_Owner=@j02id_owner", "j02id_owner", this.j02id_owner);
            }
            if (this.p29id > 0)
            {
                AQ("a.p29ID=@p29id", "p29id", this.p29id);
            }
            if (this.b02id > 0)
            {
                AQ("a.b02ID=@b02id", "b02id", this.b02id);
            }
            if (this.p51id > 0)
            {
                AQ("a.p51ID_Billing=@p51id", "p51id", this.p51id);
            }
            if (this.o51id > 0)
            {
                AQ("a.p28ID IN (select o52RecordPid FROM o52TagBinding where o52RecordEntity='p28' AND o51ID=@o51id)", "o51id", this.o51id);
            }
            if (this.o51ids != null)
            {
                AQ(" AND a.p28ID IN (SELECT o52RecordPID FROM o52TagBinding WHERE o52RecordEntity='p28' AND o51ID IN (" + String.Join(",", this.o51ids) + "))", null, null);
            }

            if (this.p24id > 0)
            {
                AQ("a.p28ID IN (select p28ID FROM p25ContactGroupBinding WHERE p24ID=@p24id)", "p24id", this.p24id);
            }
            if (this.p24ids != null && this.p24ids.Count > 0)
            {
                AQ($"a.p28ID IN (select p28ID FROM p25ContactGroupBinding WHERE j11ID IN ({string.Join(",", this.p24ids)}))", null, null);
            }

            //if (this.p29ids !=null)
            //{
            //    AQ("a.p29ID IN (" + string.Join(",", this.p29ids) + ")", null, null);
            //}
            //if (this.b02ids != null)
            //{
            //    AQ("a.b02ID IN (" + string.Join(",", this.b02ids) + ")", null, null);
            //}
            //if (this.p51ids != null)
            //{
            //    AQ("a.p51ID_Billing IN (" + string.Join(",", this.p51ids) + ")", null, null);
            //}
            if (this.p28parentid > 0)
            {
                AQ("a.p28ParentID=@parentpid", "parentpid", this.p28parentid);
            }
            if (this.p28iscompany != null)
            {
                if (this.p28iscompany == true) AQ("a.p28IsCompany=1", null, null);
                if (this.p28iscompany == false) AQ("a.p28IsCompany=0", null, null);

            }
            if (this.canbe_client != null)
            {
                if (this.canbe_client == true)
                {
                    AQ("p29x.p29ScopeFlag IN (0,1,3)", null, null);
                }
                if (this.canbe_client == false)
                {
                    AQ("p29x.p29ScopeFlag IN (2,4)", null, null);
                }
            }

            if (this.canbe_supplier != null)
            {
                if (this.canbe_supplier == true)
                {
                    AQ("p29x.p29ScopeFlag IN (0,2,3)", null, null);
                }
                if (this.canbe_supplier == false)
                {
                    AQ("p29x.p29ScopeFlag IN (1,4)", null, null);
                }
            }

            if (this.treenext > this.treeprev)
            {
                AQ("a.p28TreeIndex BETWEEN @prev AND @next", "prev", this.treeprev, "AND", null, null, "next", this.treenext);
            }


            if (!string.IsNullOrEmpty(_searchstring))
            {
                string s = "";
                if (_searchstring.Length == 1)
                {
                    //hledat pouze podle počátečních písmen
                    s = "a.p28Name COLLATE Latin1_General_CI_AI Like @expr+'%' OR a.p28Code LIKE '%'+@expr+'%' OR a.p28ShortName LIKE @expr+'%' OR a.p28CompanyName LIKE @expr+'%'";
                    
                }
                else
                {
                    //něco jako fulltext
                    s = "a.p28Name COLLATE Latin1_General_CI_AI LIKE '%'+@expr+'%' OR a.p28ShortName LIKE '%'+@expr+'%' OR a.p28CompanyName LIKE '%'+@expr+'%'";
                    if (_searchstring.Length >= 2)
                    {
                        s += " OR a.p28Code LIKE '%'+@expr+'%' OR a.p28RegID LIKE @expr+'%' OR a.p28VatID LIKE @expr+'%' OR a.p28City1 COLLATE Latin1_General_CI_AI LIKE '%'+@expr+'%' OR a.p28Street1 COLLATE Latin1_General_CI_AI LIKE '%'+@expr+'%'";
                    }
                    if (_searchstring.Length >= 4)
                    {
                        s += " OR a.p28ID IN (SELECT p28ID FROM o32Contact_Medium WHERE o32Value LIKE '%'+@expr+'%')";
                    }

                }
                AQ(s, "expr", _searchstring);

            }

            if (this.isportal)
            {
                Handle_MyDisponible_Portal();
            }
            else
            {
                if (this.MyRecordsDisponible || this.MyRecordsDisponible_Approve)
                {
                    Handle_MyDisponible();
                }
            }

            if (vyloucit_kontaktni_osoby == true)
            {
                AQ("a.p28ID NOT IN (select p28ID_Person FROM p30ContactPerson)", null, null);
                //AQ("NOT EXISTS (SELECT p28ID FROM p30ContactPerson WHERE p28ID_Person=a.p28ID)", null, null);
            }
            


            return this.InhaleRows();

        }


        private void Handle_MyDisponible_Portal()
        {
            
            if (this.CurrentUser.TestPermission(PermValEnum.GR_p28_Owner))
            {
                return; //přístup vlastníka ke všem kontaktům
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("EXISTS (SELECT 1 FROM x69EntityRole_Assign xa inner join x67EntityRole xb ON xa.x67ID=xb.x67ID INNER JOIN x68EntityRole_Permission xc ON xa.x67ID=xc.x67ID");
            sb.Append(" WHERE xb.x67Entity='p28' AND xc.x68PermValue=10 AND (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1");
            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                sb.Append($" OR xa.j11ID IN ({this.CurrentUser.j11IDs})");
            }
            sb.Append(")");
            sb.Append(" AND (xa.x69RecordEntity='p28' AND xa.x69RecordPid=a.p28ID) OR (xa.x69RecordEntity='p29' AND xa.x69RecordPid=a.p29ID)");
            sb.Append(")");

            AQ(sb.ToString(), "j02id_query", get_real_j02id_query());
        }

        private void Handle_MyDisponible()
        {
            if (this.MyRecordsDisponible_Approve)
            {
                //pouze klienti ke schvalování úkonů
                if (this.CurrentUser.TestPermission(PermValEnum.GR_P31_Approver)) return; //oprávnění schvalovat všechny úkony
            }
            else
            {
                if (this.CurrentUser.TestPermission(PermValEnum.GR_p28_Reader) || this.CurrentUser.TestPermission(PermValEnum.GR_p28_Owner))
                {
                    return; //přístup ke všem kontaktům
                }
            }
            
            var sb = new System.Text.StringBuilder();

            if (this.MyRecordsDisponible_Approve)
            {

                sb.Append("EXISTS (SELECT xxa.x72ID FROM x72ProjectRole_Permission xxa INNER JOIN p41Project xxb ON xxa.p41ID=xxb.p41ID WHERE xxb.p28ID_Client=a.p28ID AND xxa.o28PermFlag IN (3,4) AND xxa.x72IsValid=1 AND (xxa.j02ID=@j02id_query OR xxa.x72IsAllUsers=1");
                if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
                {
                    sb.Append($" OR xxa.j11ID IN ({this.CurrentUser.j11IDs})");
                }
                sb.Append(")");
                sb.Append(")");

                //sb.Append(" EXISTS (SELECT 1 FROM p41Project ca INNER JOIN x69EntityRole_Assign xa ON ca.p41ID=xa.x69RecordPid INNER JOIN x67EntityRole xb ON xa.x67ID=xb.x67ID");
                //sb.Append(" INNER JOIN o28ProjectRole_Workload xc ON xb.x67ID=xc.x67ID");
                //sb.Append(" WHERE ca.p28ID_Client=a.p28ID");                
                //sb.Append(" AND xb.x67Entity='p41' AND xc.o28PermFlag IN (3,4) AND (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1");

                //if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
                //{
                //    sb.Append($" OR xa.j11ID IN ({this.CurrentUser.j11IDs})");
                //}
                //sb.Append(")");

                //if (this.CurrentUser.x69ID_p41_p28 > 0)
                //{
                //    sb.Append(" OR (xa.x69RecordEntity='p28' AND xa.x69RecordPid=ca.p28ID_Client)");    //projektové role nastavené v klientovi 
                //}
                //if (this.CurrentUser.x69ID_p41_j18 > 0)
                //{
                //    sb.Append(" OR (xa.x69RecordEntity='j18' AND xa.x69RecordPid=ca.j18ID)");    //projektové role nastavené ve středisku 
                //}

            }
            else
            {
                
                sb.Append("a.j02ID_Owner=@j02id_query");
                sb.Append(" OR EXISTS (SELECT 1 FROM x75ContactRole_Permission WHERE (j02ID=@j02id_query OR x75IsAllUsers=1");                

                if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
                {
                    sb.Append($" OR j11ID IN ({this.CurrentUser.j11IDs})");
                }
                sb.Append(")");
                sb.Append(" AND p28ID=a.p28ID AND x75IsValid=1");
                
                

                //sb.Append("a.j02ID_Owner=@j02id_query");
                //sb.Append(" OR EXISTS (SELECT 1 FROM x69EntityRole_Assign xa inner join x67EntityRole xb ON xa.x67ID=xb.x67ID INNER JOIN x68EntityRole_Permission xc ON xa.x67ID=xc.x67ID");
                //sb.Append(" WHERE xb.x67Entity='p28' AND xc.x68PermValue IN (1,2) AND (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1");

                //if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
                //{
                //    sb.Append($" OR xa.j11ID IN ({this.CurrentUser.j11IDs})");
                //}
                //sb.Append(")");
                //sb.Append(" AND (xa.x69RecordEntity='p28' AND xa.x69RecordPid=a.p28ID) OR (xa.x69RecordEntity='p29' AND xa.x69RecordPid=a.p29ID)");

            }





            sb.Append(")");

            AQ(sb.ToString(), "j02id_query", get_real_j02id_query());
        }
    }
}
