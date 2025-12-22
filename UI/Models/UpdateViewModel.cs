namespace UI.Models
{
    public class UpdateViewModel:BaseViewModel
    {
        public string MyPassword { get; set; }
        public string DestDbName { get; set; }

        public IEnumerable<BO.Sys.sysobject> sourceTabs { get; set; }
        public IEnumerable<BO.Sys.syscolumn> sourceCols { get; set; }
        public IEnumerable<BO.Sys.sysindex> sourceInds { get; set; }
        public IEnumerable<BO.Sys.sysdefval> sourceDefVals { get; set; }
        public IEnumerable<BO.Sys.sysconstraint> sourceConstraints { get; set; }

        public IEnumerable<BO.Sys.sysobject> destTabs { get; set; }
        public IEnumerable<BO.Sys.syscolumn> destCols { get; set; }
        public IEnumerable<BO.Sys.sysindex> destInds { get; set; }
        public IEnumerable<BO.Sys.sysconstraint> destConstraints { get; set; }

        public IEnumerable<BO.Sys.sysdefval> destDefVals { get; set; }

        public string RunResult { get; set; }
        public string ResultFlag { get; set; }

        public IEnumerable<BO.x01License> lisX01_CloudHeader { get; set; }
    }
}
