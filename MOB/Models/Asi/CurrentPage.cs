namespace MOB.Models
{
    public class CurrentPage
    {
        public string j02textsize { get; set; }
        public string j02theme { get; set; }
        public string pagetitle { get; set; }
        
        public bool ispostback{get;set;}
        public string postbackoper { get; set; }
        public string element2focus { get; set; }
        public string pagesymbol { get; set; }
        public bool islivechaton { get; set; }
        public bool isneedpingupdate { get; set; }
        public List<BO.StringPair> messages4notify { get; set; }


    }
}
