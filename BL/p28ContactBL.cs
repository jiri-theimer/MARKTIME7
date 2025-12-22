

namespace BL
{
    public interface Ip28ContactBL
    {
        public BO.p28Contact Load(int pid);        
        public BO.p28Contact LoadByCode(string strCode, int pid_exclude);
        public BO.p28Contact LoadByGuid(string guid);
        public BO.p28Contact LoadByICO(string strICO, int pid_exclude);
        public BO.p28Contact LoadByDIC(string strDIC, int pid_exclude);
        public BO.p28Contact LoadByBankAccount(string bankovni_ucet);
        public IEnumerable<BO.p28Contact> GetList(BO.myQueryP28 mq, bool ischangelog = false);
        public IEnumerable<BO.o32Contact_Medium> GetList_o32(int p28id,int p24id,int p30id);
        public int Save(BO.p28Contact rec, List<BO.o32Contact_Medium> lisO32, List<BO.FreeFieldInput> lisFFI,string tempguid, List<BO.x69EntityRole_Assign> lisX69, List<BO.p30ContactPerson> lisP30,string billingmemo=null);
        public bool ValidateBeforeSave(BO.p28Contact rec, List<BO.o32Contact_Medium> lisO32, List<BO.p30ContactPerson> lisP30);
        public BO.p28ContactSum LoadSumRow(int pid);
        
        public BO.p28RecDisposition InhaleRecDisposition(int pid, BO.p28Contact rec = null);
        public string GetBillingFlagHtml(BO.p28Contact rec);
        public IEnumerable<BO.p26ProjectContact> GetList_p26(int p28id);
        public IEnumerable<BO.p30ContactPerson> GetList_p30(int p28id);
        public IEnumerable<BO.p30ContactPerson> GetList_p30(List<int> p28ids);
        public IEnumerable<BO.p30ContactPerson> GetList_p30_mother(int p28id);
        public IEnumerable<BO.p30ContactPerson> GetList_p30_p31entry(int p28id);    //nabídka kontaktních osob pro vykazování úkonu
        public IEnumerable<BO.p28MyTop10> GetList_MyTop10(int j02id, int toprecs);   //nabídka naposledy vykazovaných klientů
        public BO.p28Contact LoadTreeTopRec(BO.p28Contact rec);
        public string LoadBillingMemo(int p28id);
        
    }
    class p28ContactBL : BaseBL,Ip28ContactBL
    {
        public p28ContactBL(BL.Factory mother):base(mother)
        {
           
        }

        private string GetSQL1(string strAppend = null,int intRecsTop=0, bool ischangelog = false,bool istestcloud=false)
        {
            if (intRecsTop == 0)
            {
                sb("SELECT a.*");
            }
            else
            {
                sb($"SELECT TOP {intRecsTop} a.*");
            }
                       
            sb(",p29x.p29Name,p29x.p29ScopeFlag,p92.p92Name,b02.b02Name");            
            sb(",p51billing.p51Name as p51Name_Billing,j02owner.j02Name as Owner");
            sb(",p29x.x38ID,p29x.b01ID");
            if (!ischangelog)
            {
                sb(",a.p28Cache_p31Count");
            }
            sb($",{_db.GetSQL1_Ocas("p28")} FROM {(ischangelog ? "p28Contact_Log a": "p28Contact a")} INNER JOIN p29ContactType p29x ON a.p29ID=p29x.p29ID");

            sb(" LEFT OUTER JOIN p92InvoiceType p92 ON a.p92ID=p92.p92ID");
            sb(" LEFT OUTER JOIN b02WorkflowStatus b02 ON a.b02ID=b02.b02ID");
            sb(" LEFT OUTER JOIN p51PriceList p51billing ON a.p51ID_Billing=p51billing.p51ID");
            sb(" LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");

            if (istestcloud)
            {
                sb(this.AppendCloudQuery(strAppend, "p29x.x01ID"));
            }
            else
            {
                sb(strAppend);
            }
            
            return sbret();
        }
       
