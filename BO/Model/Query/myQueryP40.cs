

namespace BO
{
    public  class myQueryP40: baseQuery
    {
        public int p41id { get; set; }
        public int p28id { get; set; }
        public int j02id { get; set; }
        
        public DateTime? datwaiting { get; set; }
        public int filtrovani { get; set; }  //1: skluz generování mimo dosah robota, 2: skluz generování, 3: expirované období, 4: projekt v archivu
        public myQueryP40()
        {
            this.Prefix = "p40";
        }

        public override List<QRow> GetRows()
        {
            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "p39DateCreate":
                        AQ("a.p40ID IN (select p40ID FROM p39WorkSheet_Recurrence_Plan WHERE p39DateCreate BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    default:
                        AQ("a.p40ID IN (select p40ID FROM p39WorkSheet_Recurrence_Plan WHERE p39Date BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                }
            }
            

            if (this.p41id > 0)
            {
                AQ("a.p41ID=@p41id", "p41id", this.p41id);
            }
            if (this.j02id > 0)
            {
                AQ("a.j02ID=@j02id", "j02id", this.j02id);
            }
            if (this.p28id > 0)
            {
                AQ("EXISTS(select 1 FROM p39WorkSheet_Recurrence_Plan xa INNER JOIN p40WorkSheet_Recurrence xb ON xa.p40ID=xb.p40ID INNER JOIN p41Project xc ON xb.p41ID=xc.p41ID WHERE xc.p28ID_Client=@p28id AND xa.p40ID=a.p40ID)", "p28id", this.p28id);
            }

            if (this.datwaiting != null)
            {
                string s = "a.p40ValidUntil>GETDATE() AND p41x.p41ValidUntil>GETDATE() AND a.p40ID IN (select p40ID FROM p39WorkSheet_Recurrence_Plan WHERE p31ID_NewInstance IS NULL AND p39DateCreate BETWEEN dateadd(day,-2,@dat) AND @dat)";
                AQ(s, "dat", this.datwaiting);
            }

            switch (this.filtrovani)
            {
                case 1:
                    AQ("p39miss.p39DateCreate_Min<DATEADD(DAY,-31,GETDATE())", null, null);
                    break;
                case 2:
                    AQ("p39miss.p39DateCreate_Min IS NOT NULL", null, null);
                    break;
                case 3:
                    AQ("a.p40LastSupplyDate<GETDATE()", null, null);
                    break;
                case 4:
                    AQ("p41x.p41ValidUntil<GETDATE()", null, null);
                    break;
               

            }

            return this.InhaleRows();

        }
    }
}
