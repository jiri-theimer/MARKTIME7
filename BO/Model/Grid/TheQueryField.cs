using System;
using System.Collections.Generic;
using System.Text;

namespace BO
{
    public class TheQueryField
    {
        private string _Entity;
        private string _Prefix;
        private string _Field;
        public string Entity
        {
            get
            {
                return _Entity;
            }
            set
            {
                _Entity = value;

                _Prefix = _Entity.Substring(0, 3);
            }
        }
        
        public string Field { get => _Field; set => _Field = value; }
        public string FieldSqlSyntax { get; set; }
        public string FieldType { get; set; }   //string, bool, multi, combo, number, date, datetime
        public string Header { get; set; }

        public string SourceEntity { get; set; }
        public string SourceSql { get; set; }
        public bool IsOfferPersonsAdd { get; set; }   //true: nabízet dodatečně i filtr podle uživatelů: j02AddValue

        public string MyQueryInline { get; set; }
        public string SqlWrapper { get; set; }

        public string TranslateLang1 { get; set; }
        public string TranslateLang2 { get; set; }
        public string TranslateLang3 { get; set; }

        public string MasterPrefix { get; set; }
        public int MasterPid { get; set; }
        
    }
}
