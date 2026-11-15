namespace UI.Views.Shared.Components.myGrid
{
    //Nabídka hodnot sloupce pro sloupcový filtr "Vybrat z hodnot" (myGridSupport.Event_HandleTheGridFilterValues)
    public class myGridFilterValues
    {
        public bool supported { get; set; }     //false: pro tento sloupec nelze seznam hodnot sestavit (není textový, SQL výraz nejde seskupit)
        public bool truncated { get; set; }     //hodnot je víc než limit: zobrazena jen první část, zbytek přes hledání
        public int limit { get; set; }
        public int blankcount { get; set; }     //počet záznamů s prázdnou hodnotou (ty se filtrují operátorem "Je prázdné")
        public List<myGridFilterValue> items { get; set; } = new List<myGridFilterValue>();
    }

    public class myGridFilterValue
    {
        public string v { get; set; }   //hodnota
        public int c { get; set; }      //počet záznamů
    }
}
