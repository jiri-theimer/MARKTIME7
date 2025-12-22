
namespace UI.Models
{
    public enum PosEnum
    {
        Notepad=2,
        Files=4,
        BillingTab=8,
        Roles=16,
        Trimming=32,
        Tags=64,
        ProjectBudget=128,
        ProjectClient=256,
        Inbox=512,
        ProjectContacts=1024,
        NotepadIsBillingMemo=2048,
        ContactPersons=4096,
        ContactMedia = 8192
    }

    public class DispoziceViewModel
    {
        public List<DispoziceItem> Items { get; set; }
        public bool IsInhaled { get; set; }
        public void InitItem(string itemname,PosEnum pos,bool ischecked=false, byte defvalue=0)
        {
            if (this.Items == null) this.Items = new List<DispoziceItem>();
            this.Items.Add(new DispoziceItem() { ItemPos = pos, DefValue = defvalue, ItemName = itemname, IsChecked = ischecked });
        }
        public void InitItems(string prefix,BL.Factory f)
        {
            if (this.Items == null) this.Items = new List<DispoziceItem>();
            switch (prefix)
            {
                case "p28":
                  
                    InitItem(f.tra("Přílohy v kartě záznamu"), PosEnum.Files);
                    InitItem(f.tra("Záložka: Fakturační nastavení"), PosEnum.BillingTab);
                    InitItem(f.tra("Kontaktní osoby"), PosEnum.ContactPersons);
                    InitItem(f.tra("Kontaktní média"), PosEnum.ContactMedia);
                    InitItem(f.tra("Role | Oprávnění"), PosEnum.Roles);
                    break;
                case "p41":
                   
                    InitItem(f.tra("Přílohy v kartě záznamu"), PosEnum.Files);
                    InitItem(f.tra("Záložka: Fakturační nastavení"), PosEnum.BillingTab);
                    InitItem(f.tra("Obsazení projektových rolí"), PosEnum.Roles);
                    InitItem(f.tra("Plán/Rozpočet"), PosEnum.ProjectBudget);
                    InitItem(f.tra("Klient projektu"), PosEnum.ProjectClient);
                    InitItem(f.tra("Kontakty projektu (kromě klienta projektu)"), PosEnum.ProjectContacts);
                    break;
                case "o23":
                    InitItem("Nodepad poznámka", PosEnum.Notepad);
                    InitItem(f.tra("Přílohy"), PosEnum.Files);                    
                    InitItem(f.tra("Role | Oprávnění"), PosEnum.Roles);
                    InitItem(f.tra("Štítky"), PosEnum.Tags);
                    InitItem(f.tra("Inbox: Došlá pošta"), PosEnum.Inbox);
                    break;
                case "p31":
                    InitItem(f.tra("Korekce pro vyúčtování"), PosEnum.Trimming);
                    
                    InitItem(f.tra("Přílohy"), PosEnum.Files);
                    InitItem(f.tra("Štítky"), PosEnum.Tags);
                    InitItem(f.tra("Inbox: Došlá pošta"), PosEnum.Inbox);
                    break;
                case "j02":
                    
                    InitItem(f.tra("Přílohy v kartě záznamu"), PosEnum.Files);                    
                    break;
                case "p90":
                    
                    InitItem(f.tra("Přílohy v kartě záznamu"), PosEnum.Files);
                    InitItem(f.tra("Role | Oprávnění"), PosEnum.Roles);
                    break;
                case "p91":
                    
                    InitItem(f.tra("Přílohy v kartě záznamu"), PosEnum.Files);
                    InitItem(f.tra("Role | Oprávnění"), PosEnum.Roles);
                    break;
            }
            
        }
        private byte UpdateValFromCache(int intCacheBitstream,byte defval,PosEnum pos)
        {
            if (intCacheBitstream <= 0) return defval;
            byte ret = defval;
            if (defval != 2 && (intCacheBitstream & (int)pos) == (int)pos)  //hodnota 2 je: zákaz rozšíření zapnout
            {
                if (defval != 3) ret = 1;   //hodnota 3: zapnuto a nelze vypnout
            }

            return ret;
        }
        public byte SetVal(PosEnum pos,byte defval,int totalbitstream,int rec_pid,int intCacheBitstream)
        {
            if (intCacheBitstream > 0)
            {
                defval = UpdateValFromCache(intCacheBitstream, defval, pos);
            }
            this.Items.First(p => p.ItemPos == pos).DefValue = defval;
            switch (defval)
            {
                case 2:
                    //zakázáno, nelze vybrat
                    SetChecked(pos,false);
                    break;
                case 3:
                    //zapnuto, nelze vypnout
                    SetChecked(pos, true);
                    
                    break;
                default:    //0 nebo 1
                    if (totalbitstream > 0 && (totalbitstream & (int)pos) == (int)pos)
                    {
                        SetChecked(pos, true);   //zaškrtnuto
                    }
                    if (totalbitstream==0 && rec_pid==0 && defval == 1)
                    {
                        //předem zaškrtnuto
                        SetChecked(pos, true);
                    }
                    
                    break;
               
            }
            this.IsInhaled = true;

            return defval;
           
        }
        public void RecoveryDefaultCheckedStates()
        {
            foreach(var c in this.Items.Where(p=>p.DefValue==0))
            {
                c.IsChecked = false;
            }
            foreach (var c in this.Items.Where(p => p.DefValue == 1))
            {
                c.IsChecked = true;
            }
        }
        public void SetChecked(PosEnum pos, bool ischecked)
        {
            this.Items.First(p => p.ItemPos == pos).IsChecked = ischecked;
        }
        public DispoziceItem LoadItem(PosEnum pos)
        {
            return this.Items.First(p => p.ItemPos == pos);
        }

