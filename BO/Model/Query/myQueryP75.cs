

namespace BO
{
    public class myQueryP75:baseQuery
    {
        public int p41id { get; set; }
        public int p28id { get; set; }

        public myQueryP75()
        {
            this.Prefix = "p75";
        }

        public override List<QRow> GetRows()
        {


            if (this.p41id > 0)
            {
                AQ("(a.p41ID=@p41id)", "p41id", this.p41id);
            }
            if (this.p28id > 0)
            {
                AQ("(a.p28ID=@p28id)", "p28id", this.p28id);
            }

            if (!this.CurrentUser.TestPermission(PermValEnum.GR_P91_Owner) && !this.CurrentUser.TestPermission(PermValEnum.GR_P91_Reader))
            {
                AQ("a.j02ID_Owner=@j02idme", "j02idme", this.CurrentUser.pid);
            }

            return this.InhaleRows();

        }
    }
}
