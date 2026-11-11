namespace UI.Models
{
    /// <summary>Vstup pro Views/Shared/_p65Checklist.cshtml (check-list úkolu).</summary>
    public class p65ChecklistViewModel
    {
        public int p56ID { get; set; }                  //0 = nový, ještě neuložený úkol: položky se drží ve formuláři a uloží se s úkolem
        public bool CanEdit { get; set; }
        public List<BO.p65Checklist> Items { get; set; }
        public string PendingJson { get; set; }         //položky neuloženého úkolu: JSON pole textů
        public string PendingFieldName { get; set; } = "ChecklistPending";
        public string BoxCss { get; set; } = "rf-card"; //třídy obalu podle místa použití (karta úkolu / stránka záznamu)
    }
}
