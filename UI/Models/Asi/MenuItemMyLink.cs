namespace UI.Models
{
    public class MenuItemMyLink:MenuItem
    {
        public string TempGuid { get; set; }
        public bool IsTempDeleted { get; set; }
        public int Ordinary { get; set; }
        public bool IsJustNew { get; set; }
        public string CssTempDisplay { get
            {
                if (this.IsTempDeleted)
                {
                    return "display:none;";
                }
                else
                {
                    if (this.IsJustNew)
                    {
                        return "display:block;background-color:orange;";
                    }
                    else
                    {
                        return "display:block;";
                    }
                    
                }
                
            }
        }
    }
}
