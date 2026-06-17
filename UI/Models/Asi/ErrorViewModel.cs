using System;

namespace UI.Models
{
    public class ErrorViewModel: BaseViewModel
    {
        public int? StatusCode { get; set; }
        public string RequestId { get; set; }
        public Exception Error { get; set; }
        public string OrigFullPath { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
