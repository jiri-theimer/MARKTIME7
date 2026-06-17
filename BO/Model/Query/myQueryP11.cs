

namespace BO
{
    public class myQueryP11:baseQuery
    {
        public int j02id { get; set; }

        public myQueryP11()
        {
            this.Prefix = "p11";
        }
        public override List<QRow> GetRows()
        {
            if (this.j02id > 0)
            {
                AQ("a.j02ID=@j02id", "j02id", this.j02id);
            }

            if (this.IsActivePeriodQuery())
            {
                if (this.global_d1 != null && this.global_d2 != null)
                {
                    AQ("a.p11Date BETWEEN @d1 AND @d2", "d1", this.global_d1, "AND", null, null, "d2", this.global_d2);

                }
            }
                

            return this.InhaleRows();

        }
    }
}
