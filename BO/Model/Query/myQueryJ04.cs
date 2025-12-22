

namespace BO
{
    public class myQueryJ04:baseQuery
    {
        public int x31id { get; set; }
                
        public int x55id { get; set; }
        public int j72id { get; set; }
        public int j79id { get; set; }
      
        public myQueryJ04()
        {
            this.Prefix = "j04";
        }

        public override List<QRow> GetRows()
        {
            if (this.x31id > 0)
            {
                AQ("a.j04ID IN (select j04ID FROM x37ReportRestriction_UserRole WHERE x31ID=@x31id)", "x31id", this.x31id);
            }
           

           
            if (this.x55id > 0)
            {
                AQ("a.j04ID IN (select j04ID FROM x57WidgetRestriction WHERE x55ID=@x55id)", "x55id", this.x55id);
            }
            if (this.j72id > 0)
            {
                AQ("a.j04ID IN (select j04ID FROM j74TheGridReceiver WHERE j72ID=@j72id)", "j72id", this.j72id);
            }
            if (this.j79id > 0)
            {
                AQ("a.j04ID IN (select j04ID FROM j80TotalsReceiver WHERE j79ID=@j79id)", "j79id", this.j79id);
            }


            return this.InhaleRows();

        }
    }
}
