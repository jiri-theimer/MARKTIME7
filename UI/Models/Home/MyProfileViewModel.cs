using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UAParser;


namespace UI.Models.Home
{
    public class MyProfileViewModel:BaseViewModel
    {
        
        public BO.j02User RecJ02 { get; set; }
        
        
        public string userAgent { get; set; }
        public ClientInfo client_info { get; set; }

        public string Teams { get; set; }
        

        public IEnumerable<BO.j40MailAccount> lisJ40 { get; set; }

        public int SearchboxTopRecs { get; set; }
        public string SearchboxJ02 { get; set; }
        public string SearchboxP41 { get; set; }
        public string SearchboxP28 { get; set; }
        public string SearchboxP56 { get; set; }
        public string SearchboxP32 { get; set; }
        public string SearchboxP91 { get; set; }
        public List<BO.TheGridColumn> lisSearchboxJ02 { get; set; }
        public List<BO.TheGridColumn> lisSearchboxP41 { get; set; }
        public List<BO.TheGridColumn> lisSearchboxP28 { get; set; }
        public List<BO.TheGridColumn> lisSearchboxP56 { get; set; }
        public List<BO.TheGridColumn> lisSearchboxP32 { get; set; }
        public List<BO.TheGridColumn> lisSearchboxP91 { get; set; }
    }
}