        public BO.p28Contact Load(int intPID)
        {
            return _db.Load<BO.p28Contact>(GetSQL1(" WHERE a.p28ID=@pid"), new { pid = intPID });            
        }
        
        public BO.p28Contact LoadByCode(string strCode, int pid_exclude)
        {
            return _db.Load<BO.p28Contact>(GetSQL1(" WHERE a.p28Code LIKE @code AND a.p28ID<>@pid_exclude",0,false,_mother.CurrentUser.IsHostingModeTotalCloud), new { code = strCode, pid_exclude = pid_exclude });
        }
        public BO.p28Contact LoadByICO(string strICO, int pid_exclude)
        {
            return _db.Load<BO.p28Contact>(GetSQL1(" WHERE a.p28RegID = @ico AND a.p28ID<>@pid_exclude",0,false,_mother.CurrentUser.IsHostingModeTotalCloud), new { ico = strICO, pid_exclude = pid_exclude });
        }
        public BO.p28Contact LoadByDIC(string strDIC, int pid_exclude)
        {
            return _db.Load<BO.p28Contact>(GetSQL1(" WHERE (a.p28VatID = @dic OR a.p28ICDPH_SK = @dic) AND a.p28ID<>@pid_exclude AND a.p28VatID NOT LIKE 'CZ699%'", 0,false,_mother.CurrentUser.IsHostingModeTotalCloud), new { dic = strDIC, pid_exclude = pid_exclude });
        }
        public BO.p28Contact LoadByGuid(string guid)
        {
            return _db.Load<BO.p28Contact>(GetSQL1(" WHERE a.p28Guid=@guid"), new { guid = guid });
        }

        public BO.p28Contact LoadByBankAccount(string bankovni_ucet)
        {
            return _db.Load<BO.p28Contact>(GetSQL1(" WHERE a.p28BankAccount LIKE @s OR a.p28BankAccount+'/'+a.p28BankCode LIKE @s"), new { s = bankovni_ucet });
        }

        public string LoadBillingMemo(int p28id)
        {
            return _db.Load<BO.GetString>("select p28BillingMemo as Value FROM p28Contact WHERE p28ID=@pid", new { pid = p28id }).Value;            
        }
        

        public IEnumerable<BO.p28Contact>GetList(BO.myQueryP28 mq,bool ischangelog=false)
        {
            
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(null,mq.TopRecordsOnly,ischangelog), mq, _mother.CurrentUser);
            return _db.GetList<BO.p28Contact>(fq.FinalSql,fq.Parameters);
        }

     

        public int Save(BO.p28Contact rec, List<BO.o32Contact_Medium> lisO32, List<BO.FreeFieldInput> lisFFI,string tempguid, List<BO.x69EntityRole_Assign> lisX69, List<BO.p30ContactPerson> lisP30,string billingmemo=null)
        {
            
            if (!ValidateBeforeSave(rec, lisO32,lisP30))
            {
                return 0;
            }
            if (!_mother.x67EntityRoleBL.Validate_lisX69_BeforeAssign(lisX69))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope()) //ukládání podléhá transakci
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
                p.AddInt("p29ID", rec.p29ID, true);
                p.AddInt("p92ID", rec.p92ID, true);
                p.AddInt("p63ID", rec.p63ID, true);
                p.AddInt("j61ID_Invoice", rec.j61ID_Invoice, true);
                
                p.AddInt("p28ParentID", rec.p28ParentID, true);
                p.AddInt("p51ID_Billing", rec.p51ID_Billing, true);
                p.AddInt("p51ID_Internal", rec.p51ID_Internal, true);

                p.AddString("p28FirstName", rec.p28FirstName);
                p.AddString("p28LastName", rec.p28LastName);
                p.AddString("p28TitleBeforeName", rec.p28TitleBeforeName);
                p.AddString("p28TitleAfterName", rec.p28TitleAfterName);

                p.AddBool("p28IsCompany", rec.p28IsCompany);
                p.AddString("p28CompanyName", rec.p28CompanyName);

                p.AddString("p28ShortName", rec.p28ShortName);

