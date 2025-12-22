namespace UI.Models.Admin
{
    public class SpaceUsedViewModel:BaseViewModel
    {
        public IEnumerable<BO.Sys.sp_spaceused_table> lisTabsUsedSpace { get; set; }
        public BO.Sys.sp_spaceused_db DbUsedSpace { get; set; }

        public int size_j92 { get; set; }
        public int rows_j92 { get; set; }
        
        

        public int size_p31 { get; set; }
        public int size_p31log { get; set; }
        public int size_p31del { get; set; }
        public int rows_p31 { get; set; }
        public int rows_p31log { get; set; }
        public int rows_p31del { get; set; }

        
        
        public int size_outbox { get; set; }        
        public int rows_outbox { get; set; }
        
        public int size_inbox { get; set; }
        public int rows_inbox { get; set; }

        public int size_temp { get; set; }
        public int rows_temp { get; set; }

        public int rows_updatelog { get; set; }
        public int size_updatelog { get; set; }

        public bool IsTruncateJ92 { get; set; }
        
        public int MonthBeforeJ92 { get; set; }
        public bool IsTruncateTemps { get; set; }
        public bool IsTruncateEmls { get; set; }
        public bool IsTruncateP96 { get; set; }
        public bool IsTruncateUpdateLogs { get; set; }
        public int MonthBeforeUpdateLogs { get; set; }

        public bool IsTruncateP31log { get; set; }
        public int MonthBeforeP31Log { get; set; }

        public bool IsTruncateOutbox { get; set; }
        public int MonthBeforeOutbox { get; set; }
    }
}
