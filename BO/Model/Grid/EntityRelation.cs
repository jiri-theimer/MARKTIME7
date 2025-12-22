using System;
using System.Collections.Generic;
using System.Text;

namespace BO
{
    public class EntityRelation
    {
        private string _Prefix;
        private string _TableName;

        public string TableName
        {
            get
            {
                return _TableName;
            }
            set
            {
                _TableName = value;

                _Prefix = _TableName.Substring(0, 3);
            }
        }
        public string Prefix
        {
            get
            {
                return _Prefix;
            }
        }

        public string AliasSingular { get; set; }
        public string Translate1 { get; set; }
        public string Translate2 { get; set; }

        public string SqlFrom { get; set; }

        public string RelName { get; set; }

        public string RelNameDependOn { get; set; }

       

    }
}
