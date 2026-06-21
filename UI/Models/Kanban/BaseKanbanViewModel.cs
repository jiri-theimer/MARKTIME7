using Markdig.Renderers.Html;

namespace UI.Models.Kanban
{
    public class BaseKanbanViewModel:BaseViewModel
    {
        public string viewtype { get; set; }
        public p31StateQueryViewModel p31statequery { get; set; }   //filtrování podle stavu aktivit v horním pruhu

        public TheGridQueryViewModel TheGridQueryButton { get; set; }

        public List<KanbanPolozka> polozky { get; set; }
        public List<KanbanSloupec> sloupce { get; set; }
        public List<BO.StringPair> viewtypes { get; set; }

        public int go2pid { get; set; }

        public int j72id_query { get; set; }    //pojmenovaný filtr


        public RecordBinQueryViewModel recordbinquery { get; set; } //filtrování podle archivu
    }
}
