using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum o23LockedTypeENUM
    {
        _NotSpecified = 0,
        LockAllFiles = 1
    }

    public class o23Doc:BaseBO
    {        
        public int o18ID { get; set; }   //typ dokumentu
        public int x04ID { get; set; }
        public int j95ID { get; set; }
        public string o18Name { get; }
        public o18TemplateENUM o18TemplateFlag { get; }
        public int j02ID_Owner { get; set; }
        public int b02ID { get; set; }
        public string o23Name { get; set; } //pro účtenku: Text úkonu
        public int o23BitStream { get; set; }
        public string o23Notepad { get; set; }
        public int o23Ordinary { get; set; }
        public string o23ArabicCode { get; set; }
        public string o23Code { get; set; }     //pro účtenku: kód dokladu
        public string o23ExternalCode { get; set; }
        public string o23BackColor { get; set; }
        public string o23ForeColor { get; set; }

        public bool o23IsEncrypted { get; set; }
        public string o23Password { get; set; }
        
        public Guid o23Guid { get; set; }

        public bool o23IsDraft { get; set; }
        public o23LockedTypeENUM o23LockedFlag { get; set; } = o23LockedTypeENUM._NotSpecified;
        public DateTime? o23ReminderDate { get; set; }

        public string Rezerva { get; set; }      
        public DateTime? o23LastLockedWhen { get; }             
        public string o23LastLockedBy { get; }
        
        public int b01ID { get; }      
        public int o17ID { get; }
        public string o23FreeText01 { get; set; }   //pro účtenku: Dodavatel
        public string o23FreeText02 { get; set; }
        public string o23FreeText03 { get; set; }
        public string o23FreeText04 { get; set; }
        public string o23FreeText05 { get; set; }
        public string o23FreeText06 { get; set; }
        public string o23FreeText07 { get; set; }
        public string o23FreeText08 { get; set; }
        public string o23FreeText09 { get; set; }
        public string o23FreeText10 { get; set; }
        public string o23FreeText11 { get; set; }
        public string o23FreeText12 { get; set; }
        public string o23FreeText13 { get; set; }
        public string o23FreeText14 { get; set; }
        public string o23FreeText15 { get; set; }        
        public double o23FreeNumber01 { get; set; } //pro účtenku: Čáska vč. DPH
        public double o23FreeNumber02 { get; set; } //pro účtenku: Sazba DPH
        public double o23FreeNumber03 { get; set; } //pro účtenku: Částka bez DPH
        public double o23FreeNumber04 { get; set; } //pro účtenku: Částka DPH
        public double o23FreeNumber05 { get; set; }
        public DateTime? o23FreeDate01 { get; set; }    //pro účtenku: datum
        public DateTime? o23FreeDate02 { get; set; }
        public DateTime? o23FreeDate03 { get; set; }
        public DateTime? o23FreeDate04 { get; set; }
        public DateTime? o23FreeDate05 { get; set; }
        public bool o23FreeBoolean01 { get; set; }
        public bool o23FreeBoolean02 { get; set; }
        public bool o23FreeBoolean03 { get; set; }
        public bool o23FreeBoolean04 { get; set; }
        public bool o23FreeBoolean05 { get; set; }

       
        public string TagsInlineHtml { get; set; }
        public int o27ExistInt { get; set; }

        public int p34ID_Expense { get; set; }
        public int p32ID_Expense { get; set; }
        public int j27ID_Expense { get; set; }
        public int p41ID_Expense { get; set; }


        public string b02Name { get; }
      
        public string b02Color { get; }
        public int x38ID { get; }
      
        public string NameWithCode
        {
            get
            {
                if (this.o23Code == null)
                    return this.o23Name;
                else
                    return this.o23Name + " (" + this.o23Code + ")";
            }
        }
        public string StyleDecoration
        {
            get
            {
                if (this.isclosed)
                    return "line-through";
                else
                    return "";
            }
        }

        public string Owner { get; }
        
    }
}
