

namespace BO
{
    public class myQueryP42: baseQuery
    {
        
        public myQueryP42()
        {
            this.Prefix = "p42";

        }


        public override List<QRow> GetRows()
        {

            

           
            return this.InhaleRows();

        }

    }
}
