

namespace UI.Models
{
    public class BaseViewModel
    {
        public bool IsPostback { get; set; }
        public string PostbackOper { get; set; }

        private string _pagetitle;
        
        public string PageTitle
        {
            get
            {
                return _pagetitle;
            }
            set
            {
                _pagetitle = value;
            }
        }
        public string PageTitleAfter { get; set; }
       
        public string Element2Focus { get; set; }   //id elementu, na který se má automaticky udělit focus
        public string PageSymbol { get; set; }
        
       
        public string Javascript_CallOnLoad { get; set; }
        
       
        public void SetJavascript_CallOnLoad(int intPID, string strFlag = null,string jsfunction= "_reload_layout_and_close")
        {
            this.Javascript_CallOnLoad = string.Format(jsfunction+"({0},'{1}');", intPID,strFlag);
        }

        public void SetJavascript_CallOnLoad(string reloadurl_parentsite)
        {
            this.Javascript_CallOnLoad = "_close_and_reload_parentsite('"+reloadurl_parentsite+"')";
        }

        
    }
}
