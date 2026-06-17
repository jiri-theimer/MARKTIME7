using System;
using System.ComponentModel.DataAnnotations;

namespace BO
{
    public class j90LoginAccessLog
    {
        [Key]
        public int j90ID;

        public DateTime j90Date { get; set; }
        public int j02ID { get; set; }
        public bool j90IsDomainTrusted { get; set; }
        public string j90ClientBrowser { get; set; }
        public string j90BrowserFamily { get; set; }    //nové pole verze 7
        public string j90BrowserDeviceType { get; set; }    //nové pole verze 7
        public string j90BrowserDeviceFamily { get; set; }    //nové pole verze 7
        public int j90BrowserInnerWidth { get; set; }    //nové pole verze 7
        public int j90BrowserInnerHeight { get; set; }    //nové pole verze 7
        public string j90LoginMessage { get; set; } //nové pole verze 7
        public string j90LoginName { get; set; }    //nové pole verze 7
        public int j90CookieExpiresInHours { get; set; }    //nové pole verze 7
        public string j90Platform { get; set; }
        public string j90UserHostAddress { get; set; }
        public string j90UserHostName { get; set; }
        
        public string j90AppClient { get; set; }
        public bool j90IsMobileDevice { get; set; }
        public string j90MobileDevice { get; set; }
        public int j90ScreenPixelsHeight { get; set; }
        public int j90ScreenPixelsWidth { get; set; }
        public string j90DomainUserName { get; set; }
        public string j90RequestURL { get; set; }
        public int x01ID { get; set; }
        public string ScreenResolution
        {
            get
            {
                if (j90ScreenPixelsHeight > 0)
                    return j90ScreenPixelsWidth.ToString() + " x " + j90ScreenPixelsHeight.ToString();
                else
                    return "";
            }
        }
        public string BrowserInfo
        {
            get
            {
                return "..." + BO.Code.Bas.RightString(this.j90ClientBrowser, 50);
            }
        }
        public string PageUrl
        {
            get
            {
                if (this.j90RequestURL.IndexOf("/") == -1)
                    return this.j90RequestURL;

                return BO.Code.Bas.RightString(this.j90RequestURL, this.j90RequestURL.Length - this.j90RequestURL.LastIndexOf("/"));
            }
        }

    }
}
