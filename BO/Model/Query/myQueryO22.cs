
namespace BO
{
    public class myQueryO22 : baseQuery
    {
        public int typeflag { get; set; }
        public int o21id { get; set; }
        public int p41id { get; set; }

        public int j02id { get; set; }
        public List<int> j02ids { get; set; }
       
        public int j02id_owner { get; set; }
        public int x67id { get; set; }
        public int leindex { get; set; }   //nadřízená vertikální úrověň projektu #1 - #4
        public int lepid { get; set; }     //nadřízená vertikální úrověň projektu, hodnota p41id
        public bool isportal { get; set; }
        public myQueryO22()
        {
            this.Prefix = "o22";
        }

        public override List<QRow> GetRows()
        {

            if (this.typeflag > 0)
            {

                AQ("o21x.o21TypeFlag = @typeflag", "typeflag", this.typeflag);
            }

            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "o22DateInsert":
                        AQ("a.o22DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;

                    case "o22PlanFrom":
                        AQ("a.o22PlanFrom BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "o22PlanUntil":
                        AQ("a.o22PlanUntil BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;

                }
            }


            if (this.o21id > 0)
            {
                AQ("a.o21ID=@o21id)", "o21id", this.o21id);
            }
            if (this.p41id > 0)
            {
                AQ("(a.p41ID=@p41id)", "p41id", this.p41id);
            }

            if (this.leindex > 0 && this.lepid > 0)
            {
                AQ($"p41x.p41ID_P07Level{this.leindex}=@lepid OR a.p41ID=@lepid", "lepid", this.lepid);
            }

            if (this.j02id_owner > 0)
            {
                AQ("a.j02ID_Owner=@ownerid)", "ownerid", this.j02id_owner);
            }


            if (this.j02id > 0)
            {
                if (this.x67id > 0)
                {
                    //obdržel roli
                    AQ($"a.o22ID IN (SELECT x69.x69RecordPID FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID WHERE x67.x67Entity='o22' AND x69.x67ID={this.x67id} AND (x69.j02ID=@j02id OR x69.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID = @j02id)))", "j02id", this.j02id);
                }
                else
                {
                    //obdržel jakoukoliv roli v úkolu
                    AQ("a.o22ID IN (SELECT x69.x69RecordPID FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID WHERE x67.x67Entity='o22' AND (x69.j02ID=@j02id OR x69.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID = @j02id)))", "j02id", this.j02id);
                }                
            }

            if (this.j02ids !=null && this.j02ids.Count() > 0)
            {
                if (this.x67id > 0)
                {
                    //obdržel roli
                    AQ($"a.o22ID IN (SELECT x69.x69RecordPID FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID WHERE x67.x67Entity='o22' AND x69.x67ID={this.x67id} AND (x69.j02ID IN ({string.Join(", ", this.j02ids)}) OR x69.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID IN ({string.Join(", ", this.j02ids)}))))", "j02id", this.j02id);
                }
                else
                {
                    //obdržel jakoukoliv roli v úkolu
                    AQ($"a.o22ID IN (SELECT x69.x69RecordPID FROM x69EntityRole_Assign x69 INNER JOIN x67EntityRole x67 ON x69.x67ID=x67.x67ID WHERE x67.x67Entity='o22' AND (x69.j02ID IN ({string.Join(", ", this.j02ids)}) OR x69.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID IN ({string.Join(", ", this.j02ids)}))))", "j02id", this.j02id);
                }                
            }
            if (this.x67id > 0 && this.j02id == 0 && this.j02ids == null)
            {
                AQ("a.o22ID IN (SELECT x69RecordPid FROM x69EntityRole_Assign WHERE x67ID=@x67id)", "x67id", this.x67id);
            }

            if (this.MyRecordsDisponible)
            {
                Handle_MyDisponible();
            }

            return this.InhaleRows();

        }

        private void Handle_MyDisponible()
        {
            if (this.CurrentUser.IsAdmin || this.CurrentUser.TestPermission(PermValEnum.GR_o22_Reader) || this.CurrentUser.TestPermission(PermValEnum.o22_Reader))
            {
                return; //přístup ke všem termínům
            }
            string s = "a.j02ID_Owner=@j02id_query";
            s += " OR EXISTS (SELECT 1 FROM x69EntityRole_Assign xa inner join x67EntityRole xb ON xa.x67ID=xb.x67ID";
            s += " WHERE xb.x67Entity='o22' and (xa.j02ID=@j02id_query";

            if (!this.isportal)
            {
                s += " OR xa.x69IsAllUsers=1";  //v portálu se nehraje na oprávnění pro všechny uživatele
            }

            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                s += " OR xa.j11ID IN (" + this.CurrentUser.j11IDs + ")";
            }
            s += ")";
            s += " AND xa.x69RecordEntity='o22' AND xa.x69RecordPid=a.o22ID";
            s += ")";

            AQ(s, "j02id_query", get_real_j02id_query());
        }
    }
}
