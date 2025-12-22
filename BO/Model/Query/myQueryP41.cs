

namespace BO
{
    public class myQueryP41 : baseQuery
    {
        public int p42id { get; set; }
        public int p51id { get; set; }
        public int p07level { get; set; }
        public int j02id_owner { get; set; }

        public int p15id { get; set; }
        
        public int b02id { get; set; }
        public int j18id { get; set; }
        public int p61id { get; set; }
        public int p91id { get; set; }
        public int p28id { get; set; }
        public bool notquery_disponible { get; set; }   //zobrazit všechny projekty bez testování přístupových práv
        
        public int p34id_for_p31_entry { get; set; }    //omezovat na projekty pro vykazování v sešitu p34id_for_p31_entry
        public int p33id_for_p31_entry { get; set; }
        public int p34incomestatementflag_for_p31_entry { get; set; }

        public int j02id_for_p56_o22_entry { get; set; }    //omezovat na projekty, kde uživatel může založit úkol nebo termín/lhůtu

        public int j27id_query { get; set; }
        public int leindex { get; set; }   //nadřízená vertikální úrověň #1 - #4
        public int lepid { get; set; }     //nadřízená vertikální úrověň, hodnota p41id

        public DateTime? AvailableCapacityD1 { get; set; }
        public DateTime? AvailableCapacityD2 { get; set; }
        public myQueryP41(string prefix)
        {
            this.Prefix = prefix;
        }

