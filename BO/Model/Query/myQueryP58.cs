

namespace BO
{
    public class myQueryP58: baseQuery
    {
        public int p41id { get; set; }
        
        public myQueryP58()
        {
            this.Prefix = "p58";
        }

        public override List<QRow> GetRows()
        {

                       
            if (this.p41id > 0)
            {
                AQ("(a.p41ID=@p41id)", "p41id", this.p41id);
            }

            if (!this.CurrentUser.TestPermission(PermValEnum.GR_p56_Owner) && !this.CurrentUser.TestPermission(PermValEnum.GR_p56_Reader))
            {
                AQ("a.j02ID_Owner=@j02idme", "j02idme", this.CurrentUser.pid);
            }
            
            return this.InhaleRows();

        }
    }
}
