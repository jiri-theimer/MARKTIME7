
namespace BO
{
    public class myQueryO18:baseQuery
    {
        public string entity { get; set; }
        public int o17id { get; set; }
        public myQueryO18()
        {
            this.Prefix = "o18";
        }

        public override List<QRow> GetRows()
        {
            if (this.entity !=null)
            {
                AQ("a.o18ID IN (select o18ID FROM o20DocTypeEntity WHERE o20Entity=@entity)", "entity", this.entity);
            }

            if (this.o17id > 0)
            {
                AQ("a.o17ID=@o17id", "o17id", this.o17id);
            }

            return this.InhaleRows();

        }
    }
}
