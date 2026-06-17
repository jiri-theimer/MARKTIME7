

namespace BO
{
    public class myQueryP24:baseQuery
    {
        public int p28id { get; set; }


        public myQueryP24()
        {
            this.Prefix = "p24";
        }

        public override List<QRow> GetRows()
        {
            if (this.p28id > 0)
            {
                AQ("a.p24ID IN (select p24ID FROM p25ContactGroupBinding where p28ID=@p28id)", "p28id", this.p28id);
            }


            return this.InhaleRows();

        }
    }
}
