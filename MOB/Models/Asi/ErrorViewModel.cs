

namespace MOB.Models
{
    public class ErrorViewModel: MOB.Models.BaseViewModel
    {
        public int? StatusCode { get; set; }
        public string RequestId { get; set; }
        public Exception Error { get; set; }
        public string OrigFullPath { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
