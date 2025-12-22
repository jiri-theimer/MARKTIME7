
namespace BO
{
   
    public class myQueryP31 : baseQuery
    {
        public int j02id { get; set; }
        public List<int> j02ids { get; set; }
        public int j11id { get; set; }
        public List<int> j11ids { get; set; }
        public List<int> j07ids { get; set; }
        public int p28id { get; set; }
        public int p41id { get; set; }
        public int p56id { get; set; }
        public int p40id { get; set; }  //vygenerované úkony v rámci opakované odměny
        public bool? hodiny_v_p40id { get; set; }   //hodiny v opakované odměně
        public List<int> p41ids { get; set; }
        public List<int> p28ids { get; set; }
        public List<int> p56ids { get; set; }
        public int p61id { get; set; }
        public int p32id { get; set; }
        public List<int> p32ids { get; set; }
       
        public int p91id { get; set; }
        public int p51id { get; set; }
        public int p51id_billingrate { get; set; }
        public int p51id_costrate { get; set; }
        public int o23id { get; set; }
        public int p70id { get; set; }

        public int leindex { get; set; }   //nadřízená vertikální úrověň #1 - #4 - funguje dohromady i s p41ids
        public int lepid { get; set; }     //nadřízená vertikální úrověň, hodnota p41id
        public bool bez_podrizenych { get; set; }
        public bool? p32isbillable { get; set; }
        public int p71id { get; set; }
        public int p72id_afterapprove { get; set; }
        public string tabquery { get; set; }
        
        public bool ischangelog { get; set; } //zobrazovat záznamy z tabulky p31Worksheet_Log
        public string tempguid { get; set; }
        public string p85guid { get; set; }

        public bool? iswip_or_excluded { get; set; }    //rozpracované nebo vyloučené z vyúčtování
        
        public int p31masterid { get; set; }
        public int p31masterid_includemaster { get; set; }
        public myQueryP31()
        {
            this.Prefix = "p31";
            this.IsRecordValid = null;  //v p31worksheet si nehrajeme na archiv!
        }

