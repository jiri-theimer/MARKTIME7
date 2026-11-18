namespace UI.Models.Record
{
    public class x07Record: BaseRecordViewModel
    {
        public BO.x07Integration Rec { get; set; }

        public bool IsShowToken { get; set; }
        public bool IsShowLoginPassword { get; set; }

        //nastavení podle typu (ukládá se do x07Config)
        public BO.x07Config Config { get; set; } = new BO.x07Config();
        public List<string> DocMapNames { get; set; } = new List<string>();     //názvy typů dokumentů k řádkům Config.DocMaps
        public bool HasPassword { get; set; }       //heslo / token už je uložený: prázdné pole při uložení znamená "beze změny"
        public bool HasToken { get; set; }
        public string TestMessage { get; set; }     //výsledek testu spojení
        public bool TestOk { get; set; }
        public IEnumerable<BO.x09IntegrationLog> lisLog { get; set; }
    }
}
