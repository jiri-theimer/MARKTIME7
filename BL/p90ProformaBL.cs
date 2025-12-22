

namespace BL
{
    public interface Ip90ProformaBL
    {
        public BO.p90Proforma Load(int pid);
        public BO.p90Proforma LoadByCode(string code);
        public BO.p90Proforma LoadByNumericCode(string numeric);
        public BO.p90Proforma LoadMyLastCreated();
        public IEnumerable<BO.p90Proforma> GetList(BO.myQueryP90 mq);
        public int Save(BO.p90Proforma rec, List<BO.FreeFieldInput> lisFFI, List<BO.x69EntityRole_Assign> lisX69);
        public BO.p90RecDisposition InhaleRecDisposition(int pid,BO.p90Proforma rec=null);
        public BO.p90ProformaSum LoadSumRow(int pid);
        public BO.Integrace.InputZaloha CreateIntegraceRecord(BO.p90Proforma c);

    }
    class p90ProformaBL : BaseBL, Ip90ProformaBL
    {
        public p90ProformaBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null,int toprec=0,bool istestcloud=false)
        {
            if (toprec == 0)
            {
                sb("SELECT a.*");
            }
            else
            {
                sb($"SELECT TOP {toprec} a.*");
            }            
            //sb(",p82.p82ID as p82ID_First,j27.j27Code,p89.p89Name,p28.p28Name");
            sb(",j27.j27Code,p89x.p89Name,p28.p28Name,p28.p28RegID,p28.p28VatID");
            sb(",j02owner.j02LastName+' '+j02owner.j02FirstName as Owner");
            sb(",p89x.x38ID,p89x.x31ID,p89x.x31ID_Payment,");
            sb(_db.GetSQL1_Ocas("p90"));
            sb(" FROM p90Proforma a");
            sb(" INNER JOIN p89ProformaType p89x ON a.p89ID=p89x.p89ID");
            sb(" LEFT OUTER JOIN p28Contact p28 ON a.p28ID=p28.p28ID");
            sb(" LEFT OUTER JOIN j27Currency j27 ON a.j27ID=j27.j27ID");
            sb(" LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");
            
            if (istestcloud)
            {
                sb(this.AppendCloudQuery(strAppend, "p89x.x01ID"));
            }
            else
            {
                sb(strAppend);
            }
            return sbret();
        }
        public BO.p90Proforma Load(int pid)
        {
            return _db.Load<BO.p90Proforma>(GetSQL1(" WHERE a.p90ID=@pid"), new { pid = pid });
        }
        public BO.p90Proforma LoadByCode(string code)
        {
            return _db.Load<BO.p90Proforma>(GetSQL1(" WHERE a.p90Code LIKE @code",0,_mother.CurrentUser.IsHostingModeTotalCloud), new { code = code });
        }
        public BO.p90Proforma LoadByNumericCode(string numeric)
        {
            return _db.Load<BO.p90Proforma>(GetSQL1(" WHERE convert(bigint,a.p90CodeNumeric) = convert(bigint,dbo.get_only_numerics(@code))",0,_mother.CurrentUser.IsHostingModeTotalCloud), new { code = numeric });

        }
        public BO.p90Proforma LoadMyLastCreated()
        {
            return _db.Load<BO.p90Proforma>(GetSQL1(" WHERE a.j02ID_Owner=@j02id_owner ORDER BY a.p90ID DESC",1), new { j02id_owner = _mother.CurrentUser.pid });
        }

