
namespace BO
{
    public enum p32AbsenceFlagENUM
    {
        _None = 0,
        Nemoc=1,
        Dovolena=2,
        Ocr=3,
        Paragraf=4,
        SickDay=5,
        Obed=6,    
        RodicovskaDovolena=7,
        DarovaniKrve=8,
        NahradniVolnoZaPrescas=9,
        NeplaceneVolno=10,
        Svatek=11,
        VojenskeCviceni=12,
        MaterskaDovolena=13,
        Other=99
    }
    public enum p32AbsenceBreakFlagENUM
    {
        _None=0,
        WorkBreak=1,
        SafetyBreak=2
    }


    public enum p32SystemFlagENUM
    {
        _None = 0,
        _CreditNote = 1
    }


    public class p32Activity : BaseBO
    {
        public p32SystemFlagENUM p32SystemFlag { get; set; } = p32SystemFlagENUM._None;
        public int p34ID { get; set; }
        public int p95ID { get; set; }
        public int p35ID { get; set; }
        public int p38ID { get; set; }
        public x15IdEnum x15ID { get; set; }
        public string p32Name { get; set; }
        public string p32Code { get; set; }
        public bool p32IsBillable { get; set; }
        public bool p32IsTextRequired { get; set; }
        public int p32Ordinary { get; set; }
        
        public double p32Value_Default { get; set; }
        public double p32Value_Minimum { get; set; }
        public double p32Value_Maximum { get; set; }
        public string p32DefaultWorksheetText { get; set; }
       

        public bool p32IsSupplier { get; set; }
        public bool p32IsCP { get; set; }
        public string p32Name_BillingLang1 { get; set; }
        public string p32Name_BillingLang2 { get; set; }
        public string p32Name_BillingLang3 { get; set; }
        public string p32Name_BillingLang4 { get; set; }
        public string p32DefaultWorksheetText_Lang1 { get; set; }
        public string p32DefaultWorksheetText_Lang2 { get; set; }
        public string p32DefaultWorksheetText_Lang3 { get; set; }
        public string p32DefaultWorksheetText_Lang4 { get; set; }
        public string p32ExternalCode { get; set; }

        public int p32ManualFeeFlag { get; set; }   //0: honorář se počítá násobkem hodin a hodinové sazby, 1: honorář zadává uživatel ručně při vykazování
        public double p32ManualFeeDefAmount { get; set; }
        public double p32MarginHidden { get; set; }
        public double p32MarginTransparent { get; set; }
        public p32AbsenceFlagENUM p32AbsenceFlag { get; set; }        
        public p32AbsenceBreakFlagENUM p32AbsenceBreakFlag { get; set; }
        public int p41ID_Absence { get; set; }
        public string p32Color { get; set; }
        public string p32HelpText { get; set; }

        public string p34Name { get; }
        
        public int p33ID { get; }
        
        public BO.p34IncomeStatementFlagENUM p34IncomeStatementFlag { get; }
        

        public string NameWithSheet
        {
            get
            {
                return $"{p32Name} ({p34Name})";
                
            }
        }
        public string CodeWithName
        {
            get
            {
                if (p32Code == null)
                    return p32Name;
                else
                    return $"{p32Code} - {p32Name}";
            }
        }
        public string NameWithCode
        {
            get
            {
                if (p32Code == null)
                    return p32Name;
                else
                    return $"{p32Name} ({p32Code})";
            }
        }

        public string p95Name { get; }
       
        
        public string p38Name { get; }
        public int p38Ordinary { get; }


        
    }


}
