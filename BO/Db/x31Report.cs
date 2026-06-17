using System;


namespace BO
{
    public enum x31FormatFlagENUM
    {
        Telerik = 1,
        DOCX = 2,       
        XLSX = 4
    }
    public enum x31QueryFlagENUM
    {
        _None = 0,
        p31 = 331,
        p41 = 141,
        p28 = 328,
        p91 = 391,
        p56 = 356,
        j02 = 102,
        o23 = 223
    }
    public class x31Report : BaseBO
    {
        public int x01ID { get; set; }
        public string x31Entity { get; set; }
        public x31FormatFlagENUM x31FormatFlag { get; set; }
        public int j25ID { get; set; }
        public string x31Code { get; set; }
        public string x31Name { get; set; }
        public string x31Description { get; set; }
        public int x31Ordinary { get; set; }
        public string x31FileName { get; set; }

        public bool x31IsPeriodRequired { get; set; }
        public bool x31IsUsableAsPersonalPage { get; set; }
        public bool x31IsScheduling { get; set; }
        public string x31SQLSchedulingCondition { get; set; }

        public bool x31IsRunInDay1 { get; set; }
        public bool x31IsRunInDay2 { get; set; }
        public bool x31IsRunInDay3 { get; set; }
        public bool x31IsRunInDay4 { get; set; }
        public bool x31IsRunInDay5 { get; set; }
        public bool x31IsRunInDay6 { get; set; }
        public bool x31IsRunInDay7 { get; set; }
        public string x31RunInTime { get; set; }
        public string x31SchedulingReceivers { get; set; }
        public DateTime? x31LastScheduledRun { get; set; }
        public int x21ID_Scheduling { get; set; }
        public int j70ID_Scheduling { get; set; }

        public string x31DocSqlSource { get; set; }
        public string x31DocSqlSourceTabs { get; set; }
        public string x31ExportFileNameMask { get; set; }
        public x31QueryFlagENUM x31QueryFlag { get; set; } = x31QueryFlagENUM._None;
        public int x31PluginHeight { get; set; }
        public DateTime? x31FactoryTimestamp { get; set; }
        public int x31LangIndex { get; set; }
        public bool x31IsAllowPfx { get; set; }
        public string Rezerva { get; set; }
        public string ReportFileName { get; set; }  //je třeba zde mít SET, protože se využívá v json Deserialize, který ignoruje readonly property
        public string ReportFolder { get; }
        
        public string j25Name { get; }
        
        public string j25Code { get; set; }
        public int j25Ordinary { get; set; }
        public int o27ID { get; }

        public string FormatName
        {
            get
            {
                switch (this.x31FormatFlag)
                {
                    case x31FormatFlagENUM.DOCX:
                        {
                            return "DOCX";
                        }

                    case x31FormatFlagENUM.Telerik:
                        {
                            return "REPORT";
                        }

                    case x31FormatFlagENUM.XLSX:
                        {
                            return "XLSX";
                        }

                    default:
                        {
                            return "";
                        }
                }
            }
        }

        public string NameWithCode
        {
            get
            {
                return this.x31Name + " [" + this.x31Code + "]";
            }
        }
        public string NameWithFormat
        {
            get
            {
                string s = this.x31Name;
                switch (this.x31FormatFlag)
                {

                    case x31FormatFlagENUM.DOCX:
                        {
                            return s + " (DOCX)";
                        }

                    case x31FormatFlagENUM.XLSX:
                        {
                            return s + " (XLSX)";
                        }

                    default:
                        {
                            return s;
                        }
                }
            }
        }

    }
}