        public BO.p90RecDisposition InhaleRecDisposition(int pid,BO.p90Proforma rec=null)
        {
            var c = new BO.p90RecDisposition();

            if (_mother.CurrentUser.IsAdmin || _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p90_Owner))
            {
                c.OwnerAccess = true; c.ReadAccess = true;
                return c;
            }
            if (rec == null) rec = Load(pid);
            if (rec.j02ID_Owner == _mother.CurrentUser.pid)
            {
                c.OwnerAccess = true; c.ReadAccess = true;
                return c;
            }
            var lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneProforma(rec,true);
            foreach (var role in lisX69)
            {
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p90_Owner))
                {
                    c.OwnerAccess = true; c.ReadAccess = true;  //vlastník                    
                    return c;
                }
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p90_Reader))
                {
                    c.ReadAccess = true;                    
                }
            }

           if (!c.ReadAccess) c.ReadAccess = _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p90_Reader);

            return c;
        }

        public IEnumerable<BO.p90Proforma> GetList(BO.myQueryP90 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p90Proforma>(fq.FinalSql, fq.Parameters);
        }

        

        public int Save(BO.p90Proforma rec, List<BO.FreeFieldInput> lisFFI, List<BO.x69EntityRole_Assign> lisX69)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddBool("p90IsDraft", rec.p90IsDraft);

                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
                p.AddInt("p89ID", rec.p89ID, true);
                p.AddInt("p28ID", rec.p28ID, true);
                p.AddInt("j27ID", rec.j27ID, true);
                p.AddInt("j19ID", rec.j19ID, true);
                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);

                p.AddString("p90Code", rec.p90Code);
                p.AddString("p90Text1", rec.p90Text1);
                p.AddString("p90Text2", rec.p90Text2);
                p.AddString("p90TextDPP", rec.p90TextDPP);

                if (rec.p90Guid == Guid.Empty)
                {
                    rec.p90Guid = Guid.NewGuid();
                }
                p.AddString("p90Guid", rec.p90Guid.ToString());

                p.AddDouble("p90Amount_WithoutVat", rec.p90Amount_WithoutVat);
                p.AddDouble("p90Amount_Vat", rec.p90Amount_Vat);
                p.AddDouble("p90VatRate", rec.p90VatRate);
                p.AddDouble("p90Amount", rec.p90Amount);

                p.AddDateTime("p90Date", rec.p90Date);
                p.AddDateTime("p90DateMaturity", rec.p90DateMaturity);
                p.AddInt("p90BitStream", rec.p90BitStream);

               

                int intPID = _db.SaveRecord("p90Proforma", p, rec);
                if (intPID > 0)
                {
                    if (!DL.BAS.SaveFreeFields(_db, intPID, lisFFI))
                    {
                        return 0;
                    }
                    if (lisX69 != null && !DL.BAS.SaveX69(_db, "p90", intPID, lisX69))
                    {
                        this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                        return 0;
                    }


                    if (_db.RunSql("exec dbo.p90_aftersave @p90id,@j02id_sys", new { p90id = intPID, j02id_sys = _mother.CurrentUser.pid }))
                    {
                        sc.Complete();
                        return intPID;
                    }

                    

                }

                return intPID;
                
            }
                

        }

        private bool ValidateBeforeSave(BO.p90Proforma rec)
        {
            if (rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;
           
            if (rec.p90Amount > 0 && rec.p90Amount_WithoutVat>0)
            {
                if (Math.Abs((rec.p90Amount_WithoutVat + rec.p90Amount_Vat) - rec.p90Amount) > 0.01)
                {
                    this.AddMessage("Částka bez DPH + částka DPH musí souhlasit s celkovou částkou.");return false;
                }
            }
            if (rec.p90Amount>0 && rec.p90VatRate>0 && rec.p90Amount_WithoutVat == 0 && rec.p90Amount_Vat==0)
            {
                rec.p90Amount_WithoutVat = rec.p90Amount / (1 + rec.p90VatRate / 100);
                rec.p90Amount_Vat = rec.p90Amount - rec.p90Amount_WithoutVat;
            }
            if (rec.p90Amount == 0 && rec.p90VatRate > 0 && rec.p90Amount_WithoutVat > 0 && rec.p90Amount_Vat == 0)
            {                
                rec.p90Amount_Vat = rec.p90Amount_WithoutVat * rec.p90VatRate / 100;
                rec.p90Amount = rec.p90Amount_WithoutVat + rec.p90Amount_Vat;
            }
            if (rec.j27ID==0)
            {
                this.AddMessage("Chybí vyplnit [Měna]."); return false;
            }
            if (rec.p89ID==0)
            {
                this.AddMessage("Chybí vyplnit [Typ zálohy]."); return false;
            }
            if (rec.p28ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Klient]."); return false;
            }
            if (_mother.CBL.TestIfAllowCreateRecord("p90", rec.p89ID).Flag == BO.ResultEnum.Failed)
            {
                return this.FalsehMessage("Nemáte oprávnění zakládat zálohy tohoto typu.");
            }

            return true;
        }

        public BO.p90ProformaSum LoadSumRow(int pid)
        {
            return _db.Load<BO.p90ProformaSum>("EXEC dbo.p90_inhale_sumrow @j02id_sys,@pid", new { j02id_sys = _mother.CurrentUser.pid, pid = pid });
        }


        public BO.Integrace.InputZaloha CreateIntegraceRecord(BO.p90Proforma c)
        {
            var rec = new BO.Integrace.InputZaloha() { pid = c.pid, p90Code = c.p90Code, p90Guid = c.p90Guid, j27ID = c.j27ID, p90Date = c.p90Date, p90DateMaturity = c.p90DateMaturity,p90DateBilled=c.p90DateBilled };
            rec.p89ID = c.p89ID;
            rec.p90Text1 = c.p90Text1;
            rec.p90Text2 = c.p90Text2;
            rec.p90VatRate = c.p90VatRate;            
            rec.x15ID = _mother.p53VatRateBL.NajdiX15ID(rec.p90Date, rec.p90VatRate,rec.j27ID);
            rec.j27ID_Domestic = _mother.Lic.j27ID; rec.j27Code_Domestic = _mother.FBL.LoadCurrencyByID(_mother.Lic.j27ID).j27Code;
            rec.j27ID = c.j27ID; rec.j27Code = c.j27Code;

            rec.p28ID = c.p28ID;
            var recP28 = _mother.p28ContactBL.Load(rec.p28ID);
            rec.p28Name = c.p28Name; rec.p28RegID = c.p28RegID; rec.p28VatID = c.p28VatID; rec.p28City1 = recP28.p28City1; rec.p28Street1 = recP28.p28Street1;rec.p28PostCode1 = recP28.p28PostCode1; rec.p28Country1 = recP28.p28Country1;
            
            rec.p90Amount = c.p90Amount; rec.p90Amount_Billed = c.p90Amount_Billed; rec.p90Amount_Vat = c.p90Amount_Vat; rec.p90Amount_WithoutVat = c.p90Amount_WithoutVat;
           
            var recP86 = _mother.p86BankAccountBL.LoadProformaAccount(c.pid);
            if (recP86 != null)
            {
                rec.p86Account = recP86.p86Account; rec.p86Code = recP86.p86Code; rec.p86BankName = recP86.p86BankName; rec.p86IBAN = recP86.p86IBAN; rec.p86SWIFT = recP86.p86SWIFT;
            }
            

            var recP89 = _mother.p89ProformaTypeBL.Load(c.p89ID);
            var recP93 = _mother.p93InvoiceHeaderBL.Load(recP89.p93ID);
            rec.p93Zip = recP93.p93Zip;
            if (recP93.p93CountryCode == null) recP93.p93CountryCode = "CZ";
            if (recP93.p93Country == null) recP93.p93CountryCode = "Česká republika";
            rec.p93CountryCode = recP93.p93CountryCode; rec.p93Company = recP93.p93Company; rec.p93VatID = recP93.p93VatID; rec.p93RegID = recP93.p93RegID; rec.p93City = recP93.p93City; rec.p93Street = recP93.p93Street; rec.p93Zip = recP93.p93Zip; rec.p93Country = recP93.p93Country;
            
           

            return rec;
        }

        

    }
}
