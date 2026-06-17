

namespace BO
{
    public class myQueryX31:baseQuery
    {
        public string entity { get; set; }
        public bool is4portal { get; set; }
        public bool ishidesubreports { get; set; }
        public myQueryX31()
        {
            this.Prefix = "x31";
        }

        public override List<QRow> GetRows()
        {
            if (this.entity !=null)
            {
                AQ("a.x31Entity=@entity", "entity", this.entity);
            }

            if (this.is4portal)
            {
                AQ("(a.x31Entity IN ('p91','p28') OR a.x31Entity IS NULL)",null,null);
            }
            if (this.ishidesubreports)
            {
                AQ("a.x31Code NOT LIKE '%_subreport%'", null, null);
            }

            if (this.MyRecordsDisponible)
            {
                
                if (!this.CurrentUser.IsAdmin)
                {
                    bool b = this.CurrentUser.TestPermission(PermValEnum.GR_x31_ReadAll);                   
                    if (!b)
                    {
                        string strW = "NOT EXISTS(select x69.x69RecordPid FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID AND x67.x67Entity='x31' AND x69.x69RecordPid=a.x31ID)";
                        strW = $"{strW} OR a.X31ID IN (SELECT x69.x69RecordPid FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID WHERE x67.x67Entity='x31' AND (x69.x69IsAllUsers=1 OR x69.j02ID=@j02id_query OR x69.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID=@j02id_query)))";
                        AQ(strW, "j02id_query", this.CurrentUser.pid);
                    }                    
                }
            }

            return this.InhaleRows();

        }
    }
}
