

namespace UI.Models
{
    public class kendoTreeItem
    {
        private string _recordid { get; set; }
        public string id { get; set; }
        public string prefix { get; set; }  //prefix není povinný                  
        public string parentid { get; set; }
        public string recordid
        {
            get
            {
                if (_recordid == null)
                {
                    return this.id;
                }
                else
                {
                    return _recordid;
                }
            }
            set
            {
                _recordid = value;
            }
        }
        public string text { get; set; }
        public bool? expanded { get; set; }
        public string imageUrl { get; set; }

        public bool? @checked { get; set; }
        
        public string cssclass { get; set; }
        public string textocas { get; set; }

        public string customvalue1 { get; set; }
        public string customvalue2 { get; set; }
        public string customvalue3 { get; set; }
        
        public string tooltip { get; set; }
        public string text_plus_group { get; set; }

        public List<kendoTreeItem> items { get; set; }

        public string active { get; set; }
        public bool isdivider { get; set; }
    }
}
