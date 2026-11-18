namespace UI.Code.Integrace
{
    //výsledek odeslání jednoho záznamu do cizího systému
    public class IntegraceRow
    {
        public int pid { get; set; }
        public string Name { get; set; }
        public bool IsOk { get; set; }
        public bool IsSkipped { get; set; }     //záznam se neposílal (např. typ dokumentu není namapovaný)
        public string Message { get; set; }
        public string ExternalPid { get; set; }
    }

    public class IntegraceResult
    {
        public List<IntegraceRow> Rows { get; set; } = new List<IntegraceRow>();
        public string Error { get; set; }       //chyba celého běhu (spojení, přihlášení)
        public string Summary { get; set; }
        public int CountOk { get { return Rows.Count(p => p.IsOk); } }
        public int CountErr { get { return Rows.Count(p => !p.IsOk && !p.IsSkipped); } }
        public int CountSkipped { get { return Rows.Count(p => p.IsSkipped); } }
    }
}
