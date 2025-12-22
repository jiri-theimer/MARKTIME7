

namespace BO
{
    public class myQueryJ11:baseQuery
    {
        public int j02id { get; set; }
       
       
        public myQueryJ11()
        {
            this.Prefix = "j11";
        }

        public override List<QRow> GetRows()
        {
            if (this.j02id > 0)
            {
                AQ("a.j11ID IN (select j11ID FROM j12Team_Person where j02ID=@j02id)", "j02id", this.j02id);
            }
           
            
            return this.InhaleRows();

        }
    }
}
