using System;
using System.ComponentModel.DataAnnotations;

namespace BO
{
    public class j92PingLog
    {
        [Key]
        public int j92ID;
        public int j02ID { get; set; }
        public DateTime j92Date { get; set; }
        public string j92BrowserUserAgent { get; set; }
        public string j92BrowserFamily { get; set; }
        public string j92BrowserOS { get; set; }
        public string j92BrowserDeviceType { get; set; }
        public string j92BrowserDeviceFamily { get; set; }
        public int j92BrowserAvailWidth { get; set; }
        public int j92BrowserAvailHeight { get; set; }
        public int j92BrowserInnerWidth { get; set; }
        public int j92BrowserInnerHeight { get; set; }
        public string j92RequestURL { get; set; }


    }
}
