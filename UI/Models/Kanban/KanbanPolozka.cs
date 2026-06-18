namespace UI.Models.Kanban
{
    public class KanbanPolozka
    {
        public string sloupec_nazev { get; set; }
        public string sloupec_poradi { get; set; }
        public string sloupec_barva { get; set; }
        public int pid { get; set; }
        public string prefix { get; set; }
        public string nazev { get; set; }
        public string nazev_after { get; set; }
        public string kod { get; set; }
        public double hodiny_nevyuctovane { get; set; }
        public double hodiny_vykazane { get; set; }
        public string b02Name { get; set; }
        public string b02Color { get; set; }
    }
}
