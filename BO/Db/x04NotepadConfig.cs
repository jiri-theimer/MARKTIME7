using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class x04NotepadConfig:BaseBO
    {
        public int x01ID { get; set; }
        public string x04Name { get; set; }
        public int x04Ordinary { get; set; }
        public string x04ToolbarButtons { get; set; }
        public string x04ToolbarButtonsXS { get; set; }
        public bool x04IsToolbarInline { get; set; }
        public bool x04IsToolbarSticky { get; set; }
        public string x04PlaceHolder { get; set; }
        public int x04ImageMaxSize { get; set; }
        public int x04FileMaxSize { get; set; }
        public string x04FileAllowedTypes { get; set; }
        public string x04ImageAllowedTypes { get; set; }
        public bool x04IsTrackChanges { get; set; }
        public string x04InlineClasses { get; set; }
    }
}
