namespace UI.Models.Record
{
    public class b20Record: BaseRecordViewModel
    {
        public BO.b20Hlidac Rec { get; set; }
        public string ComboTeam { get; set; }
        public string ComboRole1 { get; set; }
        public string ComboRole2 { get; set; }

        /// <summary>Hotová pravidla nabízená na kartě nového hlídače (jen GET, pid = 0).</summary>
        public List<BL.b20HlidacTemplate> Templates { get; set; }
        /// <summary>Klíč šablony, ze které je karta právě předvyplněná.</summary>
        public string TemplateKey { get; set; }

        
    }
}
