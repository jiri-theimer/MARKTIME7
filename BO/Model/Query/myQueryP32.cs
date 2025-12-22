
namespace BO
{
    public class myQueryP32 : baseQuery
    {
        public int p34id { get; set; }
        public int p38id { get; set; }
        public int p95id { get; set; }
        public int p33id {get;set;}
        public int p61id { get; set; }
        public int p41id { get; set; }
        public bool? isabsence { get; set; }
        public bool? ismoneyinput { get; set; }
        
        public bool? isbillable { get; set; }
        public myQueryP32()
        {
            this.Prefix = "p32";
        }

        public override List<QRow> GetRows()
        {
            if (this.p34id > 0)
            {
                AQ("a.p34ID=@p34id", "p34id", this.p34id);
            }
            if (this.p38id > 0)
            {
                AQ("a.p38ID=@p38id", "p38id", this.p38id);
            }
            if (this.p61id > 0)
            {
                AQ("a.p32ID IN (SELECT p32ID FROM p62ActivityCluster_Item WHERE p61ID=@p61id)", "p61id", this.p61id);
            }
            if (this.p95id > 0)
            {
                AQ("a.p95ID=@p95id", "p95id", this.p95id);
            }
            if (this.p33id > 0)
            {
                AQ("a.p34ID IN (select p34ID FROM p34ActivityGroup WHERE p33ID=@p33id)", "p33id", this.p33id);
            }
            
          
            if (this.ismoneyinput != null)
            {
                if (this.ismoneyinput==true)
                {
                    AQ("a.p34ID IN (select p34ID FROM p34ActivityGroup WHERE p33ID IN (2,5))", null, null);
                }
                else
                {
                    AQ("a.p34ID IN (select p34ID FROM p34ActivityGroup WHERE p33ID IN (1,3))", null, null);
                }
            }
            if (this.isbillable != null)
            {
                AQ("a.p32IsBillable=@billable", "billable", this.isbillable);
            }
            if (this.isabsence == true)
            {
                AQ("isnull(a.p32AbsenceFlag,0)>0",null,null);
            }
            if (this.isabsence == false)
            {
                AQ("isnull(a.p32AbsenceFlag,0)=0", null, null);
            }


            if (this.p41id > 0)
            {
                //nabídka sešitů při vykazování v projektu p41id
                string s = "SELECT p43.p34ID FROM p43ProjectType_Workload p43 INNER JOIN p42ProjectType p42 ON p43.p42ID=p42.p42ID INNER JOIN p41Project p41 ON p42.p42ID=p41.p42ID";
                s += " WHERE p41.p41ID=@p41id";

                if (this.p61id > 0)
                {
                    //zúžení sešitů podle klastru aktivit v projektu
                    s += $" AND p43.p34ID IN (select xa.p34ID FROM p32Activity xa INNER JOIN p62ActivityCluster_Item xb ON xa.p32ID=xb.p32ID WHERE xb.p61ID={this.p61id})";
                }

                //zúžení sešitů podle projektové role uživatele v projektu p41id - je třeba dodělat!!!!
                //if (!this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Hours) || !this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Expenses) || !this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Fees))
                //{
                //    //uživatel nemá oprávnění vykazovat všechny sešity do všech projektů  
                //    string ss = null;
                //    if (this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Hours))
                //    {
                //        //právo vykazovat hodiny a kusovník do všech projektů    
                //        ss += " OR a.p33ID IN (1,3)";
                //    }
                //    if (this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Expenses))
                //    {
                //        //právo vykazovat výdaje do všech projektů    
                //        ss += " OR (a.p33ID IN (2,5) AND a.p34IncomeStatementFlag=1)";
                //    }
                //    if (this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Fees))
                //    {
                //        //právo vykazovat odměny do všech projektů    
                //        ss += " OR (a.p33ID IN (2,5) AND a.p34IncomeStatementFlag=2)";
                //    }
                //    if (ss != null)
                //    {
                //        s += " AND (1=1 " + ss + ")";
                //    }


                //}

                AQ($"a.p34ID IN ({s})", "p41id", this.p41id);



            }

            return this.InhaleRows();

        }
    }
}
