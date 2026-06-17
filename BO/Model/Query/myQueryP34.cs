

using System.ComponentModel;

namespace BO
{
    public class myQueryP34:baseQuery
    {
        public int p42id { get; set; }
        public int p41id { get; set; }
        public int p61id { get; set; }
        public int p36id { get; set; }  //sešity z uzamčeného období
        
        public bool? ismoneyinput { get; set; }
        public bool? isexpenseinput { get; set; }
        public bool? iskusovnik { get; set; }

        public myQueryP34()
        {
            this.Prefix = "p34";
            
        }

        public override List<QRow> GetRows()
        {
            
            if (this.p42id > 0)
            {
                AQ("a.p34ID IN (select p34ID FROM p43ProjectType_Workload WHERE p42ID=@p42id)", "p42id", this.p42id);
            }
            if (this.p36id > 0)
            {
                AQ("a.p34ID IN (select p34ID FROM p37LockPeriod_Sheet WHERE p36ID=@p36id)", "p36id", this.p36id);
            }
            if (this.ismoneyinput != null)
            {
                if (this.ismoneyinput == true)
                {
                    AQ("a.p33ID IN (2,5)", null, null);
                }
                else
                {
                    AQ("a.p33ID IN (1,3)", null, null);                    
                }
            }
            if (this.isexpenseinput == true)
            {
                AQ("a.p33ID IN (2,5) AND a.p34IncomeStatementFlag=1", null, null);
            }
            if (this.iskusovnik == true)
            {
                AQ("a.p33ID=3", null, null);
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
                if (!this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Hours) || !this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Expenses) || !this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Fees))
                {
                    //uživatel nemá oprávnění vykazovat všechny sešity do všech projektů  
                    string ss = null;                   
                    if (this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Hours))
                    {
                        //právo vykazovat hodiny a kusovník do všech projektů    
                        ss += " OR a.p33ID IN (1,3)";
                    }
                    if (this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Expenses))
                    {
                        //právo vykazovat výdaje do všech projektů    
                        ss += " OR (a.p33ID IN (2,5) AND a.p34IncomeStatementFlag=1)";
                    }
                    if (this.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Fees))
                    {
                        //právo vykazovat odměny do všech projektů    
                        ss += " OR (a.p33ID IN (2,5) AND a.p34IncomeStatementFlag=2)";
                    }
                    if (ss != null)
                    {
                        s += " AND (1=1 " + ss + ")";
                    }
                    

                }

                AQ($"a.p34ID IN ({s})", "p41id", this.p41id);

               

            }

            
            return this.InhaleRows();

        }
    }
}
