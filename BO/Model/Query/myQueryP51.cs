
namespace BO
{
    public class myQueryP51:baseQuery
    {
        public int typeflag { get; set; }
        public bool? iscustomtailor { get; set; }

        public myQueryP51()
        {
            this.Prefix = "p51";
        }

        public override List<QRow> GetRows()
        {

            if (this.typeflag>0)
            {

                AQ("a.p51TypeFlag = @flag", "flag", this.typeflag);
            }

            if (this.iscustomtailor != null)
            {
                if (this.iscustomtailor == true)
                {
                    AQ("a.p51IsCustomTailor=1", null, null);
                }
                else
                {
                    AQ("a.p51IsCustomTailor=0", null, null);
                }
            }

            return this.InhaleRows();

        }
    }
}
