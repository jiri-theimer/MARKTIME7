namespace BO
{
    /// <summary>Oprávnění přihlášeného uživatele k úkonu a odkud plyne (verdikt z p31_inhale_disposition, důvody dohledá BL stejnými dotazy).</summary>
    public class p31DispositionExplain
    {
        public BO.p31Worksheet Record { get; set; }
        public bool CanRead { get; set; }
        public bool CanEdit { get; set; }
        public bool CanApprove { get; set; }
        public BO.p31RecordState RecordState { get; set; }
        public string LockedReasonMessage { get; set; }
        /// <summary>Čitelné důvody: text + co dává (čte/upravuje/schvaluje)</summary>
        public List<p31DispositionReason> Reasons { get; set; } = new List<p31DispositionReason>();
    }
    public class p31DispositionReason
    {
        public string Source { get; set; }      //vlastní záznam / role / nadřízený / projektová role
        public string Text { get; set; }
        public bool Read { get; set; }
        public bool Edit { get; set; }
        public bool Approve { get; set; }
        public string Url { get; set; }         //kde se to nastavuje (jen pro administrátora)
    }
    /// <summary>Řádek pro vysvětlení projektové role: role, kde je přiřazená, komu</summary>
    public class p31DispositionRoleRow
    {
        public string x67Name { get; set; }
        public int x67ID { get; set; }
        public string x69RecordEntity { get; set; }
        public int x69RecordPid { get; set; }
        public string RecordName { get; set; }
        public bool x69IsAllUsers { get; set; }
        public string j11Name { get; set; }
        public int o28PermFlag { get; set; }
    }
    /// <summary>Řádek pro vysvětlení vztahu nadřízený → osoba úkonu</summary>
    public class p31DispositionSuperiorRow
    {
        public int j05ID { get; set; }
        public string j11Name { get; set; }
        public int j05Disposition_p31 { get; set; }
    }
}
