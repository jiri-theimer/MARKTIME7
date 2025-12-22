using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models
{
    public class BaseRecordViewModel
    {
        
        public int rec_pid { get; set; }        
        public string rec_entity { get; set; }
        public string form_action { get; set; } = "Record";
        
        public string PageTitle { get; set; }
        public string PageSymbol { get; set; }
        public int ActiveTabIndex { get; set; } = 1;

        public string TagPids { get; set; }
        public string TagNames { get; set; }
        public string TagHtml { get; set; }
        public string TagEntity { get; set; } = "p41Project";

        public bool IsPostback { get; set; }
        public string PostbackOper { get; set; }
        public int PostbackBylUzKolikrat { get; set; }
        public string Element2Focus { get; set; }   //id elementu, na který se má automaticky udělit focus

        public void SetTagging(BO.TaggingHelper tg)
        {            
            this.TagPids = tg.TagPids;
            this.TagNames = tg.TagNames;
            this.TagHtml = tg.TagHtml;
        }

        public string Javascript_CallOnLoad { get; set; }

        public void SetJavascript_CallOnLoad(int intPID, string strFlag = null, string jsfunction = "_reload_layout_and_close")
        {
            this.Javascript_CallOnLoad = string.Format(jsfunction + "({0},'{1}');", intPID, strFlag);
        }

        private MyToolbarViewModel _toolbar;


        public MyToolbarViewModel Toolbar
        {
            get
            {
                return _toolbar;
            }
            set
            {
                _toolbar = value;                
                _toolbar.RecordEntity = this.rec_entity;                
            }
        }

        public void MakeClone()
        {
            this.rec_pid = 0;
            
            _toolbar.MakeClone();

        }
    }
}
