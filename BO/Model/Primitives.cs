using System;
using System.Collections.Generic;
using System.Text;

namespace BO
{
    public class GetDouble
    {
        public double Value { get; set; }
    }

    public class GetInteger
    {
        public int Value { get; set; }
    }

    public class GetString
    {
        public string Value { get; set; }
    }

    public class GetBool
    {
        public bool Value { get; set; }
    }
    public class GetDate
    {
        public DateTime? Value { get; set; }
    }

    public class GetListOfPids
    {
        public int pid { get; set; }
        public int rowindex { get; set; }
    }
}
