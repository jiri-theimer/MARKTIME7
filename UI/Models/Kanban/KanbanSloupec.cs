namespace UI.Models.Kanban
{
    public class KanbanSloupec
    {
        public int pid { get; set; }
        public string nazev { get; set; }
        public string barva { get; set; }
        public int pagenum { get; set; }
        public int pagesize { get; set; } = 200;
    }
}
