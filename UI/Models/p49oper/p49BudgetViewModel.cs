namespace UI.Models
{
    //Rozpočet projektu (Views/p49/Budget.cshtml); bez projektu přehled rozpočtů
    public class p49BudgetViewModel : BaseViewModel
    {
        public int p41id { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public bool CanEdit { get; set; }
        public int Year { get; set; }
        public int NewP41ID { get; set; }           //přehled: projekt vybraný pro nový rozpočet
        public string NewProject { get; set; }      //text vybraného projektu pro combo (taghelper vyžaduje výraz modelu)
    }
}
