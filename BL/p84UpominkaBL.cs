

namespace BL
{
    public interface Ip84UpominkaBL
    {
        public BO.p84Upominka Load(int pid);        
        public int Save(BO.p84Upominka rec);
        public IEnumerable<BO.p84Upominka> GetList(BO.myQueryP84 mq);
        public int TryCreate(int p91id, int p83id=0);
        public int GetFirstFreeUpominkaIndex(BO.p91Invoice recP91); //vrací stupeň upomínky, která čeká na vygenerování
        public int NajdiVychoziJ61ID(BO.p83UpominkaType recP83, BO.p84Upominka recP84);

    }
    class p84UpominkaBL : BaseBL, Ip84UpominkaBL
    {
        public p84UpominkaBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null, int toprec = 0, bool istestcloud = false)
        {
            sb("SELECT a.*,p83x.p83Name,p91.p91Code,j02owner.j02LastName+' '+j02owner.j02FirstName as Owner,p83x.x31ID_Index1,p83x.x31ID_Index2,p83x.x31ID_Index3,p83x.j61ID_Index1,p83x.j61ID_Index2,p83x.j61ID_Index3,p83x.p83Days_Index1,p83x.p83Days_Index2,p83x.p83Days_Index3,p91.p28ID,p91.p92ID,");
            sb(_db.GetSQL1_Ocas("p84"));
            sb(" FROM p84Upominka a INNER JOIN p83UpominkaType p83x ON a.p83ID=p83x.p83ID LEFT OUTER JOIN p91Invoice p91 ON a.p91ID=p91.p91ID LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");
            if (istestcloud)
            {
                sb(this.AppendCloudQuery(strAppend, "p83x.x01ID"));
            }
            else
            {
                sb(strAppend);
            }
            return sbret();
        }
        public BO.p84Upominka Load(int pid)
        {
            return _db.Load<BO.p84Upominka>(GetSQL1(" WHERE a.p84ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p84Upominka> GetList(BO.myQueryP84 mq)
        {                        
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(null,mq.TopRecordsOnly), mq, _mother.CurrentUser);
            return _db.GetList<BO.p84Upominka>(fq.FinalSql, fq.Parameters);
        }

        public int GetFirstFreeUpominkaIndex(BO.p91Invoice recP91)
        {
            if (recP91.p91Amount_Debt < 1)
            {
                return 0;   //uhrazená faktura
            }
            var lis = GetList(new BO.myQueryP84() { p91id = recP91.pid });
            if (lis.Count() == 0)
            {
                return 1;
            }
            int intMaxIndex = lis.Max(p => p.p84Index);
            return intMaxIndex + 1;
        }

        public int TryCreate(int p91id,int p83id=0)
        {
            if (p91id==0)
            {
                return 0;
            }
            var rec = new BO.p84Upominka() { p91ID = p91id, p83ID = p83id,j02ID_Owner=_mother.CurrentUser.pid,p84Date=DateTime.Today };
            var recP91 = _mother.p91InvoiceBL.Load(p91id);
            if (recP91.p91Amount_Debt < 1)
            {
                this.AddMessageTranslated("Faktura nemá dluh po splatnosti.");
                return 0;   //není dluh
            }
            if (rec.p83ID == 0)
            {
                var recP92 = _mother.p92InvoiceTypeBL.Load(recP91.p92ID);
                rec.p83ID = recP92.p83ID;
            }
            if (rec.p83ID == 0)
            {
                var lisP83 = _mother.p83UpominkaTypeBL.GetList(new BO.myQuery("p83"));
                if (lisP83.Count() == 0)
                {
                    this.AddMessageTranslated("V administraci chybí typy upomínek.");
                    return 0;   //není naplněn číselník typů upomínek
                }
                rec.p83ID = lisP83.First().pid;
            }
            var recP83 = _mother.p83UpominkaTypeBL.Load(rec.p83ID);            
            var intStupenCeka = GetFirstFreeUpominkaIndex(recP91);           
            
            if (intStupenCeka == 0)
            {                
                TimeSpan span = DateTime.Today - recP91.p91DateMaturity;
                if (span.Days < recP83.p83Days_Index1)
                {
                    this.AddMessageTranslated($"První upomínku je povoleno generovat až po uplynutí {recP83.p83Days_Index1} dní po datu splatnosti faktury ({recP91.p91DateMaturity.ToString("dd.MM.yyyy")}).<hr>Toto můžete změnit v nastavení typu upomínky.");
                    return 0;   //ještě nenastal čas na první upomínku
                }
                               
            }
            else
            {
                                
                var lis = GetList(new BO.myQueryP84() { p91id = recP91.pid });
                BO.p84Upominka recPre = null;
                
                switch (intStupenCeka)
                {
                    case 1:
                        rec.p84Index = intStupenCeka;
                        rec.p84Name = recP83.p83Name_Index1;
                        rec.p84TextA = recP83.p83TextA_Index1;
                        rec.p84TextB = recP83.p83TextB_Index1;
                        break;
                    case 2:
                        recPre = lis.First(p => p.p84Index == 1);                        
                        if ((DateTime.Today - recPre.p84Date).Days < recP83.p83Days_Index2)
                        {
                            this.AddMessageTranslated($"Druhou upomínku je povoleno generovat až po uplynutí {recP83.p83Days_Index2} dní od vygenerování první upomínky ({recPre.p84Date.ToString("dd.MM.yyyy")}).<hr>Toto můžete změnit v nastavení typu upomínky.");
                            return 0;   //ještě nenastal čas na druhou upomínku
                        }
                        rec.p84Name = recP83.p83Name_Index2;
                        rec.p84TextA = recP83.p83TextA_Index2;
                        rec.p84TextB = recP83.p83TextB_Index2;
                        break;
                    case 3:
                        recPre = lis.First(p => p.p84Index == 2);                        
                        if ((DateTime.Today - recPre.p84Date).Days < recP83.p83Days_Index3)
                        {
                            this.AddMessageTranslated("Ještě nenastal čas na třetí upomínku.");
                            return 0;   //ještě nenastal čas na třetí upomínku
                        }
                        rec.p84Name = recP83.p83Name_Index3;
                        rec.p84TextA = recP83.p83TextA_Index3;
                        rec.p84TextB = recP83.p83TextB_Index3;
                        break;
                    case 4:
                    case 5:
                        this.AddMessageTranslated("Třetí upomínka je poslední.");
                        return 0;   //třetí upomínka je poslední
                        
                    default:
                        break;
                }
                
                
                
                rec.p84Index = intStupenCeka;

            }
            
            rec.p84Code = $"{recP91.p91Code}-U{rec.p84Index}";
            

            return Save(rec);                        
        }


        private void MailMergeByTextTemplate(int p84id)
        {            
            var recP84 = Load(p84id);
            if (recP84 != null && (recP84.p84TextA !=null || recP84.p84TextB !=null))
            {
                var dt = _mother.gridBL.GetList4MailMerge("p84", p84id);
                string strA = BO.Code.MergeContent.GetMergedContent(recP84.p84TextA, dt);
                string strB = BO.Code.MergeContent.GetMergedContent(recP84.p84TextB, dt);
                _db.RunSql("UPDATE p84Upominka SET p84TextA=@texta,p84TextB=@textb WHERE p84ID=@pid", new {pid=p84id,texta= strA, textb=strB });
            }

            
        }

        public int Save(BO.p84Upominka rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(rec.p84Code))
            {
                var recP91 = _mother.p91InvoiceBL.Load(rec.p91ID);
                rec.p84Code= $"{recP91.p91Code}-U{rec.p84Index}";
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
           
            p.AddString("p84Name", rec.p84Name);
            p.AddInt("p83ID", rec.p83ID, true);
            p.AddInt("p91ID", rec.p91ID, true);
            if (rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;
            p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
            
            p.AddInt("p84Index", rec.p84Index);
            p.AddString("p84Code", rec.p84Code);
            p.AddString("p84TextA", rec.p84TextA);            
            p.AddString("p84TextB", rec.p84TextB);
            p.AddDateTime("p84Date", rec.p84Date);

            int intPID = _db.SaveRecord("p84Upominka", p, rec);
            if (intPID > 0)
            {
                MailMergeByTextTemplate(intPID);


                _db.RunSql("UPDATE p91Invoice SET p84ID_Last=null WHERE p91ID=@p91id; UPDATE a set p84ID_Last=b.p84ID FROM p91Invoice a INNER JOIN (select top 1 p84ID,p91ID FROM p84Upominka WHERE p84ValidUntil>GETDATE() AND p91ID=@p91id ORDER BY p84Index DESC) b ON a.p91ID=b.p91ID WHERE a.p91ID=@p91id", new {p91id=rec.p91ID});
            }
            return intPID;

        }
        private bool ValidateBeforeSave(BO.p84Upominka rec)
        {
            if (rec.p83ID==0)
            {
                this.AddMessage("Chybí vyplnit [Typ upomínky]."); return false;
            }
            if (rec.p91ID == 0)
            {
                this.AddMessage("Chybí vazba na fakturu."); return false;
            }
            

            return true;
        }

        public int NajdiVychoziJ61ID(BO.p83UpominkaType recP83,BO.p84Upominka recP84)
        {
            int j61id = 0;
            if (j61id == 0 && recP84.p84Index == 1 && recP83.j61ID_Index1 > 0)
            {
                j61id = recP83.j61ID_Index1;
            }
            if (j61id == 0 && recP84.p84Index == 2 && recP83.j61ID_Index2 > 0)
            {
                j61id = recP83.j61ID_Index2;
            }
            if (j61id == 0 && recP84.p84Index == 3 && recP83.j61ID_Index3 > 0)
            {
                j61id = recP83.j61ID_Index3;
            }

            return j61id;
        }

    }


}