        public override List<QRow> GetRows()
        {
            if (this.p31statequery > 0) this.Handle_p31StateQuery();
            if (!string.IsNullOrEmpty(this.p31tabquery)) this.Handle_p31TabQuery();

            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "p41DateInsert":
                        AQ("a.p41DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p41PlanFrom":
                        AQ("a.p41PlanFrom BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p41PlanUntil":
                        AQ("a.p41PlanUntil BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p91Date":
                        AQ("EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p91Invoice xb ON xa.p91ID=xb.p91ID WHERE xa.p41ID=a.p41ID AND xb.p91Date BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                    case "p91DateSupply":
                        AQ("EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p91Invoice xb ON xa.p91ID=xb.p91ID WHERE xa.p41ID=a.p41ID AND xb.p91DateSupply BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                    case "p31Date":
                    default:                        
                        AQ("EXISTS (select p31ID FROM p31Worksheet WHERE p41ID=a.p41ID AND p31Date BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                }
            }


            if (this.iswip != null)
            {
                if (this.iswip == true)
                {
                    AQ("EXISTS (select 1 FROM p31Worksheet WHERE p41ID=a.p41ID AND p71ID IS NULL AND p91ID IS NULL AND p31ExcludeBillingFlag IS NULL AND p31Date between @p31date1 AND @p31date2)", null, null);
                }
                else
                {
                    AQ("NOT EXISTS (select 1 FROM p31Worksheet WHERE p41ID=a.p41ID AND p71ID IS NULL AND p91ID IS NULL AND p31Date between @p31date1 AND @p31date2)", null, null);
                }
            }
            if (this.isapproved_and_wait4invoice != null)
            {
                
                if (this.isapproved_and_wait4invoice == true)
                {
                    //AQ("EXISTS (select 1 FROM p31Worksheet WHERE p41ID=a.p41ID AND p71ID=1 AND p72ID_AfterApprove=4 AND p91ID IS NULL AND p31Date between @p31date1 AND @p31date2)", null, null);
                    AQ("EXISTS (select 1 FROM p31Worksheet WHERE p41ID=a.p41ID AND p71ID=1 AND p91ID IS NULL AND p31Date between @p31date1 AND @p31date2)", null, null);
                }
                //else
                //{
                //    AQ("NOT EXISTS (select 1 FROM p31Worksheet WHERE p41ID=a.p41ID AND p71ID=1 AND p72ID_AfterApprove=4 AND p91ID IS NULL AND p31Date between @p31date1 AND @p31date2)", null, null);
                //}
            }

            if (this.p07level > 0)
            {
                //AQ("p07x.p07Level=@p07level", "p07level", this.p07level);
                AQ("p07x.p07Level=" + this.p07level.ToString(), null, null);
            }
            if (this.p42id > 0)
            {
                AQ("a.p42ID=@p42id", "p42id", this.p42id);
            }

            if (this.p51id > 0)
            {
                AQ("(a.p51ID_Billing=@p51id OR a.p51ID_Internal=@p51id)", "p51id", this.p51id);
            }
            if (this.j02id_owner > 0)
            {
                AQ("a.j02ID_Owner=@ownerid", "ownerid", this.j02id_owner);
            }

          
            if (this.leindex > 0 && this.lepid > 0)
            {
                AQ($"a.p41ID_P07Level{this.leindex}=@lepid", "lepid", this.lepid);
            }

            if (this.p61id > 0)
            {
                AQ("a.p32ID IN (SELECT p32ID FROM p62ActivityCluster_Item WHERE p61ID=@p61id)", "p61id", this.p61id);
            }
            if (this.b02id > 0)
            {
                AQ("a.b02ID=@b02id", "b02id", this.b02id);
            }
            if (this.p15id > 0)
            {
                AQ("a.p15ID=@p15id", "p15id", this.p15id);
            }
            if (this.j18id > 0)
            {
                AQ("a.j18ID=@j18id", "j18id", this.j18id);
            }
            if (this.p61id > 0)
            {
                AQ("a.p61ID=@p61id", "p61id", this.p61id);
            }
            if (this.p91id > 0)
            {
                AQ("a.p41ID IN (select p41ID FROM p31Worksheet WHERE p91ID=@p91id)", "p91id", this.p91id);
            }
            if (this.p28id > 0)
            {
                AQ("(a.p28ID_Client=@p28id OR a.p28ID_Billing=@p28id)", "p28id", this.p28id);
            }

            if (this.j27id_query > 0)
            {
                AQ("EXISTS (select 1 FROM p31Worksheet WHERE p41ID=a.p41ID AND j27ID_Billing_Orig=@j27id_query)", "j27id_query", this.j27id_query);
            }

            if (this.AvailableCapacityD1 != null && this.AvailableCapacityD2 != null) //projekty k dispozici pro kapacitní plánování
            {
                DateTime d1 = Convert.ToDateTime(this.AvailableCapacityD1);
                DateTime d2 = Convert.ToDateTime(this.AvailableCapacityD2);
                AQ("a.p41PlanFrom BETWEEN @xd1 AND @xd2 OR a.p41PlanUntil BETWEEN @xd1 AND @xd2 OR (a.p41PlanFrom<=@xd1 AND a.p41PlanUntil>=@xd2) OR EXISTS(select x1.r01ID FROM r01Capacity x1 INNER JOIN r05CapacityUnit x2 ON x1.r01ID=x2.r01ID WHERE x1.p41ID=a.p41ID AND x2.r05Date BETWEEN @xd1 AND @xd2)", "xd1", d1, "AND", null, null, "xd2", d2);
            }

            if (!string.IsNullOrEmpty(_searchstring))
            {
                string s = null;
                if (_searchstring.Length == 1)
                {
                    //hledat pouze podle počátečních písmen
                    s = "a.p41Name COLLATE Latin1_General_CI_AI LIKE @expr+'%' OR a.p41Code LIKE '%'+@expr+'%' OR a.p41NameShort LIKE @expr+'%'";
                    s += " OR a.p28ID_Client IN (select p28ID FROM p28Contact WHERE p28Name COLLATE Latin1_General_CI_AI LIKE @expr+'%' OR p28CompanyName LIKE @expr+'%')";
                }
                else
                {
                    //něco jako fulltext
                    s = "a.p41Name COLLATE Latin1_General_CI_AI LIKE '%'+@expr+'%' OR a.p41Code LIKE '%'+@expr+'%' OR a.p41NameShort LIKE '%'+@expr+'%'";
                    s += " OR a.p28ID_Client IN (select p28ID FROM p28Contact WHERE p28Name COLLATE Latin1_General_CI_AI LIKE '%'+@expr+'%' OR p28CompanyName LIKE '%'+@expr+'%')";
                }
                
                AQ(s, "expr", _searchstring);

            }

            
            if (this.p34id_for_p31_entry>0 || this.p33id_for_p31_entry>0)
            {
                //projekty povolené pro vykazování úkonů
                Handle_Vykazovani();
            }
            else
            {
                if (this.j02id_for_p56_o22_entry > 0)
                {                    
                    Handle_UkolyTerminy();  //projekty, kde uživatel může zakládat úkoly a termíny/lhůty
                }
                else
                {
                    Handle_MyDisponible();  //automaticky se zužuje okruh projektů pouze na přístupné
                }
                
            }
            

            return this.InhaleRows();

        }

        private void Handle_UkolyTerminy()
        {
            if (this.notquery_disponible) return;
            if (this.CurrentUser.TestPermission(PermValEnum.GR_p41_Owner)) return;
            if (this.CurrentUser.TestPermission(PermValEnum.GR_P31_Creator_Hours)) return;  //pokud může vykazovat hodiny všude, může i zakládat úkoly/termíny v každém projektu

            var sb = new System.Text.StringBuilder();
            sb.Append("EXISTS (SELECT 1 FROM x69EntityRole_Assign xa INNER JOIN x67EntityRole xb ON xa.x67ID=xb.x67ID INNER JOIN x68EntityRole_Permission xc ON xb.x67ID=xc.x67ID");
            sb.Append(" WHERE xc.x68PermValue=7");
            sb.Append(" AND (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1");
            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                sb.Append($" OR xa.j11ID IN ({this.CurrentUser.j11IDs})");
            }
            sb.Append(")");
            sb.Append(" AND ((xa.x69RecordEntity='p41' AND xa.x69RecordPid=a.p41ID)");

            if (this.CurrentUser.x69ID_p41_p28 > 0)
            {
                sb.Append(" OR (xa.x69RecordEntity='p28' AND xa.x69RecordPid=a.p28ID_Client)");    //projektové role nastavené v klientovi 
            }
            if (this.CurrentUser.x69ID_p41_j18 > 0)
            {
                sb.Append(" OR (xa.x69RecordEntity='j18' AND xa.x69RecordPid=a.j18ID)");    //projektové role nastavené ve středisku 
            }
            sb.Append(")");
            sb.Append(")");

            AQ(sb.ToString(), "j02id_query", this.j02id_for_p56_o22_entry);
        }

        private void Handle_Vykazovani()
        {            
            bool bolAllHours = this.CurrentUser.TestPermission(PermValEnum.GR_P31_Creator_Hours);
            bool bolAllExpenses = this.CurrentUser.TestPermission(PermValEnum.GR_P31_Creator_Expenses);
            bool bolAllFees = this.CurrentUser.TestPermission(PermValEnum.GR_P31_Creator_Fees);
            if (bolAllHours && bolAllExpenses && bolAllFees)
            {
                return; //může vykazovat do všech sešitů
            }
            if (bolAllHours && (p33id_for_p31_entry==1 || p33id_for_p31_entry == 3))
            {
                return; //může vykazovat ve všech projektech hodiny a kusovník
            }
            if (bolAllExpenses && this.p34incomestatementflag_for_p31_entry==1 && (p33id_for_p31_entry == 2 || p33id_for_p31_entry == 5))
            {
                return; //může vykazovat ve všech projektech peněžní výdaje
            }
            if (bolAllFees && this.p34incomestatementflag_for_p31_entry == 2 && (p33id_for_p31_entry == 2 || p33id_for_p31_entry == 5))
            {
                return; //může vykazovat ve všech projektech pevné odměny
            }
            
            //Oprávnění k projektu vyplývající z projektové role přímo v projektu nebo nepřímo ve středisku projektu nebo v klientovi projektu:
            var sb = new System.Text.StringBuilder();
            sb.Append("EXISTS (SELECT 1 FROM x69EntityRole_Assign xa inner join x67EntityRole xb ON xa.x67ID=xb.x67ID INNER JOIN o28ProjectRole_Workload xc ON xb.x67ID=xc.x67ID");
            sb.Append(" WHERE xb.x67Entity='p41' AND xc.o28EntryFlag>0");
            if (this.p34id_for_p31_entry > 0)
            {
                sb.Append(" AND xc.p34ID=@p34id");
            }
            

            sb.Append(" AND (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1");

            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                sb.Append($" OR xa.j11ID IN ({this.CurrentUser.j11IDs})");
            }
            sb.Append(")");
            sb.Append(" AND ((xa.x69RecordEntity='p41' AND xa.x69RecordPid=a.p41ID) OR (xa.x69RecordEntity='j18' AND xa.x69RecordPid=a.j18ID) OR (xa.x69RecordEntity='p28' AND xa.x69RecordPid=a.p28ID_Client)");
           
            sb.Append(")");
            sb.Append(")");

            
            if (this.p34id_for_p31_entry > 0)
            {
                AQ(sb.ToString(), "j02id_query", get_real_j02id_query(), "AND", null, null, "p34id", this.p34id_for_p31_entry);
            }
            else
            {
                AQ(sb.ToString(), "j02id_query", get_real_j02id_query());
            }
                

            
        }

        private void Handle_MyDisponible()
        {
            if (this.notquery_disponible) return;

            if (this.MyRecordsDisponible_Approve)
            {
                //pouze projekty povolené ke schvalování
                if (this.CurrentUser.TestPermission(PermValEnum.GR_P31_Approver)) return; //oprávnění schvalovat všechny úkony
            }
            else
            {
                if (this.CurrentUser.TestPermission(PermValEnum.GR_p41_Owner) || this.CurrentUser.TestPermission(PermValEnum.GR_p41_Reader))
                {
                    return; //přístup ke všem projektům v systému
                }
            }
            

            //Oprávnění k projektu vyplývající z projektové role přímo v projektu nebo nepřímo ve středisku projektu nebo v klientovi projektu:
            var sb = new System.Text.StringBuilder();            
            
            //sb.Append("EXISTS (SELECT 1 FROM x69EntityRole_Assign xa INNER JOIN x67EntityRole xb ON xa.x67ID=xb.x67ID");
            if (this.MyRecordsDisponible_Approve)
            {
                sb.Append("EXISTS (SELECT x72ID FROM x72ProjectRole_Permission WHERE p41ID=a.p41ID AND o28PermFlag IN (3,4) AND x72IsValid=1 AND (j02ID=@j02id_query OR x72IsAllUsers=1");
               
            }
            else
            {
                sb.Append("EXISTS (SELECT x72ID FROM x72ProjectRole_Permission WHERE p41ID=a.p41ID AND x72IsValid=1 AND (j02ID=@j02id_query OR x72IsAllUsers=1");                
            }
            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                sb.Append($" OR j11ID IN ({this.CurrentUser.j11IDs})");
            }
            sb.Append(")");
            sb.Append(")");

            
            //sb.Append(")");
            ////sb.Append(" AND ((xa.x69RecordEntity='p41' AND xa.x69RecordPid=a.p41ID) OR (xa.x69RecordEntity='j18' AND xa.x69RecordPid=a.j18ID) OR (xa.x69RecordEntity='p28' AND xa.x69RecordPid=a.p28ID_Client)");

            //sb.Append(" AND ((xa.x69RecordEntity='p41' AND xa.x69RecordPid=a.p41ID)");

            //if (this.CurrentUser.x69ID_p41_p28 > 0)
            //{
            //    sb.Append(" OR (xa.x69RecordEntity='p28' AND xa.x69RecordPid=a.p28ID_Client)");    //projektové role nastavené v klientovi 
            //}
            //if (this.CurrentUser.x69ID_p41_j18 > 0)
            //{
            //    sb.Append(" OR (xa.x69RecordEntity='j18' AND xa.x69RecordPid=a.j18ID)");    //projektové role nastavené ve středisku 
            //}


            //if (this.p07level == 0 && 1 == 2) //zatím nepoužívat!
            //{
            //    //v jednom přehledu projekty z více vertikálních úrovní
            //    sb.Append(" OR EXISTS (select 1 FROM p41Project WHERE p41TreeIndex BETWEEN a.p41TreePrev AND a.p41TreeNext)");
            //}
            //sb.Append(")");
            //sb.Append(")");



            AQ(sb.ToString(), "j02id_query", get_real_j02id_query());
        }

    }


}
