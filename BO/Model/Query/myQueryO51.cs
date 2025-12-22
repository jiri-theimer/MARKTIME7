

namespace BO
{
    public class myQueryO51:baseQuery
    {
        public int o53id { get; set; }
        

        public myQueryO51()
        {
            this.Prefix = "o51";
        }

        public override List<QRow> GetRows()
        {

            if (this.o53id > 0)
            {
                AQ("a.o53ID = @o53id", "o53id", this.o53id);
            }
           


            return this.InhaleRows();

        }
    }
}
