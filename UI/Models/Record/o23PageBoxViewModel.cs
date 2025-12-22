namespace UI.Models.Record
{
    public class o23PageBoxViewModel:DynamicPageViewModel
    {
        public BO.a58RecPageBox RecA58 { get; set; }
        public BO.o23Doc Rec { get; set; }

        public IEnumerable<BO.o16DocType_FieldSetting> lisO16 { get; set; }

        public IEnumerable<BO.o19DocTypeEntity_Binding> lisO19 { get; set; }
    }
}
