using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class p31Record: BaseRecordViewModel
    {
        public bool IsShowDateCloneDialog { get; set; }
        public BO.p31WorksheetEntryInput Rec { get; set; }
        public BO.p31Worksheet RecP31 { get; set; }
        
        public BO.p41Project RecP41 { get; set; }
        public BO.p34ActivityGroup RecP34 { get; set; }
        public BO.p32Activity RecP32 { get; set; }
        
        public bool IsShowP56Combo { get; set; }
        
        public FreeFieldsViewModel ff1 { get; set; }
        public ReminderViewModel reminder { get; set; }

        public int SelectedLevelIndex { get; set; }
        public List<BO.ListItemValue> lisLevelIndex { get; set; }
        public string ProjectEntity { get; set; } = "p41Project";

        public DateTime? p31Date { get; set; }
        public bool IsMultiDate { get; set; }
        public string MultiDate { get; set; }
        public string IsdocLastUpload { get; set; }
        public string IsdocLastDokladText { get; set; }
        public IEnumerable<BO.p54OvertimeLevel> lisP54 { get; set; }
        public IEnumerable<BO.p40WorkSheet_Recurrence> lisP40 { get; set; }
        public IEnumerable<BO.p30ContactPerson> lisP30 { get; set; }
        public string SelectedComboProject { get; set; }
        public string SelectedComboPerson { get; set; }
        public string SelectedComboP32Name { get; set; }
        public string SelectedComboP34Name { get; set; }
        public string SelectedComboJ27Code { get; set; }
        public string SelectedComboJ19Name { get; set; }
        public string SelectedComboP35Code { get; set; }
        public string SelectedComboSupplier { get; set; }
        public string SelectedComboTask { get; set; }
        
        public string MyQueryInline_Project { get; set; }
      
        public int PiecePriceFlag { get; set; }

        public string CasOdDoIntervals { get; set; }

        public UI.Models.p31oper.hesViewModel Setting { get; set; }     //nastavení vykazování
        
        public bool IsValueTrimming { get; set; }   //zapisuje se korekce
        
        public string GuidApprove { get; set; }
        public string UploadGuid { get; set; }
        public int p91ID { get; set; }
        public int p68ID { get; set; }
        public int p49ID { get; set; }  //překlopený záznam finančního plánu
        public string DefaultText { get; set; } //hidden pomocník kvůli neschopnosti textarea aktualizovat multi-line text
        
        public int p61ID { get; set; }
        public string p61Name { get; set; }

        public int p72ID_DefaultTrimming { get; set; } //zobrazuje se, pokud má projekt vyplněnou výchozí korekci přes p72ID_BillableHours nebo p72ID_NonBillable

        public byte p34TrimmingFlag { get; set; }
        public byte p34NotepadFlag { get; set; }
        public byte p34FilesFlag { get; set; }
        public byte p34TagsFlag { get; set; }
        public byte p34InboxFlag { get; set; }

        public Notepad.EditorViewModel Notepad { get; set; }

        public DispoziceViewModel disp { get; set; }

        public string BillingLangFlagHtml { get; set; }
        public int BillingLangIndex { get; set; }
        
        public string Doc_o23Name { get; set; }

        public bool AutoOpenProjectSearchbox { get; set; }

        public int PravaZaJineho { get; set; } //1: vykazovat podle oprávnění vykazujícího uživatele, 2: vykazovat podle svých práv přihlášeného uživatele


        public int Kusovnik_p32ID1 { get; set; }
        public string Kusovnik_p31Text1 { get; set; }
        public double Kusovnik_Pocet1 { get; set; }

        public bool IsNavicKusovnik { get; set; }   //zda k hodinám vykázat i kusovníkové úkony
        public bool IsOfferNavicKusovnik { get; set; }
        public int p34ID_Kusovnik { get; set; }
        public List<KusovnikInline> lisKusovnik { get; set; }
    }

    public class KusovnikInline
    {
        
        public int p32ID { get; set; }
        public string p31Text { get; set; }
        public double Pocet { get; set; }
    }
}
