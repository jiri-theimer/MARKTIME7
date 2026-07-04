namespace MO.Models
{
    public class EntryMoneyViewModel : BaseViewModel
    {
        public int pid { get; set; }                     // 0 = nový záznam
        public DateTime Date { get; set; }

        // Sešit (p34) - PRVNÍ volba, určuje konkrétní typ peněžního úkonu (p33id) a nabídku aktivit
        public int p34ID { get; set; }
        public IEnumerable<ComboItem> SesitComboItems { get; set; } = new List<ComboItem>();
        public string SelectedSesitText { get; set; }
        // Režim zadávání aktivity dle vybraného sešitu (1=nezadává, 2=nepovinná, 3=povinná)
        public int ActivityEntryFlag { get; set; }

        // Typ peněžního úkonu dle sešitu - řídí zobrazení polí v UI
        // BO.p33IdENUM.PenizeBezDPH (2) nebo BO.p33IdENUM.PenizeVcDPHRozpisu (5)
        public BO.p33IdENUM p33ID { get; set; }
        // BO.p34IncomeStatementFlagENUM.Vydaj (1) nebo Prijem (2) - řídí zobrazení kódu dokladu / druhu úhrady
        public int IncomeStatementFlag { get; set; }

        // Projekt (p41) - povinné
        public int p41ID { get; set; }
        public IEnumerable<ComboItem> ProjectComboItems { get; set; } = new List<ComboItem>();
        public string SelectedProjectText { get; set; }

        // Aktivita (p32) - dle sešitu + projektu
        public int p32ID { get; set; }
        public IEnumerable<ComboItem> ActivityComboItems { get; set; } = new List<ComboItem>();
        public string SelectedActivityText { get; set; }

        // Úkol (p56) - volitelné
        public int p56ID { get; set; }
        public IEnumerable<BO.p56Task> TaskList { get; set; } = new List<BO.p56Task>();
        public string SelectedTaskText { get; set; }

        // Částka bez DPH - povinné
        public string AmountWithoutVat { get; set; }

        // DPH - pouze pro p33ID == PenizeVcDPHRozpisu
        public string VatRatePercent { get; set; }
        public string AmountWithVat { get; set; }
        public string AmountVat { get; set; }

        // Měna - povinné
        public int j27ID { get; set; }
        public IEnumerable<ComboItem> CurrencyComboItems { get; set; } = new List<ComboItem>();
        public string SelectedCurrencyText { get; set; }

        // Kód dokladu a druh úhrady - pouze pro výdaj (IncomeStatementFlag == Vydaj)
        public string DocumentCode { get; set; }
        public int j19ID { get; set; }
        public IEnumerable<ComboItem> PaymentTypeComboItems { get; set; } = new List<ComboItem>();
        public string SelectedPaymentTypeText { get; set; }

        // Popis - povinné
        public string Description { get; set; }

        /// <summary>Uživatelská pole (freefields) - plní se v controlleru, ne z formuláře</summary>
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public FreeFieldsViewModel ff1 { get; set; } = new FreeFieldsViewModel();

        /// <summary>Záznam je pouze pro čtení (nemá OwnerAccess nebo stav != Editing)</summary>
        public bool IsReadOnly { get; set; }

        /// <summary>Popis stavu záznamu pro zobrazení v ReadOnly hlavičce</summary>
        public string RecordStateLabel { get; set; }

        /// <summary>Odkud přišel uživatel - "week" nebo null (Day). Řídí kam vede Zpět/Uložit.</summary>
        public string Ret { get; set; }

        /// <summary>Kotevní datum pro návrat (u "week" pondělí daného týdne)</summary>
        public string RetD { get; set; }

        /// <summary>Přílohy (fotky účtenek apod.) navázané na tento úkon - jen u již uloženého záznamu</summary>
        public List<BO.o27Attachment> Attachments { get; set; } = new List<BO.o27Attachment>();

        /// <summary>
        /// GUID pro dočasné uložení příloh u dosud neuloženého (nového) záznamu - commitne se
        /// přes Factory.o27AttachmentBL.SaveDropzoneFromTemp až po úspěšném uložení úkonu.
        /// </summary>
        public string UploadGuid { get; set; }

        /// <summary>Zatím jen dočasně nahrané soubory (p85Tempbox) u nového záznamu, ještě necommitnuté</summary>
        public List<BO.p85Tempbox> StagedAttachments { get; set; } = new List<BO.p85Tempbox>();
    }
}