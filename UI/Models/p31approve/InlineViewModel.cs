namespace UI.Models.p31approve
{
    public class InlineViewModel:p31approveMother
    {
        public string OrderBy1 { get; set; }
        public string p72Query { get; set; }
        public string pidsinline { get; set; }
        public BO.p72IdENUM p72id { get; set; }

        public int approvinglevel { get; set; }

        public List<GridRecord> lisRecs { get; set; }

        public string CheckedPids { get; set; }
        public string batchpids { get; set; }

        public double BatchInvoiceRate { get; set; }
        public int BatchApproveLevel { get; set; }
        public int j72id_report { get; set; }

        public bool IsRenderp28Name { get; set; }
        public bool IsRenderp41Name { get; set; }
        public bool IsRenderPerson { get; set; }
        public bool IsRenderp34Name { get; set; }
        public bool IsRenderp32Name { get; set; }

        public ApproveStat InitStatistic { get; set; }
    }
}
