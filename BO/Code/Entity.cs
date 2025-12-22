

namespace BO.Code
{
    public static class Entity
    {
        public static string GetPrefixDb(string prefix)
        {
            if (prefix == "le5" || prefix == "le4" || prefix == "le3" || prefix == "le2" || prefix == "le1") return "p41";

            return prefix;
        }

        
        

        public static string GetBarcode(string prefix,int pid)
        {
            switch (prefix)
            {
                case "o23":
                    return "323" + pid.ToString();
                case "p41":
                    return "241" + pid.ToString();
                case "p28":
                    return "228" + pid.ToString();
                case "p91":
                    return "291" + pid.ToString();
                case "p56":
                    return "256" + pid.ToString();
                case "p31":
                    return "231" + pid.ToString();
                case "j02":
                    return "102" + pid.ToString();
                case "o22":
                    return "322" + pid.ToString();
                default:
                    return prefix + pid.ToString();
            }
        }

        
        public static string GetEntity(string prefix)
        {
            switch (prefix.Substring(0, 3))
            {
                case "le1":
                case "le2":
                case "le3":
                case "le4":
                case "le5":
                    return prefix;


                case "p41":return "p41Project";

                case "b02": return "b02WorkflowStatus";
                case "b01": return "b01WorkflowTemplate";
                case "p42": return "p42ProjectType";
                case "p44": return "p44ProjectTemplate";

                case "j02": return "j02User";
                case "j04": return "j04UserRole";
                case "j07": return "j07PersonPosition";
                case "p28": return "p28Contact";
                case "p29": return "p29ContactType";
                case "p24": return "p24ContactGroup";

                case "o23": return "o23Doc";
                case "o18": return "o18DocType";

                case "j27": return "j27Currency";
                case "x31": return "x31Report";

                case "j18": return "j18CostUnit";

                case "p31": return "p31Worksheet";
                case "p32": return "p32Activity";
                case "p34": return "p34ActivityGroup";
                case "p54": return "p54OvertimeLevel";

                case "p56": return "p56Task";
                case "p57": return "p57TaskType";
                case "o22": return "o22Milestone";
                case "o21": return "o21MilestoneType";

                case "p40": return "p40WorkSheet_Recurrence";
                case "p49":return "p49FinancialPlan";
                case "p51": return "p51PriceList";
                case "p80": return "p80InvoiceAmountStructure";
                case "p90": return "p90Proforma";
                case "p91": return "p91Invoice";
                case "p82": return "p82Proforma_Payment";
                case "p83": return "p83UpominkaType";
                case "p84": return "p84Upominka";
                case "p85": return "p85TempBox";
                case "p15": return "p15Location";
                case "o43": return "o43Inbox";
                case "b20": return "b20Hlidac";
                case "p58": return "p58TaskRecurrence";
                case "p60": return "p60TaskTemplate";
                case "b05": return "b05Workflow_History";
                case "p11": return "p11Attendance";

                case "o51": return "o51Tag";
                case "o53": return "o53TagGroup";
                case "x67": return "x67EntityRole";
                case "x97": return "x97Translate";
                case "x01":return "x01License";
                case "x55": return "x55Widget";
                case "x54": return "x54WidgetGroup";
                case "fp1":
                case "fp2":
                case "fp3":
                    return prefix;
                default:
                    return null;
            }

        }

        public static string GetAlias(string prefix)
        {
            switch (prefix.Substring(0, 3))
            {
                case "p41":
                case "le5":
                    return "Projekt";
                case "j02":
                    return "Uživatel";
                case "p28":
                    return "Kontakt";
                case "p91":
                    return "Vyúčtování";
                case "p90":
                    return "Záloha";
                case "p56":
                    return "Úkol";
                case "o22":
                    return "Termín/Lhůta";
                case "o23":                
                    return "Dokument";
                case "p31":
                    return "Úkon";
                case "p40":
                    return "Opakovaná odměna";
                case "p15":
                    return "Lokalita";
                case "o43":
                    return "Inbox";
                case "b20":
                    return "Hlídač";
                case "p84":
                    return "Upomínka";
                case "p58":
                    return "Opakovaný úkol";
                case "p60":
                    return "Šablona úkolu";
                case "p11":
                    return "Docházka";
                case "x55":
                    return "Widget";
                default:
                    return null;

            }
        }

