using System;

namespace BO
{
    public class o16DocType_FieldSetting:BaseBO
    {
        public int o16ID { get; set; }
        public int o18ID { get; set; }
        public bool o16IsEntryRequired { get; set; }
        public string o16Name { get; set; }
        public string o16NameGrid { get; set; }
        public string o16Field { get; set; }
        public int o16Ordinary { get; set; }
        public string o16DataSource { get; set; }
        public bool o16IsFixedDataSource { get; set; }
        public bool o16IsGridField { get; set; }
        public int o16TextboxHeight { get; set; }
        public int o16TextboxWidth { get; set; }        
        public bool o16IsReportField { get; set; }
        public string o16FieldGroup { get; set; }
        public string o16Format { get; set; }
        public string o16HelpText { get; set; }
        public int o16ReminderNotifyBefore { get; set; }

        public BO.x24IdENUM FieldType
        {
            get
            {
                if (this.o16Field.ToLower().Contains("number"))
                    return x24IdENUM.tDecimal;

                if (this.o16Field.ToLower().Contains("date"))
                    return x24IdENUM.tDateTime;

                if (this.o16Field.ToLower().Contains("boolean"))
                    return x24IdENUM.tBoolean;

                return x24IdENUM.tString;
            }
        }

        public string FieldTypeAlias
        {
            get
            {
                if (this.o16Field.ToLower().Contains("number"))
                    return "num";

                if (this.o16Field.ToLower().Contains("date"))
                    return "datetime";

                if (this.o16Field.ToLower().Contains("boolean"))
                    return "boolean";

                return "string";
            }
        }
        
    }
}
