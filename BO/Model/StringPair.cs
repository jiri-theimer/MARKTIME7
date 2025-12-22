using System;
using System.Collections.Generic;
using System.Text;


namespace BO
{
    public class StringPair
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class StringPairTimestamp
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime DateUpdate { get; set; }
    }

    public class ListItemValue
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
}
