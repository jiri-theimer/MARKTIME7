using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class j02Record: BaseRecordViewModel
    {
        public BO.j02User Rec { get; set; }
        public BO.j04UserRole RecJ04 { get; set; }
        public string ComboJ07Name { get; set; }
        public string ComboJ04Name { get; set; }
        public string ComboC21Name { get; set; }
        public string ComboJ18Name { get; set; }

        public string NewPassword { get; set; }
        public string VerifyPassword { get; set; }
        public bool IsDefinePassword { get; set; }
        public bool IsChangeLogin { get; set; }

        public string p34Names { get; set; }

        

        public int SelectedP28ID { get; set; }
        public string SelectedP28Name { get; set; }

        public FreeFieldsViewModel ff1 { get; set; }
        public IEnumerable<BO.o15AutoComplete> lisAutocomplete { get; set; }
        public string TempGuid { get; set; }
        public string UploadGuid { get; set; }
        public DispoziceViewModel disp { get; set; }

        public string SignatureInvoiceFile { get; set; }
        public string UploadGuidSignature { get; set; }
        public bool IsDeleteSignature { get; set; }

        
    }

   
}