                p.AddInt("p28BillingLangIndex", rec.p28BillingLangIndex);
                if (rec.p28Code == null) rec.p28Code = $"TEMP{BO.Code.Bas.GetGuid().Substring(0,15)}";
                p.AddString("p28Code", rec.p28Code);           


                p.AddString("p28JobTitle", rec.p28JobTitle);
                p.AddString("p28Salutation", rec.p28Salutation);
                p.AddString("p28RegID", rec.p28RegID);
                p.AddString("p28VatID", rec.p28VatID);
                p.AddString("p28ICDPH_SK", rec.p28ICDPH_SK);

                p.AddString("p28BankAccount", rec.p28BankAccount);
                p.AddString("p28BankCode", rec.p28BankCode);

                if (rec.p28Guid == Guid.Empty)
                {
                    rec.p28Guid = Guid.NewGuid();
                }
                
                p.AddString("p28Guid", rec.p28Guid.ToString());

                p.AddString("p28CountryCode", rec.p28CountryCode);

                p.AddString("p28City1", rec.p28City1);
                p.AddString("p28Street1", rec.p28Street1);
                p.AddString("p28PostCode1", rec.p28PostCode1);
                p.AddString("p28Country1", rec.p28Country1);
                p.AddString("p28BeforeAddress1", rec.p28BeforeAddress1);

                p.AddString("p28City2", rec.p28City2);
                p.AddString("p28Street2", rec.p28Street2);
                p.AddString("p28PostCode2", rec.p28PostCode2);
                p.AddString("p28Country2", rec.p28Country2);

                p.AddString("p28InvoiceDefaultText1", rec.p28InvoiceDefaultText1);
                p.AddString("p28InvoiceDefaultText2", rec.p28InvoiceDefaultText2);
                p.AddInt("p28InvoiceMaturityDays", rec.p28InvoiceMaturityDays);
             
                p.AddDouble("p28Round2Minutes", rec.p28Round2Minutes);

                p.AddString("p28ExternalCode", rec.p28ExternalCode);
                p.AddInt("p28BitStream", rec.p28BitStream);
                p.AddEnumInt("p28BillingFlag", rec.p28BillingFlag,true);
                p.AddString("p28VatCodePohoda", rec.p28VatCodePohoda);
                p.AddString("p28BillingMemo200", rec.p28BillingMemo200);
                if (rec.p28BillingMemo200 == null || billingmemo != null)
                {
                    p.AddString("p28BillingMemo", billingmemo);
                }

                int intPID = _db.SaveRecord("p28Contact", p, rec);
                if (intPID > 0)
                {
                    if (lisO32 != null) Handle_SaveO32(intPID, lisO32);
                    if (lisP30 != null) Handle_SaveP30(intPID, lisP30);

                    if (!string.IsNullOrEmpty(tempguid))
                    {
                        
                        _db.RunSql("INSERT INTO p85TempBox(p85GUID,p85Prefix,p85DataPID) VALUES(@guid,'p28',@pid)", new { guid = tempguid, pid = intPID });
                    }

                    _db.RunSql("exec dbo.p28_aftersave @p28id,@j02id_sys", new { p28id = intPID, j02id_sys = _mother.CurrentUser.pid });
                    
                    if (!DL.BAS.SaveFreeFields(_db, intPID, lisFFI))
                    {
                        return 0;
                    }

                    if (lisX69 != null && !DL.BAS.SaveX69(_db, "p28", intPID, lisX69))
                    {
                        this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                        return 0;
                    }
                    
                    var recP29 = _mother.p29ContactTypeBL.Load(rec.p29ID);
                    if (recP29.b01ID > 0 && rec.b02ID == 0)
                    {                        
                        _mother.WorkflowBL.InitWorkflowStatus(intPID, "p28");   //nahodit úvodní workflow stav záznamu
                    }

                    sc.Complete();
                    

                    return intPID;
                }

