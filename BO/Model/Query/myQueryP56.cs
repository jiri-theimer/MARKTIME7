

namespace BO
{
    public class myQueryP56 : baseQuery
    {
        public int b02id { get; set; }
        public int p57id { get; set; }
        public int p41id { get; set; }
        public int p91id { get; set; }
        public int j02id { get; set; }
        public List<int> j02ids { get; set; }
        public int j02id_owner { get; set; }
        public int p55id { get; set; }
        public int p58id { get; set; }
        public int p28id { get; set; }
        
        public int j27id_query { get; set; }
        
        public int x67id { get; set; }
        public int leindex { get; set; }   //nadřízená vertikální úrověň projektu #1 - #4
        public int lepid { get; set; }     //nadřízená vertikální úrověň projektu, hodnota p41id

        public myQueryP56()
        {
            this.Prefix = "p56";
        }

        public override List<QRow> GetRows()
        {
            if (this.p31statequery > 0) this.Handle_p31StateQuery();
            if (!string.IsNullOrEmpty(this.p31tabquery)) this.Handle_p31TabQuery();

            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "p56DateInsert":
                        AQ("a.p56DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;

                    case "p56PlanFrom":
                        AQ("a.p56PlanFrom BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p56PlanUntil":
                        AQ("a.p56PlanUntil BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p56PlanFrom_or_p56PlanUntil":
                        AQ("(a.p56PlanUntil BETWEEN @d1 AND @d2 OR a.p56PlanFrom BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p91Date":
                        AQ("a.p56ID IN (select xa.p56ID FROM p31Worksheet xa INNER JOIN p91Invoice xb ON xa.p91ID=xb.p91ID WHERE xa.p56ID IS NOT NULL AND xb.p91Date BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                    case "p91DateSupply":
                        AQ("a.p56ID IN (select xa.p56ID FROM p31Worksheet xa INNER JOIN p91Invoice xb ON xa.p91ID=xb.p91ID WHERE xa.p56ID IS NOT NULL AND xb.p91DateSupply BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                    case "p31Date":
                    default:
                        AQ("a.p56ID IN (select p56ID FROM p31Worksheet WHERE p56ID IS NOT NULL AND p31Date BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                }
            }

            if (this.b02id > 0)
            {
                AQ("a.b02ID=@b02id)", "b02id", this.b02id);
            }
            if (this.p55id > 0)
            {
                AQ("a.p55ID=@p55id", "p55id", this.p55id);
            }
            if (this.p58id > 0)
            {
                AQ("a.p56ID IN (select p56ID_NewInstance FROM p59TaskRecurrence_Plan xa INNER JOIN p58TaskRecurrence xb ON xa.p58ID=xb.p58ID WHERE xb.p58ID=@p58id)", "p58id", this.p58id);
            }
            if (this.p28id > 0)
            {
                AQ("a.p41ID IN (select p41ID FROM p41Project WHERE p28ID_Client=@p28id)", "p28id", this.p28id);
            }
            if (this.p57id > 0)
            {
                AQ("a.p57ID=@p57id)", "p57id", this.p57id);
            }
            if (this.p41id > 0)
            {
                AQ("(a.p41ID=@p41id)", "p41id", this.p41id);
            }

            if (this.leindex > 0 && this.lepid > 0)
            {
                AQ($"p41x.p41ID_P07Level{this.leindex}=@lepid OR a.p41ID=@lepid", "lepid", this.lepid);
            }

            if (this.j02id_owner > 0)
            {
                AQ("a.j02ID_Owner=@ownerid)", "ownerid", this.j02id_owner);
            }
            

            if (this.p91id > 0)
            {
                AQ("a.p56ID IN (select p56ID FROM p31Worksheet WHERE p56ID IS NOT NULL AND p91ID=@p91id)", "p91id", this.p91id);
            }
            if (this.j02id > 0)
            {
                if (this.x67id > 0)
                {
                    //obdržel roli
                    AQ($"a.p56ID IN (SELECT x69.x69RecordPid FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID WHERE x67.x67Entity='p56' AND x69.x67ID={this.x67id} AND (x69.j02ID=@j02id OR x69.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID = @j02id) OR x69.x69IsAllUsers=1))", "j02id", this.j02id);
                }
                else
                {
                    //obdržel jakoukoliv roli v úkolu
                    AQ("a.p56ID IN (SELECT x69.x69RecordPid FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID WHERE x67.x67Entity='p56' AND (x69.j02ID=@j02id OR x69.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID = @j02id) OR x69.x69IsAllUsers=1))", "j02id", this.j02id);
                }
                
            }
            if (this.j02ids != null && this.j02ids.Count() > 0)
            {
                if (this.x67id > 0)
                {
                    //obdržel jakoukoliv roli v úkolu
                    AQ($"a.p56ID IN (SELECT x69.x69RecordPid FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID WHERE x67.x67Entity='p56' AND x69.x67ID={this.x67id} AND (x69.j02ID IN ({string.Join(", ", this.j02ids)}) OR x69.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID IN ({string.Join(", ", this.j02ids)})) OR x69.x69IsAllUsers=1))", "j02id", this.j02id);
                }
                else
                {
                    //obdržel jakoukoliv roli v úkolu
                    AQ($"a.p56ID IN (SELECT x69.x69RecordPid FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID WHERE x67.x67Entity='p56' AND (x69.j02ID IN ({string.Join(", ", this.j02ids)}) OR x69.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID IN ({string.Join(", ", this.j02ids)})) OR x69.x69IsAllUsers=1))", "j02id", this.j02id);
                }
                
            }

            if (this.j27id_query > 0)
            {
                AQ("EXISTS (select 1 FROM p31Worksheet WHERE p56ID=a.p56ID AND j27ID_Billing_Orig=@j27id_query)", "j27id_query", this.j27id_query);
            }

            if (this.x67id>0 && this.j02id==0 && this.j02ids == null)
            {
                AQ("a.p56ID IN (SELECT x69RecordPid FROM x69EntityRole_Assign WHERE x67ID=@x67id)", "x67id", this.x67id);
            }
            
            if (!string.IsNullOrEmpty(_searchstring))
            {
                string s = "";
                if (_searchstring.Length >1)
                {                   
                    //něco jako fulltext
                    s = "a.p56Name LIKE '%'+@expr+'%' COLLATE Latin1_General_CI_AI OR a.p56Code LIKE '%'+@expr+'%' COLLATE Latin1_General_CI_AI";
                    s += " OR a.p41ID IN (select xa.p41ID FROM p41Project xa LEFT OUTER JOIN p28Contact xb ON xa.p28ID_Client=xb.p28ID WHERE xa.p41NameShort LIKE '%'+@expr+'%' OR xa.p41Name LIKE '%'+@expr+'%' OR xb.p28Name LIKE '%'+@expr+'%')";
                    s += " OR a.p56ID IN (select xa.x71RecordPid FROM x71EntityRole_Inline xa INNER JOIN x67EntityRole xb ON xa.x67ID=xb.x67ID WHERE xb.x67Entity='p56' AND xa.x71Value LIKE '%'+@expr+'%')";
                   

                }
                
                AQ(s, "expr", _searchstring);

            }

            if (this.MyRecordsDisponible || this.MyRecordsDisponible_Approve)
            {
                Handle_MyDisponible();
            }


            return this.InhaleRows();

        }

        private void Handle_MyDisponible()
        {
            if (this.MyRecordsDisponible_Approve)
            {
                //pouze projekty povolené ke schvalování
                if (this.CurrentUser.TestPermission(PermValEnum.GR_P31_Approver)) return; //oprávnění schvalovat všechny úkony
            }
            else
            {
                if (this.CurrentUser.IsAdmin || this.CurrentUser.TestPermission(PermValEnum.GR_p56_Reader) || this.CurrentUser.TestPermission(PermValEnum.GR_p56_Owner))
                {
                    return; //přístup ke všem úkolům
                }
            }
            
            var sb = new System.Text.StringBuilder();
            if (this.MyRecordsDisponible_Approve)
            {
                sb.Append(" EXISTS (SELECT 1 FROM p41Project ca INNER JOIN x69EntityRole_Assign xa ON ca.p41ID=xa.x69RecordPid INNER JOIN x67EntityRole xb ON xa.x67ID=xb.x67ID");
                sb.Append(" INNER JOIN o28ProjectRole_Workload xc ON xb.x67ID=xc.x67ID");
                sb.Append(" WHERE ca.p41ID=a.p41ID");
                sb.Append(" AND xb.x67Entity='p41' AND xc.o28PermFlag IN (3,4) AND (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1");

                if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
                {
                    sb.Append($" OR xa.j11ID IN ({this.CurrentUser.j11IDs})");
                }
                sb.Append(")");

                if (this.CurrentUser.x69ID_p41_p28 > 0)
                {
                    sb.Append(" OR (xa.x69RecordEntity='p28' AND xa.x69RecordPid=ca.p28ID_Client)");    //projektové role nastavené v klientovi 
                }
                if (this.CurrentUser.x69ID_p41_j18 > 0)
                {
                    sb.Append(" OR (xa.x69RecordEntity='j18' AND xa.x69RecordPid=ca.j18ID)");    //projektové role nastavené ve středisku 
                }
            }
            else
            {
                sb.Append("a.j02ID_Owner=@j02id_query");
                sb.Append(" OR EXISTS (SELECT 1 FROM x69EntityRole_Assign xa inner join x67EntityRole xb ON xa.x67ID=xb.x67ID");
                sb.Append(" WHERE xb.x67Entity='p56' and (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1");

                if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
                {
                    sb.Append($" OR xa.j11ID IN ({this.CurrentUser.j11IDs})");
                }
                sb.Append(")");
                sb.Append(" AND xa.x69RecordEntity='p56' AND xa.x69RecordPid=a.p56ID");
            }



            
            sb.Append(")");

            AQ(sb.ToString(), "j02id_query", get_real_j02id_query());
        }
    }
}