        public static List<BO.PermValEnum> GetAllPermValues(string prefix)
        {
            var lis = GetAllPermissions(prefix);
            return lis.Select(p => p.Value).ToList();
        }
        public static List<int> GetAllPermIntValues(string prefix)
        {
            var lis = GetAllPermissions(prefix);
            return lis.Select(p => Convert.ToInt32(p.Value)).ToList();
        }
        public static List<BO.Permission> GetAllPermissions(string prefix)
        {
            var lis = new List<BO.Permission>();
            if (prefix == "j04")
            {
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_Admin, Name = "Administrátor systému" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p28_Creator, Name = "Vytvářet kontakty všech typů", Group = "Kontakty" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p28_Reader, Name = "Číst všechny kontakty", Group = "Kontakty" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p28_Owner, Name = "Vlastnické oprávnění ke všem kontaktům", Group = "Kontakty" });

                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_o23_Creator, Name = "Vytvářet dokumenty všech typů", Group = "Dokumenty" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_o23_Reader, Name = "Číst všechy dokumenty", Group = "Dokumenty" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_o23_Owner, Name = "Vlastnické oprávnění ke všem dokumentům", Group = "Dokumenty" });

                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p41_Creator, Name = "Vytvářet projekty všech typů", Group = "Projekty" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p41_Reader, Name = "Číst všechny projekty", Group = "Projekty" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p41_Owner, Name = "Vlastnické oprávnění k projektům všech typů", Group = "Projekty" });

                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P31_Creator_Hours, Name = "Vykazovat hodiny a kusovník ve všech projektech", Group = "Úkony" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P31_Creator_Expenses, Name = "Vykazovat výdaje ve všech projektech", Group = "Úkony" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P31_Creator_Fees, Name = "Vykazovat pevné odměny ve všech projektech", Group = "Úkony" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P31_Reader, Name = "Číst všechny vykázané úkony (hodiny/výdaje/odměny)", Group = "Úkony" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P31_Owner, Name = "Vlastnické oprávnění ke všem úkonům (hodiny/výdaje/odměny)", Group = "Úkony" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P31_Approver, Name = "Schvalovat všechny úkony (hodiny/výdaje/odměny)", Group = "Úkony" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P31_Vysledovky, Name = "Výsledovky a nákladové sazby", Group = "Úkony" });

                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_AllowRates, Name = "Přístup k billing informacím (sazby/honoráře/částky odměn)", Group = "Billing/Vyúčtování" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p51_Admin, Name = "Správce ceníků hodinových sazeb", Group = "Billing/Vyúčtování" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P91_Creator, Name = "Vytvářet vyúčtování za všechny úkony", Group = "Billing/Vyúčtování" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P91_Draft_Creator, Name = "Vytvářet DRAFT vyúčtování za všechny úkony", Group = "Billing/Vyúčtování" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P91_Reader, Name = "Číst všechna vyúčtování", Group = "Billing/Vyúčtování" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_P91_Owner, Name = "Vlastnické oprávnění ke všem vyúčtováním", Group = "Billing/Vyúčtování" });

                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p56_Reader, Name = "Číst všechny úkoly", Group = "Úkoly" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p56_Owner, Name = "Vlastnické oprávnění ke všem úkolům", Group = "Úkoly" });

                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_o22_Reader, Name = "Číst všechny termíny/lhůty", Group = "Termíny/lhůty" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_o22_Owner, Name = "Vlastnické oprávnění ke všem termínům/lhůtám", Group = "Termíny/lhůty" });

                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p90_Creator, Name = "Vytvářet zálohy všech typů", Group = "Zálohy" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p90_Reader, Name = "Číst všechny zálohy", Group = "Zálohy" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_p90_Owner, Name = "Vlastnické oprávnění ke všem zálohám", Group = "Zálohy" });

                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_b05ReadAll, Name = "Číst všechny poznámky", Group = "Poznámky/Historie" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_b05OwnerAll, Name = "Vlastnické oprávnění ke všem poznámkám", Group = "Poznámky/Historie" });

                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_MyInbox, Name = "Importovat poštovní zprávy do Inbox", Group = "Inbox" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_o43_Reader, Name = "Číst všechny Inbox záznamy", Group = "Inbox" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_o43_Owner, Name = "Vlastnické oprávnění ke všem Inbox záznamům", Group = "Inbox" });

                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_MyProfile, Name = "Menu [Můj profil]", Group = "Ostatní" });
                
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_x31_Personal, Name = "Přístup k osobním tiskovým sestavám", Group = "Ostatní" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_o51_Admin, Name = "Správce štítků", Group = "Ostatní" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_GridColumnDesigner, Name = "Tabulka: Návrhář sloupců", Group = "Tabulky" });
                lis.Add(new BO.Permission() { Entity = "j04", Value = BO.PermValEnum.GR_GridExports, Name = "Tabulka: GRID-REPORT + Export do MS-EXCEL/CSV", Group = "Tabulky" });

               
            }



            if (prefix == "o23")
            {
                lis.Add(new BO.Permission() { Entity = "o23", Value = BO.PermValEnum.o23_Reader, Name = "Číst záznam dokumentu" });
                lis.Add(new BO.Permission() { Entity = "o23", Value = BO.PermValEnum.o23_Owner, Name = "Vlastník záznamu dokumentu" });
            }



            if (prefix == "p41")
            {
                lis.Add(new BO.Permission() { Entity = "p41", Value = BO.PermValEnum.p41_Reader, Name = "Číst záznam projektu" });
                lis.Add(new BO.Permission() { Entity = "p41", Value = BO.PermValEnum.p41_Owner, Name = "Vlastník záznamu projektu" });
                lis.Add(new BO.Permission() { Entity = "p41", Value = BO.PermValEnum.p41_CreateTasks, Name = "V projektu zakládat úkoly a termíny/lhůty" });
                lis.Add(new BO.Permission() { Entity = "p41", Value = BO.PermValEnum.p41_ReadInvoice, Name = "Přístup k vyúčtování projektu" });
                lis.Add(new BO.Permission() { Entity = "p41", Value = BO.PermValEnum.p41_CreateDraftInvoice, Name = "V projektu vystavovat DRAFT vyúčtování" });
                lis.Add(new BO.Permission() { Entity = "p41", Value = BO.PermValEnum.p41_CreateInvoice, Name = "V projektu vystavovat vyúčtování" });
                lis.Add(new BO.Permission() { Entity = "p41", Value = BO.PermValEnum.p41_ReadDocs, Name = "Přístup k dokumentům projektu" });
                lis.Add(new BO.Permission() { Entity = "p41", Value = BO.PermValEnum.p41_client_ReadDocs, Name = "Přístup k dokumentům klienta projektu" });

            }

            if (prefix == "p28")
            {
                lis.Add(new BO.Permission() { Entity = "p28", Value = BO.PermValEnum.p28_Reader, Name = "Číst záznam kontaktu" });
                lis.Add(new BO.Permission() { Entity = "p28", Value = BO.PermValEnum.p28_Owner, Name = "Vlastník záznamu kontaktu" });
                lis.Add(new BO.Permission() { Entity = "p28", Value = BO.PermValEnum.p28_Portal, Name = "Přístup na klientský portál (dashboard)" });
                lis.Add(new BO.Permission() { Entity = "p28", Value = BO.PermValEnum.p28_ReadDocs, Name = "Přístup k dokumentům" });

            }

            if (prefix == "p91")
            {
                lis.Add(new BO.Permission() { Entity = "p91", Value = BO.PermValEnum.p91_Reader, Name = "Číst záznam vyúčtování" });
                lis.Add(new BO.Permission() { Entity = "p91", Value = BO.PermValEnum.p91_Owner, Name = "Vlastník záznamu vyúčtování" });
            }

            if (prefix == "p90")
            {
                lis.Add(new BO.Permission() { Entity = "p90", Value = BO.PermValEnum.p90_Reader, Name = "Číst záznam zálohy" });
                lis.Add(new BO.Permission() { Entity = "p90", Value = BO.PermValEnum.p90_Owner, Name = "Vlastník záznamu zálohy" });
            }

            if (prefix == "p56")
            {
                lis.Add(new BO.Permission() { Entity = "p56", Value = BO.PermValEnum.p56_Reader, Name = "Číst záznam úkolu" });
                lis.Add(new BO.Permission() { Entity = "p56", Value = BO.PermValEnum.p56_Owner, Name = "Vlastník záznamu úkolu" });
                lis.Add(new BO.Permission() { Entity = "p56", Value = BO.PermValEnum.p56_ChangeStatus, Name = "Měnit stav úkolu" });
                lis.Add(new BO.Permission() { Entity = "p56", Value = BO.PermValEnum.p56_EntryP31, Name = "Přes úkol vykazovat v projektu úkony" });
            }

            if (prefix == "o22")
            {
                lis.Add(new BO.Permission() { Entity = "o22", Value = BO.PermValEnum.o22_Reader, Name = "Číst záznam termínu" });
                lis.Add(new BO.Permission() { Entity = "o22", Value = BO.PermValEnum.o22_Owner, Name = "Vlastník záznamu termínu" });
            }

            if (prefix == "x31")
            {
                lis.Add(new BO.Permission() { Entity = "x31", Value = BO.PermValEnum.x31_Reader, Name = "Přístup k sestavě" });
                
            }
            return lis;
        }
    }
}
