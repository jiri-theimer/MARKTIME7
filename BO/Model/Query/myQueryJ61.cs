

namespace BO
{
    public class myQueryJ61:baseQuery
    {
        public string entity { get; set; }
        public int b01id { get; set; }
        public myQueryJ61()
        {
            this.Prefix = "j61";
        }

        public override List<QRow> GetRows()
        {
            if (!string.IsNullOrEmpty(this.entity))
            {
                AQ("a.j61Entity=@entity", "entity", this.entity);
            }
            if (this.b01id > 0)
            {
                AQ("a.b01ID=@b01id","b01id", this.b01id);
            }

            if (this.MyRecordsDisponible)
            {
                AQ("(a.j02ID_Owner=@j02id_query OR a.j61IsPublic=1)", "j02id_query", this.CurrentUser.pid);
                //AQ("(a.j02ID_Owner=@j02id_query OR a.j61ID IN (SELECT x69.x69RecordPid FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID WHERE x67.x67Entity='j61' AND (x69.j02ID=@j02id_query OR x69.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID=@j02id_query))))", "j02id_query", this.CurrentUser.pid);
            }

            return this.InhaleRows();

        }
    }
}
