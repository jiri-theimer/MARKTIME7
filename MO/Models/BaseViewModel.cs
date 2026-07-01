namespace MO.Models
{
    public class BaseViewModel
    {
        public bool IsPostback { get; set; }
        public string PostbackOper { get; set; }

        public string PageTitle { get; set; }
        public string PageTitleAfter { get; set; }

        /// <summary>
        /// Potlačí vykreslení hlavního nadpisu (H1) v layoutu - PageTitle se použije jen jako &lt;title&gt; stránky.
        /// Používá se u stránek, které si titulek/datum zobrazují vlastní hlavičkou (Day, Index, Week v Kalendáři).
        /// </summary>
        public bool HideHeaderTitle { get; set; }

        /// <summary>id elementu, na který se má automaticky udělit focus po načtení stránky</summary>
        public string Element2Focus { get; set; }

        public string PageSymbol { get; set; }

        public string Javascript_CallOnLoad { get; set; }

        /// <summary>Chybová zpráva zobrazená layoutem (může obsahovat HTML)</summary>
        public string Message { get; set; }

        /// <summary>Úspěšná zpráva zobrazená layoutem</summary>
        public string MessageSuccess { get; set; }
    }
}