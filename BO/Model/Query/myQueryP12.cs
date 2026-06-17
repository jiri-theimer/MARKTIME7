
namespace BO
{
    public class myQueryP12:baseQuery
    {
        public int j02id { get; set; }

        public bool? isstatus { get; set; }

        public myQueryP12()
        {
            this.Prefix = "p12";
            this.IsRecordValid = null;
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
                    AQ("a.p12Date BETWEEN @d1 AND @d2", "d1", this.global_d1, "AND", null, null, "d2", this.global_d2);

                }
            }

            if (this.isstatus == true)
            {
                AQ("a.p12StatusFlag>0",null,null);
            }

            return this.InhaleRows();

        }
    }
}