        public bool IsChecked(PosEnum pos)
        {
            if (this.Items.Any(p=>p.ItemPos==pos && (p.IsChecked || p.DefValue==3)))
            {
                return true;
            }

            return false;
        }

        public int GetBitStream()
        {
            int x = 0;
            foreach(var c in this.Items.Where(p=>p.IsChecked==true))
            {
                x += (int) c.ItemPos;
            }

            return x;
        }
        public string GetTabText(PosEnum pos)
        {
            switch (pos)
            {
                case PosEnum.BillingTab:
                    return "Fakturační nastavení";
                case PosEnum.Roles:
                    return "Role | Oprávnění";
                case PosEnum.Notepad:
                    if (this.IsChecked(PosEnum.Notepad) && this.IsChecked(PosEnum.NotepadIsBillingMemo))
                    {
                        return "Fakturační poznámka";
                    }
                    if (this.IsChecked(PosEnum.Files) && this.IsChecked(PosEnum.Notepad))
                    {
                        return "Poznámka | Přílohy";
                    }
                    if (this.IsChecked(PosEnum.Notepad))
                    {
                        return "Poznámka";
                    }
                    if (this.IsChecked(PosEnum.Files))
                    {
                        return "Přílohy";
                    }

                    return "Poznámka | Přílohy";
                case PosEnum.Files:
                    return "Přílohy";
                default:
                    return null;


            }
        }

        public bool IsNotepad
        {
            get
            {
                return this.IsChecked(PosEnum.Notepad);
            }
        }
        public bool IsFiles
        {
            get
            {
                return this.IsChecked(PosEnum.Files);
            }
        }
        public bool IsBilling
        {
            get
            {
                return this.IsChecked(PosEnum.BillingTab);
            }
        }
        public bool IsRoles
        {
            get
            {
                return this.IsChecked(PosEnum.Roles);
            }
        }
        public bool IsTrimming
        {
            get
            {
                return this.IsChecked(PosEnum.Trimming);
            }
        }
        public bool IsTags
        {
            get
            {
                return this.IsChecked(PosEnum.Tags);
            }
        }

        public bool IsBudget
        {
            get
            {
                return this.IsChecked(PosEnum.ProjectBudget);
            }
        }
        public bool IsProjectClient
        {
            get
            {
                return this.IsChecked(PosEnum.ProjectClient);
            }
        }
        public bool IsProjectContacts
        {
            get
            {
                return this.IsChecked(PosEnum.ProjectContacts);
            }
        }
        public bool IsContactMedia
        {
            get
            {
                return this.IsChecked(PosEnum.ContactMedia);
            }
        }
        public bool IsContactPersons
        {
            get
            {
                return this.IsChecked(PosEnum.ContactPersons);
            }
        }

    }





    public class DispoziceItem
    {
        public PosEnum ItemPos { get; set; }
        public string ItemName { get; set; }
        public bool IsChecked { get; set; }
        public byte DefValue { get; set; } = 0;//0: Povoleno, 1: Povoleno a Zapnuto, 2: Zakázáno, 3: Zapnuto bez možnosti vypnout
    }
}