        public override List<QRow> GetRows()
        {
            if (this.p31statequery > 0) Handle_p31StateQuery_p31();
            if (!string.IsNullOrEmpty(this.p31tabquery))
            {
                this.tabquery = this.p31tabquery;
            }

            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "p31DateInsert":
                        AQ("a.p31DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p91Date":
                        AQ("p91x.p91Date BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                    case "p91DateSupply":
                        AQ("p91x.p91DateSupply BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                    case "p91DateBilled":
                        AQ("p91x.p91DateBilled BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                    case "p31Approved_When":
                        AQ("a.p31Approved_When BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p31Date":
                    default:
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("a.p31Date BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("a.p31Date = @d1", "d1", this.global_d1_query);
                        }
                        break;
                }
            }

            if (this.j02id > 0)
            {
                AQ("a.j02ID=@j02id", "j02id", this.j02id);
            }
            if (this.j02ids != null && this.j02ids.Count() > 0)
            {
                AQ($"a.j02ID IN ({string.Join(",", this.j02ids)})", null, null);
            }
            if (this.j11id > 0)
            {
                AQ("a.j02ID IN (select j02ID FROM j12Team_Person WHERE j11ID=@j11id)", "j11id", this.j11id);
            }
            if (this.j11ids != null && this.j11ids.Count() > 0)
            {
                AQ($"a.j02ID IN (select j02ID FROM j12Team_Person WHERE j11ID IN ({string.Join(",", this.j11ids)}))", null, null);
            }
            if (this.j07ids != null && this.j07ids.Count() > 0)
            {
                AQ($"a.j02ID IN (select j02ID FROM j02User WHERE j07ID IN ({string.Join(",", this.j07ids)}))", null, null);
            }
            if (this.p56id > 0)
            {
                AQ("a.p56ID=@p56id", "p56id", this.p56id);
            }
            if (this.p31masterid > 0)
            {
                AQ("a.p31MasterID=@p31masterid", "p31masterid", this.p31masterid);
            }
            if (this.p31masterid_includemaster > 0)
            {
                AQ("(a.p31MasterID=@p31masterid_includemaster OR a.p31ID=@p31masterid_includemaster)", "p31masterid_includemaster", this.p31masterid_includemaster);
            }

            if (this.p40id > 0)
            {
                if (this.hodiny_v_p40id == true)
                {
                    AQ("a.p40ID_FixPrice=@p40id", "p40id", this.p40id); //Úkony v paušálu
                }
                else
                {
                    AQ("a.p40ID_Source=@p40id", "p40id", this.p40id); //vygenerované úkony v předpisu
                    //AQ("EXISTS(select p39ID FROM p39WorkSheet_Recurrence_Plan WHERE p40ID=@p40id AND p31ID_NewInstance=a.p31ID)", "p40id", this.p40id); //vygenerované úkony v předpisu
                }
                
            }
            
            if (this.p41id > 0)
            {
                AQ("a.p41ID=@p41id", "p41id", this.p41id);
            }
            if (this.p41ids != null && this.p41ids.Count()>0)
            {
                if (this.leindex > 0 && this.leindex <5)
                {
                    AQ($"p41x.p41ID_P07Level{this.leindex} IN ({string.Join(",", this.p41ids)})", null, null);
                }
                else
                {
                    AQ($"a.p41ID IN ({string.Join(",", this.p41ids)})", null, null);
                }
                
            }
            if (this.p28id > 0)
            {
                AQ("a.p41ID IN (select p41ID FROM p41Project WHERE p28ID_Client=@p28id)", "p28id", this.p28id);
            }
            if (this.p28ids != null && this.p28ids.Count() > 0)
            {
                AQ($"a.p41ID IN (select p41ID FROM p41Project WHERE p28ID_Client IN ({string.Join(",", this.p28ids)}))",null,null);                
            }
            if (this.p56ids != null && this.p56ids.Count() > 0)
            {
                AQ($"a.p56ID IN ({string.Join(",", this.p56ids)})", null, null);
            }
            if (this.p91id > 0)
            {
                AQ("a.p91ID=@p91id", "p91id", this.p91id);
            }
            
            if (this.p32id > 0)
            {
                AQ("a.p32ID=@p32id", "p32id", this.p32id);
            }
            if (this.p32ids != null && this.p32ids.Count()>0)
            {
                AQ("a.p32ID IN (" + string.Join(",", this.p32ids) + ")", null, null);
            }
            if (this.p61id > 0)
            {
                AQ($"a.p32ID IN (select p32ID FROM p62ActivityCluster_Item WHERE p61ID={this.p61id})",null,null);
            }
            if (this.o23id > 0)
            {
                AQ("a.o23ID=@o23id", "o23id", this.o23id);
            }
            
            if (this.p51id > 0)
            {
                //AQ("(a.p51ID_BillingRate=@p51id OR a.p51ID_CostRate=@p51id)", "p51id", this.p51id);
                AQ("a.p51ID_BillingRate=@p51id OR a.p51ID_CostRate=@p51id OR a.p41ID IN (select xa.p41ID FROM p41Project xa LEFT OUTER JOIN p28Contact xb ON xa.p28ID_Client=xb.p28ID WHERE xa.p51ID_Billing=@p51id OR ISNULL(xb.p51ID_Billing,0)=@p51id)", "p51id", this.p51id);

            }

            if (this.p51id_billingrate > 0)
            {
                AQ("a.p51ID_BillingRate=@p51id_billingrate OR a.p41ID IN (select xa.p41ID FROM p41Project xa LEFT OUTER JOIN p28Contact xb ON xa.p28ID_Client=xb.p28ID WHERE xa.p51ID_Billing=@p51id_billingrate OR ISNULL(xb.p51ID_Billing,0)=@p51id_billingrate)", "p51id_billingrate", this.p51id_billingrate);
            }
            if (this.p51id_costrate > 0)
            {
                AQ("a.p51ID_CostRate=@p51id_costrate", "p51id_costrate", this.p51id_costrate);
            }
            if (this.p70id > 0)
            {
                if (this.p70id == 23)
                {
                    AQ("a.p70ID IN (2,3)",null,null);
                }
                else
                {
                    AQ("a.p70ID=@p70id", "p70id", this.p70id);
                }
                
            }
            if (this.p72id_afterapprove > 0)
            {
                if (this.p72id_afterapprove == 23)
                {
                    AQ("a.p72ID_AfterApprove IN (2,3)", null, null);
                }
                else
                {
                    AQ("a.p72ID_AfterApprove=@p72id", "p72id", this.p72id_afterapprove);
                }

            }
            if (this.p71id > 0)
            {
                AQ("a.p71ID=@p71id", "p71id", this.p71id);
            }
            if (this.p32isbillable != null)
            {
                AQ("p32x.p32IsBillable=@billable","billable",this.p32isbillable);
            }
            
            if (this.iswip != null)
            {
                if (this.iswip == true)
                {
                    
                    AQ("a.p71ID IS NULL AND a.p91ID IS NULL AND a.p31ExcludeBillingFlag IS NULL AND p41x.p41BillingFlag<99", null, null);
                }
                else
                {
                    
                    AQ("a.p71ID IS NOT NULL", null, null);
                }                
            }
            if (this.iswip_or_excluded != null)
            {
                if (this.iswip_or_excluded == true)
                {

                    AQ("((a.p71ID IS NULL AND a.p91ID IS NULL AND p41x.p41BillingFlag<99) OR (a.p31ExcludeBillingFlag=1))", null, null);
                }
                else
                {

                    AQ("a.p71ID IS NOT NULL", null, null);
                }
            }
            if (this.isinvoiced != null)
            {
                if (this.isinvoiced == true)
                {
                    AQ("a.p91ID IS NOT NULL", null, null);
                }
                else
                {
                    AQ("a.p91ID IS NULL AND p41x.p41BillingFlag<99 AND a.p31ExcludeBillingFlag IS NULL", null, null);
                }
            }
            if (this.isapproved_and_wait4invoice != null)
            {
                if (this.isapproved_and_wait4invoice == true)
                {
                    
                    AQ("a.p71ID=1 AND a.p91ID IS NULL", null, null);
                }
                else
                {
                    
                    AQ("a.p91ID IS NULL", null, null);
                }
            }

            if (this.tabquery != null)
            {
                switch (this.tabquery)
                {
                    case "time":
                        AQ("p34x.p33ID=1", null, null); break;
                    case "expense":
                        AQ("p34x.p33ID IN (2,5) AND p34x.p34IncomeStatementFlag=1", null, null); break;
                    case "fee":
                        AQ("p34x.p33ID IN (2,5) AND p34x.p34IncomeStatementFlag=2", null, null); break;
                    case "kusovnik":
                        AQ("p34x.p33ID=3", null, null); break;
                    case "time_or_kusovnik":
                        AQ("p34x.p33ID IN (1,3)", null, null); break;
                }
            }


            if (this.leindex > 0 && this.lepid>0 && this.p41id==0)
            {
                AQ($"p41x.p41ID_P07Level{this.leindex}=@lepid OR a.p41ID=@lepid", "lepid", this.lepid);
            }
            


            if (!string.IsNullOrEmpty(_searchstring) && _searchstring.Length>=3)
            {                
                string s = "p41x.p41Code LIKE '%'+@expr+'%' OR p41x.p41Name LIKE '%'+@expr+'%' OR a.p31Text COLLATE Latin1_general_CI_AI LIKE '%'+@expr+'%' COLLATE Latin1_general_CI_AI OR p32x.p32Name like '%'+@expr+'%'";
                s += " OR p41x.p28ID_Client IN (select p28ID FROM p28Contact WHERE p28Name LIKE '%'+@expr+'%' OR p28CompanyName LIKE '%'+@expr+'%')";
                s += " OR a.j02ID IN (select j02ID FROM j02User WHERE j02LastName LIKE '%'+@expr+'%')";
                AQ(s, "expr", _searchstring);

            }

            if (this.tempguid != null)
            {
                //platí pro p31Worksheet_Temp
                AQ("a.p31Guid=@p31guid", "p31guid", this.tempguid);
                //AQ("a.p31Guid='"+this.tempguid+"'",null,null);
            }
            if (this.p85guid != null)
            {
                AQ("a.p31ID IN (select p85DataPID FROM p85TempBox WHERE p85Guid=@p85guid)", "p85guid", this.p85guid);
            }

            if (this.MyRecordsDisponible_Approve)
            {
                this.MyRecordsDisponible = false;
                Handle_MyDisponible_Approve();
            }
            if (this.MyRecordsDisponible)
            {
                Handle_MyDisponible();
            }
            if (this.CurrentUser.j02WorksheetOperFlag == 2) //skrýt před uživatelem vyúčtované úkony
            {                
                AQ("a.p91ID IS NULL", null, null);
            }

            return this.InhaleRows();

        }

        private void Handle_MyDisponible()
        {
           
            if (this.CurrentUser.TestPermission(PermValEnum.GR_P31_Reader)) return; //oprávnění vidět všechny záznamy

            var sb = new System.Text.StringBuilder();            

            sb.Append("a.j02ID=@j02id_query");
            sb.Append(" OR EXISTS (SELECT x72ID FROM x72ProjectRole_Permission WHERE p41ID=a.p41ID AND p34ID=p32x.p34ID AND x72IsValid=1 AND o28PermFlag>0 AND (j02ID=@j02id_query OR x72IsAllUsers=1");
            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                sb.Append($" OR j11ID IN ({this.CurrentUser.j11IDs})");
            }
            sb.Append(")");            
            sb.Append(")");
            
            if (this.CurrentUser.IsMasterPerson)
            {
                if (this.CurrentUser.MasterSlave_j02IDs != null)
                {
                    sb.Append($" OR a.j02ID IN ({this.CurrentUser.MasterSlave_j02IDs})");
                }
                else
                {
                    sb.Append(" OR a.j02ID IN (select j02ID_Slave FROM j05MasterSlave WHERE j02ID_Master=@j02id_query AND j02ID_Slave IS NOT NULL)");
                    sb.Append(" OR a.j02ID IN (select xb.j02ID FROM j05MasterSlave xa INNER JOIN j12Team_Person xb ON xa.j11ID_Slave=xb.j11ID WHERE xa.j02ID_Master=@j02id_query AND xa.j11ID_Slave IS NOT NULL)");
                    //sb.Append(" OR EXISTS (SELECT 1 FROM j05MasterSlave WHERE j02ID_Master=@j02id_query AND j02ID_Slave=a.j02ID)");
                    //sb.Append(" OR EXISTS (SELECT 1 FROM j05MasterSlave xa INNER JOIN j12Team_Person xb ON xa.j11ID_Slave=xb.j11ID WHERE xa.j02ID_Master=@j02id_query AND xb.j02ID=a.j02ID)");
                }
            }
            

           

            AQ(sb.ToString(), "j02id_query", get_real_j02id_query());

        }

        private void Handle_MyDisponible_Approve()
        {
            if (this.CurrentUser.TestPermission(PermValEnum.GR_P31_Approver)) return; //oprávnění schvalovat všechny záznamy

            var sb = new System.Text.StringBuilder();

            sb.Append("EXISTS (SELECT x72ID FROM x72ProjectRole_Permission WHERE p41ID=a.p41ID AND p34ID=p32x.p34ID AND o28PermFlag IN (3,4) AND x72IsValid=1 AND (j02ID=@j02id_query OR x72IsAllUsers=1");
            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                sb.Append($" OR j11ID IN ({this.CurrentUser.j11IDs})");
            }
            sb.Append(")");
            sb.Append(")");
           
            if (this.CurrentUser.IsMasterPerson)
            {
                if (this.CurrentUser.MasterSlave_Approve_j02IDs != null)
                {
                    sb.Append($" OR a.j02ID IN ({this.CurrentUser.MasterSlave_Approve_j02IDs})");                    
                }
                else
                {
                    //sb.Append(" OR a.j02ID IN (SELECT j02ID_Slave FROM j05MasterSlave WHERE j02ID_Master=@j02id_query AND j05Disposition_p31 IN (3,4) AND j02ID_Slave IS NOT NULL)");
                    sb.Append(" OR EXISTS(SELECT j05ID FROM j05MasterSlave WHERE j02ID_Master=@j02id_query AND j05Disposition_p31 IN (3,4))");
                    //sb.Append(" OR a.j02ID IN (select xb.j02ID FROM j05MasterSlave xa INNER JOIN j12Team_Person xb ON xa.j11ID_Slave=xb.j11ID WHERE xa.j02ID_Master=@j02id_query AND xa.j05Disposition_p31 IN (3,4) AND xa.j11ID_Slave IS NOT NULL)");
                    sb.Append(" OR EXISTS (SELECT xa.j05ID FROM j05MasterSlave xa INNER JOIN j12Team_Person xb ON xa.j11ID_Slave=xb.j11ID WHERE xa.j02ID_Master=@j02id_query AND xb.j02ID=a.j02ID AND xa.j05Disposition_p31 IN (3,4))");
                }
            }


            AQ(sb.ToString(), "j02id_query", get_real_j02id_query());

        }

        private void Handle_p31StateQuery_p31()
        {
            
            switch (this.p31statequery)
            {
                case 1: //Rozpracované                    
                    this.iswip = true;
                    break;

                case 2://rozpracované s korekcí                    
                    AQ("a.p71ID IS NULL AND a.p91ID IS NULL AND a.p72ID_AfterTrimming IS NOT NULL AND a.p31ExcludeBillingFlag IS NULL", null, null); break;
                case 3://nevyúčtované                    
                    AQ("a.p91ID IS NULL AND a.p31ExcludeBillingFlag IS NULL AND p41x.p41BillingFlag<99", null, null); break;
                case 4://schválené
                    this.isapproved_and_wait4invoice = true; break;  //AQ("a.p71ID=1 AND a.p91ID IS NULL", null, null); break;
                case 5://schválené jako fakturovat
                    AQ("a.p71ID=1 AND a.p72ID_AfterApprove=4 AND a.p91ID IS NULL", null, null); break;
                case 6://schválené jako paušál
                    AQ("a.p71ID=1 AND a.p72ID_AfterApprove=6 AND a.p91ID IS NULL", null, null); break;
                case 7://schválené jako odpis
                    AQ("a.p71ID=1 AND a.p72ID_AfterApprove IN (2,3) AND a.p91ID IS NULL", null, null); break;
                case 8://schválené jako fakturovat později
                    AQ("a.p71ID=1 AND a.p72ID_AfterApprove=7 AND a.p91ID IS NULL", null, null); break;
                case 9://neschválené
                    AQ("a.p71ID=2 AND a.p91ID IS NULL", null, null); break;
                case 10://vyúčtované
                    this.isinvoiced = true; break;
                case 11://DRAFT vyúčtování
                    AQ("a.p91ID IS NOT NULL AND p91x.p91IsDraft=1", null, null); break;
                case 12://vyúčtované jako fakturovat
                    AQ("a.p70ID=4 AND a.p91ID IS NOT NULL", null, null); break;
                case 13://vyúčtované jako paušál
                    AQ("a.p70ID=6 AND a.p91ID IS NOT NULL", null, null); break;
                case 14://vyúčtované jako odpis
                    AQ("a.p70ID IN (2,3) AND a.p91ID IS NOT NULL", null, null); break;
                case 15: //vyloučené z vyúčtování - náhrada za archiv
                    AQ("a.p31ExcludeBillingFlag=1", null, null); break;
                case 16://rozpracované Fa aktivita
                    
                    AQ("a.p71ID IS NULL AND a.p91ID IS NULL AND p32x.p32IsBillable=1 AND a.p31ExcludeBillingFlag IS NULL", null, null); break;
                case 17://rozpracované Fa aktivita
                    
                    AQ("a.p71ID IS NULL AND a.p91ID IS NULL AND p32x.p32IsBillable=0 AND a.p31ExcludeBillingFlag IS NULL", null, null); break;
                case 18://vyúčtované s nulovou cenou
                    AQ("a.p91ID IS NOT NULL AND a.p31Amount_WithoutVat_Invoiced=0", null, null); break;
                case 21:
                    AQ("a.p91ID IS NOT NULL AND p91x.p91Amount_Debt<1", null, null); break; //uhrazené vyúčtování
                case 19: //bez vyloučených z vyúčtování - náhrada za archiv
                    AQ("a.p31ExcludeBillingFlag IS NULL", null, null); break;
                case 20:
                    AQ("a.p91ID IS NULL AND a.p71ID IS NULL", null, null); break;   //využití pro přesunutí do vyloučené z vyúčtování
                    
            }
        }
    }
}
