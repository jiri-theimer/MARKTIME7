using System;

namespace BO
{
    public class TheEntity
    {
        private string _Prefix;
        private string _TableName;        
        public string AliasSingular { get; set; }
        public string AliasPlural { get; set; }

        public string TranslateLang1 { get; set; }
        public string TranslateLang2 { get; set; }
        public string TranslateLang3 { get; set; }
        public bool IsGlobalPeriodQuery { get; set; }
        public string SqlFromGrid { get; set; } //kořenová from klauzule pro GRID sql dotaz
       
        public string SqlOrderBy { get; set; }
        public string SqlOrderByCombo { get; set; }

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

       

        public bool IsWithoutValidity { get; set; }
        

    }
}
