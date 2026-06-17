

namespace UI.Models
{
    public class FileUploadSingleViewModel:BaseViewModel
    {
        public string Guid { get; set; }
        public string JavascriptParentSite { get; set; }

        public List<BO.o27Attachment> lisTempFiles { get; set; }


    }
}
