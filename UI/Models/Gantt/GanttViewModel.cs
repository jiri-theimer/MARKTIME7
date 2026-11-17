namespace UI.Models.Gantt
{
    public class GanttViewModel : BaseViewModel
    {
        public int p41id { get; set; }              //kontext jednoho projektu (včetně jeho podstromu), 0 = všechny naplánované projekty
        public BO.p41Project RecP41 { get; set; }
        public string Zoom { get; set; }            //day|week|month
        public bool ShowTasks { get; set; }
        public bool IsTasksModule { get; set; }
    }

    public class GanttRow
    {
        public string key { get; set; }             //p41-123 | p56-456
        public string prefix { get; set; }          //p41 | p56
        public int pid { get; set; }
        public string parent { get; set; }          //key nadřízeného řádku
        public int depth { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string sub { get; set; }             //klient projektu | stav úkolu
        public string d1 { get; set; }              //yyyy-MM-dd
        public string d2 { get; set; }              //yyyy-MM-dd
        public bool onlyend { get; set; }           //úkol má vyplněný jen termín (bez plánovaného zahájení)
        public double hours { get; set; }           //plán hodin
        public int edit { get; set; }               //0: jen čtení, 1: může upravit, 2: rozhodne server při uložení
        public bool closed { get; set; }
        public string color { get; set; }
    }
}
