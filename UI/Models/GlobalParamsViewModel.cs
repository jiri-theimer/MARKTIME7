namespace UI.Models
{
    public class GlobalParamsViewModel:BaseViewModel
    {
     
        public List<o59Repeater> lisO59 { get; set; }
        public IEnumerable<BO.o58GlobalParam> lisO58 { get; set; }
        public string prefix { get; set; }
        public int pid { get; set; }
    }

    public class o59Repeater : BO.o59GlobalParamBinding
    {
        public string ComboPerson { get; set; }
        public BO.x24IdENUM x24ID { get; set; }
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted == true)
                {
                    return "display:none;";
                }
                else
                {
                    return "display:table-row;";
                }
            }
        }

    }
}
