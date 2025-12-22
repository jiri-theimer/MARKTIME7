

using System.Diagnostics;

namespace BO
{
    public class myQueryP49:baseQuery
    {
        public int p41id { get; set; }
        public int j02id { get; set; }
        public int strana { get; set; } //1:výdaj, 2: příjem
        public bool? realizovano { get; set; }
        public int p56id { get; set; }
        public int leindex { get; set; }   //nadřízená vertikální úrověň projektu #1 - #4
        public int lepid { get; set; }     //nadřízená vertikální úrověň projektu, hodnota p41id

        public DateTime? datwaiting { get; set; }

        public myQueryP49()
        {
            this.Prefix = "p49";
        }

        public override List<QRow> GetRows()
        {
            if (this.IsActivePeriodQuery())
            {
                AQ("a.p49Date BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
            }


            if (this.p41id > 0)
            {
                AQ("a.p41ID=@p41id", "p41id", this.p41id);
            }
            if (this.p56id > 0)
            {
                AQ("a.p56ID=@p56id", "p56id", this.p56id);
            }
            if (this.j02id > 0)
            {
                AQ("a.j02ID=@j02id", "j02id", this.j02id);
            }
            if (this.leindex > 0 && this.lepid > 0)
            {
                AQ($"p41x.p41ID_P07Level{this.leindex}=@lepid OR a.p41ID=@lepid", "lepid", this.lepid);
            }

            switch (this.strana)
            {
                case 1:
                    AQ("p34x.p34IncomeStatementFlag=1", null, null);
                    break;
                case 2:
                    AQ("p34x.p34IncomeStatementFlag=2", null, null);
                    break;
            }
            if (realizovano == true)
            {
                AQ("a.p49StatusFlag=1", null, null);
            }
            if (realizovano == false)
            {
                AQ("a.p49StatusFlag=0", null, null);
            }


            return this.InhaleRows();

        }
    }
}
