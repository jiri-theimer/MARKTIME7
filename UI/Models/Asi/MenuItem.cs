using System;
using System.Collections.Generic;
using System.Text;

namespace UI.Models
{
    public class MenuItem
    {
        public string Name { get; set; }        
        public bool IsDivider { get; set; }
        public bool IsHeader { get; set; }
        public bool IsActive { get; set; }
        public bool IsCanBeGrid11 { get; set; } //true: lze přepínat mezi FlatView a MasterView
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public string Target { get; set; }
        private string _Url;        
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                _Url = value;

                
            }
        }

        public string CssClass { get; set; }
        

    }
}
