namespace UI.Models
{
    /// <summary>Vstup pro Views/Shared/_p65Checklist.cshtml (check-list úkolu nebo dokumentu, vzor check-listu v opakovaném úkolu).</summary>
    public class p65ChecklistViewModel
    {
        public string Prefix { get; set; } = "p56";     //"p56" = úkol, "o23" = dokument, "p58" = opakovaný úkol (vzor: položky se neodškrtávají)
        public int RecPid { get; set; }                 //0 = nový, ještě neuložený záznam: položky se drží ve formuláři a uloží se s ním
        public bool CanEdit { get; set; }
        public List<BO.p65Checklist> Items { get; set; }
        public string PendingJson { get; set; }         //položky neuloženého záznamu: JSON pole textů
        public string PendingFieldName { get; set; } = "ChecklistPending";
        public string BoxCss { get; set; } = "rf-card"; //třídy obalu podle místa použití (karta záznamu / stránka záznamu)
    }
}
