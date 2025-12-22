

namespace BO
{
    public enum PermValEnum
    {
        // Oprávnění globální aplikační role
        GR_Admin = 1,                        // Administrátor systému (menu Administrace)
        GR_p28_Creator = 2,                   // Vytvářet kontakty
        GR_p28_Reader = 3,                   // Čtenář všech kontaktů
        GR_p28_Owner = 4,                    // Vlastnická práva ke všem kontaktům
        GR_p41_Creator = 5,                   // Vytvářet projekty
        GR_p41_Reader = 6,                   // Čtenář všech projektů
        GR_p41_Owner = 7,                    // Vlastnická práva ke všem projektům
        GR_p51_Admin = 8,                    // Správce ceníků sazeb
        GR_o23_Creator = 9,                   // Vytvářet dokumenty všech typů
        GR_o23_Reader = 10,                   // Čtenář všech dokumentů
        GR_o23_Owner = 11,                    // Vlastnická práva ke všem dokumentům

        GR_MyInbox = 12,                          //Načítat inbox zprávy
                
        GR_o43_Reader=13,                       //čtenář všech inbox záznamů
        GR_o43_Owner = 14,                       //Vlastník všech inbox záznamů
        GR_MyProfile = 15,                        //Můj profil

        GR_b05ReadAll=16,                       //číst všechny b05 záznamy
        GR_b05OwnerAll=17,                      //vlastnická práva ke všem b05 záznamům

        GR_p90_Creator = 18,                  // vytvářet zálohové faktury
        GR_p90_Reader = 19,                  // Vlastnická práva ke všem zálohovým fakturám
        GR_p90_Owner = 20,                   // Čtenář všech zálohových faktur

        GR_P91_Creator = 24,                    // Oprávnění vystavovat faktury za všechny projekty
        GR_P91_Draft_Creator = 25,                // Oprávnění vystavovat DRAFT faktury za všechny projekty
        GR_P91_Reader = 26,                     // Čtenář všech faktur
        GR_P91_Owner = 27,                      // Vlastník všech faktur

        GR_p56_Reader = 28,                   // Čtenář všech úkolů
        GR_p56_Owner = 32,                    // Vlastnická práva ke všem kontaktům

        GR_o22_Reader=33,                      //čtenář všech termínů
        GR_o22_Owner=40,                        // Vlastnická práva ke všem termínům

        GR_P31_Reader = 21,                  // Čtenář všech worksheet záznamů
        GR_P31_Owner = 22,                   // Vlastnická práva všech worksheet záznamů
        GR_P31_Approver = 23,                // Oprávnění schvalovat všechny worksheet úkony v databázi
        GR_P31_Creator_Hours = 29,           // Oprávnění vykazovat hodiny a kusovník do jakéhokoliv projektu bez ohledu na projektovou roli
        GR_P31_Creator_Expenses = 30,        // Oprávnění vykazovat peněžní výdaje worksheet do jakéhokoliv projektu bez ohledu na projektovou roli
        GR_P31_Creator_Fees = 31,           // Oprávnění vykazovat odměny do jakéhokoliv projektu bez ohledu na projektovou roli

        GR_P31_Vysledovky=41,               //přístup k výsledovkám       
        GR_AllowRates =34,                

        GR_x31_ReadAll =35,                      //Přístup ke všem tiskovým sestavám
        GR_x31_Personal = 36,                  // Přístup k osobním tiskovým sestavám
        GR_GridColumnDesigner = 37,                   // Návrhář sloupců v gridech
        GR_GridExports = 38,                   // Export grid do XLS/PDF/

        GR_o51_Admin = 39,                     // Správce štítků

        // ---------Oprávnění projektové role------------------
        p41_Owner = 1,                    // Oprávnění vlastníka projektu
        p41_Reader = 3,                    // Oprávnění čtenáře projektu
        p41_CreateDraftInvoice=4,          //vytvářet draft vyúčtování v projektu
        p41_CreateInvoice=5,                //vytvářet ostré vyúčtování v projektu
        p41_ReadInvoice = 6,                //číst vyúčtování projektu
        p41_CreateTasks=7,                  //vytvářet v projektu úkoly a termíny
        p41_ReadDocs=10,                    //číst dokumenty projektu
        p41_client_ReadDocs=11,             //číst dokumenty klienta projektu
        //p41_Inbox =9,                       //přístup k projektovému inboxu


        // Oprávnění role v dokumentu
        o23_Reader = 1,                    // Oprávnění číst dokument
        o23_Owner = 2,                   // Oprávnění vlastníka dokumentu


        // Oprávnění role v kontaktu
        p28_Reader = 1,                    // Oprávnění číst kontakt
        p28_Owner = 2,                   // Oprávnění vlastníka kontaktu
        p28_Portal=10,                   //přístup na portál
        p28_ReadDocs=11,                //přístup k dokumentům klienta
       

        // Oprávnění role ve faktuře
        p91_Reader = 1,                    // Oprávnění číst vyúčtování
        p91_Owner = 2,                   // Oprávnění vlastníka vyúčtování

        // Oprávnění role v záloze
        p90_Reader = 1,                    // Oprávnění číst zálohu
        p90_Owner = 2,                   // Oprávnění vlastníka zálohy

        // Oprávnění role v úkolu
        p56_Reader = 1,                    // Oprávnění číst úkol
        p56_Owner = 2,                   // Oprávnění vlastníka úkolu
        p56_ChangeStatus=3,             //Oprávnění měnit stav úkolu
        p56_EntryP31 = 4,                   // Oprávnění vykazovat aktivity v úkolu

        // Oprávnění role v termínu
        o22_Reader = 1,                    // Oprávnění číst termín
        o22_Owner = 2,                   // Oprávnění vlastníka termínu

        //přístup k tiskové sestavě
        x31_Reader = 1,                    // Oprávnění číst report

    }
    public class Permission
    {       
        public string Entity { get; set; }
        public string Name { get; set; }      

        public string Group { get; set; }
        public PermValEnum Value { get; set; }

        public int ValueInt { get
            {
                return (int)this.Value;
            }
        }

    }
}
