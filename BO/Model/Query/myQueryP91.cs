

namespace BO
{
    public class myQueryP91:baseQuery
    {
        public int p28id { get; set; }
        public int p41id { get; set; }
        public int o23id { get; set; }
        public int p56id { get; set; }
        public int j02id { get; set; }
        public int p92id { get; set; }
        public int p93id { get; set; }
        public int b02id { get; set; }
        public int j27id { get; set; }
       public int o51id { get; set; }
        public int p90id { get; set; }
        public int p75id { get; set; }
        
        public int leindex { get; set; }   //nadřízená vertikální úrověň #1 - #4
        public int lepid { get; set; }     //nadřízená vertikální úrověň, hodnota p41id

        //public int uhrazene { get; set; }   ////0: bez ohledu na úhradu, 1: neuhrazene po splatnosti,2: neuhrazené, 3: ve splatnosti, 4: částečně uhrazené

        public myQueryP91()
        {
            this.Prefix = "p91";
        }

        public override List<QRow> GetRows()
        {
            if (!string.IsNullOrEmpty(this.p31tabquery)) this.Handle_p31TabQuery();

            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "p91DateInsert":
                        AQ("a.p91DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p91DateMaturity":
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("a.p91DateMaturity BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("a.p91DateMaturity = @d1", "d1", this.global_d1_query);
                        }
                        break;
                    case "p91DateSupply":
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("a.p91DateSupply BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("a.p91DateSupply = @d1", "d1", this.global_d1_query);
                        }
                        break;
                    case "p91DateBilled":
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("a.p91DateBilled BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("a.p91DateBilled = @d1", "d1", this.global_d1_query);
                        }
                        break;
                    case "p91Date":
                    default:
                        if (this.global_d2_query > this.global_d1_query)
                        {
                            AQ("a.p91Date BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        }
                        else
                        {
                            AQ("a.p91Date = @d1", "d1", this.global_d1_query);
                        }
                        break;
                }
            }
        
            if (this.b02id > 0)
            {
                AQ("a.b02ID=@b02id", "b02id", this.b02id);
            }
            if (this.p28id > 0)
            {
                //AQ("a.p28ID=@p28id", "p28id", this.p28id);
                AQ("a.p28ID=@p28id OR EXISTS (select xa.p91ID FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xa.p91ID=a.p91ID AND xb.p28ID_Client=@p28id)", "p28id", this.p28id);
                
            }
            if (this.p41id > 0)
            {
                AQ("a.p91ID IN (SELECT p91ID FROM p31Worksheet WHERE p91ID IS NOT NULL AND p41ID=@p41id)", "p41id", this.p41id);
            }
            if (this.o23id > 0)
            {
                AQ("a.p91ID IN (SELECT p91ID FROM p31Worksheet WHERE p91ID IS NOT NULL AND o23ID=@o23id)", "o23id", this.o23id);
            }
            if (this.p56id > 0)
            {
                AQ("a.p91ID IN (SELECT p91ID FROM p31Worksheet WHERE p91ID IS NOT NULL AND p56ID=@p56id)", "p56id", this.p56id);
            }
            if (this.j27id > 0)
            {
                AQ("a.j27ID=@j27id)", "j27id", this.j27id);
            }
            if (this.p93id > 0)
            {
                AQ("a.p92ID IN (SELECT p92ID FROM p92InvoiceType WHERE p93ID=@p93id)", "p93id", this.p93id);
            }
            if (this.j02id > 0)
            {
                AQ("a.p91ID IN (SELECT p91ID FROM p31Worksheet WHERE p91ID IS NOT NULL AND j02ID=@j02id)", "j02id", this.j02id);
            }
            if (this.p75id > 0)
            {
                AQ("a.p91ID IN (select p91ID_NewInstance FROM p76InvoiceRecurrence_Plan xa INNER JOIN p75InvoiceRecurrence xb ON xa.p75ID=xb.p75ID WHERE xb.p75ID=@p75id)", "p75id", this.p75id);
            }

            if (this.p90id > 0)
            {
                AQ("a.p91ID IN (SELECT za.p91ID FROM p99Invoice_Proforma za INNER JOIN p82Proforma_Payment zb ON za.p82ID=zb.p82ID WHERE zb.p90ID=@p90id)", "p90id", this.p90id);
            }
            if (this.o51id > 0)
            {
                AQ("a.p91ID IN (select o52RecordPid FROM o52TagBinding where o52RecordEntity='p91' AND o51ID=@o51id)", "o51id", this.o51id);
            }

            if (_searchstring != null && _searchstring.Length > 2)
            {
                AQ("(a.p91Code Like '%'+@expr+'%' OR a.p91Text1 LIKE '%'+@expr+'%' COLLATE Latin1_General_CI_AI OR a.p91Client_RegID LIKE '%'+@expr+'%' COLLATE Latin1_General_CI_AI OR a.p91Client_VatID LIKE @expr+'%' OR a.p41ID_First IN (select xa.p41ID FROM p41Project xa LEFT OUTER JOIN p28Contact xb ON xa.p28ID_Client=xb.p28ID WHERE xa.p41Name LIKE '%'+@expr+'%' COLLATE Latin1_General_CI_AI OR xa.p41Code LIKE '%'+@expr+'%' OR xb.p28Name LIKE '%'+@expr+'%') OR a.p91Client LIKE '%'+@expr+'%' COLLATE Latin1_General_CI_AI OR a.p28ID IN (select p28ID FROM p28Contact WHERE p28ShortName LIKE '%'+@expr+'%' OR p28Name LIKE '%'+@expr+'%'))", "expr", _searchstring);

            }

            if (this.leindex > 0 && this.lepid>0)
            {
                AQ($"a.p91ID IN (SELECT xa.p91ID FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xa.p91ID IS NOT NULL AND (xb.p41ID=@lepid OR xb.p41ID_P07Level{this.leindex}=@lepid))", "lepid", this.lepid);
            }

            //switch (this.uhrazene)
            //{
            //    case 1:
            //        AQ("a.p91IsDraft=0 AND a.p91DateMaturity<GETDATE() AND a.p91Amount_Debt>0.1", null,null);
            //        break;
            //    case 2:
            //        AQ("a.p91IsDraft=0 AND a.p91Amount_Debt>0.1", null, null);
            //        break;
            //    case 3:
            //        AQ("a.p91IsDraft=0 AND a.p91DateMaturity>GETDATE()", null, null);
            //        break;
            //    case 4:
            //        AQ("a.p91IsDraft=0 AND a.p91Amount_Debt>0.1 AND a.p91Amount_Debt+1<p91Amount_TotalDue", null, null);
            //        break;
            //    case 5:
            //        AQ("a.p91IsDraft=1", null, null);
            //        break;
            //    case 10:
            //        AQ("a.p91LockFlag=0", null, null);break;
            //    case 11:
            //        AQ("a.p91LockFlag & 2 = 2", null, null);break;
            //    case 12:
            //        AQ("a.p91LockFlag & 4 = 4", null, null);break;
            //    case 13:
            //        AQ("a.p91LockFlag & 8 = 8", null, null);break;
            //    case 14:
            //        AQ("a.p91LockFlag & 2 = 2 AND a.p91LockFlag & 4 = 4 AND a.p91LockFlag & 8 = 8", null, null); break;
            //    case 15:
            //        AQ(" EXISTS (select 1 FROM p31Worksheet_Del WHERE p91ID=a.p91ID)", null, null);
            //        break;
            //    case 16:
            //        AQ("a.p91IsDraft=0 AND a.p91Amount_Debt<0.1", null, null);
            //        break;

            //}

            if (this.MyRecordsDisponible)
            {
                Handle_MyDisponible();
            }
            

            return this.InhaleRows();

        }

        private void Handle_MyDisponible()
        {            
            if (this.CurrentUser.TestPermission(PermValEnum.GR_P91_Reader) || this.CurrentUser.TestPermission(PermValEnum.GR_P91_Owner))
            {
                return; //přístup ke všem vyúčtováním
            }
            
            
            var sb = new System.Text.StringBuilder();
            sb.Append("a.j02ID_Owner=@j02id_query");
            sb.Append(" OR EXISTS (SELECT 1 FROM x73InvoiceRole_Permission");
            sb.Append(" WHERE (j02ID=@j02id_query OR x73IsAllUsers=1");
           
            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                sb.Append($" OR j11ID IN ({this.CurrentUser.j11IDs})"); 
            }
            sb.Append(")");
            sb.Append(" AND (p41ID=a.p41ID_First OR p91ID=a.p91ID)");
            sb.Append(")");


            




            AQ(sb.ToString(), "j02id_query", get_real_j02id_query());
        }

        
    }
}
