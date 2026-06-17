using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UI.Models
{
    public class DropzoneViewModel
    {
        public string TempGuid { get; set; }
        public string RecPrefix{ get; set; }
        public int RecPid { get; set; }
        public bool IsAutoSave { get; set; }

        public bool IsInIframe { get; set; }
    }
}
