namespace MO.Models
{
    /// <summary>
    /// Univerzální položka pro mycombo. Naplnit projekcí z BO entit.
    /// </summary>
    public class ComboItem
    {
        /// <summary>PID záznamu (hidden value, posílá se s formulářem)</summary>
        public int Id { get; set; }

        /// <summary>Krátký kód / identifikátor (např. p41Code) - zobrazený v pevné šířce, mono font</summary>
        public string Code { get; set; }

        /// <summary>Hlavní text (např. p41NameShort) - hlavní popisek</summary>
        public string Text { get; set; }

        /// <summary>Volitelný dodatečný řádek (např. klient, stav, atd.)</summary>
        public string Meta { get; set; }

        /// <summary>
        /// Volitelné seskupení. Položky se stejnou hodnotou GroupBy budou vizuálně
        /// pod společnou hlavičkou (neklikatelný řádek s jiným pozadím).
        /// Pořadí skupin odpovídá pořadí prvního výskytu GroupBy v kolekci.
        /// </summary>
        public string GroupBy { get; set; }
    }
}