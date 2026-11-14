namespace MO.Models
{
    public class EntryMoneyViewModel : EntryBaseViewModel
    {
        // Typ peněžního úkonu dle sešitu - řídí zobrazení polí v UI
        // BO.p33IdENUM.PenizeBezDPH (2) nebo BO.p33IdENUM.PenizeVcDPHRozpisu (5)
        public BO.p33IdENUM p33ID { get; set; }
        // BO.p34IncomeStatementFlagENUM.Vydaj (1) nebo Prijem (2) - řídí zobrazení kódu dokladu / druhu úhrady
        public int IncomeStatementFlag { get; set; }

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