                return 0;
                
            }
            
            
        }

        private void Handle_SaveO32(int intP28ID,List<BO.o32Contact_Medium> lisO32) //uložení kontaktních médií
        {
            if (lisO32 == null) return;
            foreach (var med in lisO32)
            {
                if (med.IsSetAsDeleted)
                {
                    if (med.pid > 0)
                    {
                        _db.RunSql("DELETE FROM o32Contact_Medium WHERE o32ID=@pid", new { pid = med.pid });
                    }
                }
                else
                {
                    var recO32 = new BO.o32Contact_Medium();
                    if (med.pid > 0)
                    {
                        recO32 = _db.Load<BO.o32Contact_Medium>("select a.*," + _db.GetSQL1_Ocas("o32", false, false) + " from o32Contact_Medium a WHERE a.o32ID=@pid", new { pid = med.pid });
                    }
                    recO32.o32Value = med.o32Value; recO32.o32Description = med.o32Description; recO32.o32IsDefaultInInvoice = med.o32IsDefaultInInvoice; recO32.o33ID = med.o33ID;recO32.o32Person = med.o32Person;
                    var p = new DL.Params4Dapper();
                    p.AddInt("pid", recO32.pid);
                    p.AddInt("p28ID", intP28ID, true);
                    p.AddEnumInt("o33ID", recO32.o33ID, true);
                    p.AddString("o32Value", recO32.o32Value);
                    p.AddString("o32Description", recO32.o32Description);
                    p.AddString("o32Person", recO32.o32Person);
                    p.AddBool("o32IsDefaultInInvoice", recO32.o32IsDefaultInInvoice);
                    _db.SaveRecord("o32Contact_Medium", p, recO32, false, true);
                    
                }
            }
        }
        private void Handle_SaveP30(int intP28ID,List<BO.p30ContactPerson> lisP30)  //uložení kontaktních osob
        {
            if (lisP30 == null) return;

            foreach (var c in lisP30)
            {
                if (c.IsSetAsDeleted)
                {
                    if (c.pid > 0)
                    {
                        _db.RunSql("DELETE FROM p30ContactPerson WHERE p30ID=@pid", new { pid = c.pid });
                    }
                }
                else
                {
                    var recP30 = new BO.p30ContactPerson();
                    if (c.pid > 0)
                    {
                        recP30 = _db.Load<BO.p30ContactPerson>("select a.*," + _db.GetSQL1_Ocas("p30", false, false) + " from p30ContactPerson a WHERE a.p30ID=@pid", new { pid = c.pid });
                    }
                    var p = new DL.Params4Dapper();
                    p.AddInt("pid", recP30.pid);
                    p.AddInt("p28ID", intP28ID, true);
                    p.AddString("p30Name", c.p30Name);                    
                    p.AddInt("p28ID_Person", c.p28ID_Person);

                    _db.SaveRecord("p30ContactPerson", p, recP30, false, true);
                }
            }
        }
        public bool ValidateBeforeSave(BO.p28Contact rec, List<BO.o32Contact_Medium> lisO32, List<BO.p30ContactPerson> lisP30)
        {
            if (rec.j02ID_Owner == 0)
            {
                this.AddMessage("Chybí vyplnit [Vlastník záznamu]."); return false;
            }
            if (rec.p29ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Typ kontaktu]."); return false;
            }
            var recP29 = _mother.p29ContactTypeBL.Load(rec.p29ID);
            if (recP29.p29ScopeFlag == BO.p29ScopeFlagENUM.ContactPerson)
            {
                if (rec.p28IsCompany)
                {
                    this.AddMessage("Kontaktní osoba musí být nastavena jako fyzická osoba."); return false;
                }
            }

            if (string.IsNullOrEmpty(rec.p28CountryCode))
            {
                this.AddMessage("Chybí vyplnit [ISO kód státu]."); return false;
            }
            if (!rec.p28IsCompany && string.IsNullOrEmpty(rec.p28FirstName))
            {
                this.AddMessage("Chybí vyplnit [Jméno]."); return false;
            }
            if (!rec.p28IsCompany && string.IsNullOrEmpty(rec.p28LastName))
            {
                this.AddMessage("Chybí vyplnit [Příjmení]."); return false;
            }
            if (rec.p28IsCompany && string.IsNullOrEmpty(rec.p28CompanyName))
            {
                this.AddMessage("Chybí vyplnit [Název společnosti]."); return false;
            }            

            if (lisO32 != null && lisO32.Where(p => p.IsSetAsDeleted == false && string.IsNullOrEmpty(p.o32Value)).Count() > 0)
            {
                this.AddMessage("Kontaktní médium musí mít vyplněnou adresu/číslo."); return false;
            }
            if (lisO32 != null && lisO32.Where(p => !p.IsSetAsDeleted && (p.o33ID==BO.o33FlagEnum.Email || p.o33ID == BO.o33FlagEnum.EmailCC || p.o33ID == BO.o33FlagEnum.EmailBCC) && !BO.Code.Bas.IsValidEmail(p.o32Value)).Count() > 0)
            {
                this.AddMessageTranslated("E-mail adresa není platná."); return false;
            }
            if (lisP30 != null && lisP30.Any(p => p.IsSetAsDeleted == false && p.p28ID_Person==0))
            {
                this.AddMessage("V kontaktních osobách chybí vazba na záznam osoby."); return false;
            }
            if (lisP30 != null && lisP30.Where(p=>p.IsSetAsDeleted==false).GroupBy(p=>p.p28ID_Person).Any(p=>p.Count()>1))
            {
                this.AddMessage("V kontaktních osobách je duplicitní vazba na záznam osoby."); return false;
            }

            if (rec.UserInsert !="portal" && _mother.CBL.TestIfAllowCreateRecord("p28", rec.p29ID).Flag == BO.ResultEnum.Failed)
            {
                return this.FalsehMessage("Nemáte oprávnění zakládat kontakty tohoto typu.");
            }

            if (rec.p28Code != null && LoadByCode(rec.p28Code, rec.pid) != null)
            {
                this.AddMessageTranslated(string.Format(_mother.tra("Kód [{0}] již je obsazen jiným kontaktem."), rec.p28Code));
                return false;
            }

            if (!_mother.Lic.x01IsAllowDuplicity_RegID && !string.IsNullOrEmpty(rec.p28RegID))
            {
                var rt = LoadByICO(rec.p28RegID, rec.pid);
                if (rt != null)
                {
                    string s = $"V databázi již existuje kontakt s IČO={rec.p28RegID} ({rt.p28Name}).<hr> Kontrolu lze vypnout v administraci.";                    
                    if (rt.isclosed)
                    {
                        s += "<hr>Pozor, tento záznam kontaktu byl přesunutý do archivu.";
                    }
                    this.AddMessageTranslated(s);
                    return false;
                }
                
                
            }
            if (!_mother.Lic.x01IsAllowDuplicity_VatID && !string.IsNullOrEmpty(rec.p28VatID))
            {
                var rt = LoadByDIC(rec.p28VatID, rec.pid);
                if (rt != null)
                {
                    string s = $"V databázi již existuje kontakt s DIČ={rec.p28VatID} ({rt.p28Name}).<hr> Kontrolu lze vypnout v administraci.";
                    if (rt.isclosed)
                    {
                        s += "<hr>Pozor, tento záznam kontaktu byl přesunutý do archivu.";
                    }
                    this.AddMessageTranslated(s);
                    return false;
                }
                
            }

            return true;
        }

        public BO.p28ContactSum LoadSumRow(int pid)
        {
            if (_mother.CurrentUser.j02PerformanceFlag == 1)
            {
                return new BO.p28ContactSum() { tabname = _mother.tra("Záznam" )};
            }
            else
            {
                return _db.Load<BO.p28ContactSum>("EXEC dbo.p28_inhale_sumrow @j02id_sys,@pid", new { j02id_sys = _mother.CurrentUser.pid, pid = pid });
            }
                
        }

        public BO.p28RecDisposition InhaleRecDisposition(int pid,BO.p28Contact rec=null)
        {
            var c = new BO.p28RecDisposition() { a55ID= _mother.CurrentUser.a55ID_p28 };
          
            if (_mother.CurrentUser.IsAdmin || _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p28_Owner))
            {
                c.OwnerAccess = true; c.ReadAccess = true;  //admin nebo vlastník všech kontaktů
                c.Portal = true;
                return c;
            }
            if (rec == null) rec = Load(pid);
            if (rec == null)
            {
                return null;
            }
            if (rec.j02ID_Owner == _mother.CurrentUser.pid)
            {
                c.OwnerAccess = true; c.ReadAccess = true;  //vlastník záznamu
                return c;
            }

            var lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneContact(rec,true);
            foreach(var role in lisX69)
            {
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p28_Owner))
                {
                    c.OwnerAccess = true; c.ReadAccess = true;  //vlastník
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                    return c;
                }
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p28_Reader))
                {
                    c.ReadAccess = true;
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                }
                c.Portal = BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p28_Portal);
                
            }
            
            if (!c.ReadAccess)
            {
                c.ReadAccess = _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p28_Reader);    //čtenář všech kontaktů
            }
                        
            return c;
        }

        public IEnumerable<BO.o32Contact_Medium> GetList_o32(int p28id,int p24id,int p30id)
        {
            if (p28id > 0)
            {
                return _db.GetList<BO.o32Contact_Medium>("select a.*," + _db.GetSQL1_Ocas("o32", false, false) + ", o33.o33Name FROM o32Contact_Medium a INNER JOIN o33MediumType o33 ON a.o33ID=o33.o33ID WHERE a.p28ID=@p28id", new { p28id = p28id });
            }
            if (p24id > 0)
            {
                return _db.GetList<BO.o32Contact_Medium>("select a.*," + _db.GetSQL1_Ocas("o32", false, false) + ", o33.o33Name FROM o32Contact_Medium a INNER JOIN o33MediumType o33 ON a.o33ID=o33.o33ID WHERE a.p28ID IN (select p28ID FROM p25ContactGroupBinding WHERE p24ID=@p24id)", new { p24id = p24id });
            }
            if (p30id > 0)
            {
                return _db.GetList<BO.o32Contact_Medium>("select a.*," + _db.GetSQL1_Ocas("o32", false, false) + ", o33.o33Name FROM o32Contact_Medium a INNER JOIN o33MediumType o33 ON a.o33ID=o33.o33ID WHERE a.p28ID IN (select p28ID FROM p30ContactPerson WHERE p30ID=@p30id)", new { p30id = p30id });
            }
            return null;
        }
        public IEnumerable<BO.p26ProjectContact> GetList_p26(int p28id)
        {
            string s = "select a.*," + _db.GetSQL1_Ocas("p26", false, false) + ", p41.p41Name,p28.p28Name FROM p26ProjectContact a INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID INNER JOIN p28Contact p28 ON a.p28ID=p28.p28ID WHERE a.p28ID=@p28id";
           
            return _db.GetList<BO.p26ProjectContact>(s, new { p28id = p28id });
        }
        public IEnumerable<BO.p30ContactPerson> GetList_p30(int p28id)
        {
            string s = "select a.*," + _db.GetSQL1_Ocas("p30", false, false) + ",per.p28Name as Person, per.p28Name+isnull(' ('+mail.o32Value+')','') as PersonWithInvoiceEmail,mot.p28Name as Mother,mail.o32Value as PersonInvoiceEmail FROM p30ContactPerson a INNER JOIN p28Contact per ON a.p28ID_Person=per.p28ID INNER JOIN p28Contact mot ON a.p28ID=mot.p28ID LEFT OUTER JOIN o32Contact_Medium mail ON per.p28ID=mail.p28ID AND mail.o32IsDefaultInInvoice=1 WHERE a.p28ID=@p28id";
           
            return _db.GetList<BO.p30ContactPerson>(s, new { p28id = p28id });
        }
        public IEnumerable<BO.p30ContactPerson> GetList_p30(List<int> p28ids)
        {
            string s = $"select a.*,{_db.GetSQL1_Ocas("p30", false, false)},per.p28Name as Person, per.p28Name+isnull(' ('+mail.o32Value+')','') as PersonWithInvoiceEmail,mot.p28Name as Mother,mail.o32Value as PersonInvoiceEmail FROM p30ContactPerson a INNER JOIN p28Contact per ON a.p28ID_Person=per.p28ID INNER JOIN p28Contact mot ON a.p28ID=mot.p28ID LEFT OUTER JOIN o32Contact_Medium mail ON per.p28ID=mail.p28ID AND mail.o32IsDefaultInInvoice=1 WHERE a.p28ID IN ({string.Join(",",p28ids)})";

            return _db.GetList<BO.p30ContactPerson>(s);
        }
        public IEnumerable<BO.p30ContactPerson> GetList_p30_mother(int p28id)
        {
            string s = "select a.*," + _db.GetSQL1_Ocas("p30", false, false) + ", per.p28Name as Person,mot.p28Name as Mother FROM p30ContactPerson a INNER JOIN p28Contact per ON a.p28ID_Person=per.p28ID INNER JOIN p28Contact mot ON a.p28ID=mot.p28ID WHERE a.p28ID_Person=@p28id";

            return _db.GetList<BO.p30ContactPerson>(s, new { p28id = p28id });
        }
        public IEnumerable<BO.p30ContactPerson> GetList_p30_p31entry(int p28id)
        {
            string s = "select a.*," + _db.GetSQL1_Ocas("p30", false, false) + ",per.p28Name as Person FROM p30ContactPerson a INNER JOIN p28Contact per ON a.p28ID_Person=per.p28ID WHERE a.p28ID=@p28id ORDER BY per.p28Name";

            return _db.GetList<BO.p30ContactPerson>(s, new { p28id = p28id });
        }

        public string GetBillingFlagHtml(BO.p28Contact rec) //vrací vlajku fakturačního jazyka klienta
        {
            if (rec == null || rec.p28BillingLangIndex==0) return null;

            string s = null;
            if (rec.p28BillingLangIndex == 1) s = _mother.Lic.x01BillingFlag1;
            if (rec.p28BillingLangIndex == 2) s = _mother.Lic.x01BillingFlag2;
            if (rec.p28BillingLangIndex == 3) s = _mother.Lic.x01BillingFlag3;
            if (rec.p28BillingLangIndex == 4) s = _mother.Lic.x01BillingFlag4;           

            if (s != null && File.Exists(_mother.App.WwwRootFolder + "\\images\\flags\\" + s))
            {
                return $"<strong>{_mother.Lic.GetBillingLang(rec.p28BillingLangIndex)}</strong><img src='/images/flags/{s}'/>";
            }
            else
            {
                return $"<strong>{_mother.Lic.GetBillingLang(rec.p28BillingLangIndex)}</strong>";
            }

            
        }

        public IEnumerable<BO.p28MyTop10> GetList_MyTop10(int j02id, int toprecs)
        {

            return _db.GetList<BO.p28MyTop10>($"SELECT TOP {toprecs} p41.p28ID_Client as p28ID,MAX(a.p31DateInsert) as LastDate,MAX(p28.p28Name) as Client FROM p31Worksheet a INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID INNER JOIN p28Contact p28 ON p41.p28ID_Client=p28.p28ID WHERE a.j02ID=@j02id AND a.p31Date>DATEADD(MONTH,-12,GETDATE()) GROUP BY p41.p28ID_Client ORDER BY MAX(a.p31DateInsert) DESC", new { j02id = j02id });
        }

        

        public BO.p28Contact LoadTreeTopRec(BO.p28Contact rec)
        {
            if (rec.p28ParentID == 0)
            {
                return rec;
            }

            rec = _mother.p28ContactBL.Load(rec.p28ParentID);
            if (rec.p28ParentID == 0)
            {
                return rec;
            }
            rec = _mother.p28ContactBL.Load(rec.p28ParentID);
            if (rec.p28ParentID == 0)
            {
                return rec;
            }
            rec = _mother.p28ContactBL.Load(rec.p28ParentID);
            if (rec.p28ParentID == 0)
            {
                return rec;
            }

            return rec;

        }


    }

    
}
