
using Microsoft.AspNetCore.Mvc;
using UI.Models.banka;


namespace UI.Controllers.banka
{
    public class BankaImportController : BaseController
    {
        public IActionResult Index()
        {
            var v = new BankaImportViewModel() { TypVypisu = "fio",Guid=BO.Code.Bas.GetGuid() };

            return View(v);
        }

        [HttpPost]
        public IActionResult Index(BankaImportViewModel v)
        {
            
            if (v.TypVypisu == "fio" && string.IsNullOrEmpty(v.FioApiKey))
            {
                AddMessageTranslated("Na vstupu chybí Api Klíč");
                return View(v);
            }
            if (v.TypVypisu == "gpc" && v.PostbackOper=="nacist" && v.file1 != null)
            {
                
                Handle_SaveUpload2File(v);                
            }

            if (ModelState.IsValid)
            {
                if (v.TypVypisu == "fio")
                {
                    Handle_FioLoad(v);
                }
                if (v.TypVypisu == "gpc")
                {
                    Handle_GpcLoad(v);
                }
                

                if (v.PostbackOper == "finish")
                {
                    foreach (var polozka in v.lisPolozky.Where(p => p.p91Code != null))
                    {
                        var rec = new BO.p94Invoice_Payment() { p94Date = polozka.p94Date, p94Code = polozka.p94Code, p94Description = polozka.p94Description, p94Amount = polozka.p94Amount };
                        var recP91 = Factory.p91InvoiceBL.LoadByCode(polozka.p91Code);
                        if (recP91 != null && recP91.p91Amount_Debt > 0)
                        {
                            rec.p91ID = recP91.pid;
                            Factory.p91InvoiceBL.SaveP94(rec);

                            polozka.Dluh = Factory.p91InvoiceBL.Load(recP91.pid).p91Amount_Debt;
                        }

                    }
                    AddMessageTranslated("Dokončeno", "info");
                }
            }
           

            return View(v);

        }


        private void Handle_FioLoad(BankaImportViewModel v)
        {
            var lisTemp = Factory.p85TempboxBL.GetList(v.Guid);
            if (lisTemp.Count() > 0)
            {
                //bankovní výpis je uložený v tempu
                v.lisPolozky = new List<Polozka>();
                foreach(var rt in lisTemp)
                {
                    var polozka = new Polozka() {Dluh=rt.p85FreeNumber01, p94Amount = rt.p85FreeNumber02, p94Date = rt.p85FreeDate01.Value, p91Code=rt.p85FreeText01, p94Description=rt.p85FreeText02,Protiucet=rt.p85FreeText03 };
                    v.lisPolozky.Add(polozka);
                }
                
                return;
            }
            var engine = new BL.Code.FioBankaSupport();
            var ret = engine.LoadVypis(v.FioApiKey);

            v.lisPolozky = new List<Polozka>();

            var c = ret.Result;
            if (c == null)
            {
                this.AddMessageTranslated("Při načítání výpisu došlo k chybě: Handle_FioLoad, c == null.");
                return;
            }
            var lis = c.accountStatement.transactionList.transaction.ToList().Where(p => p.column1.value > 0);
            foreach (var cc in lis)
            {
                var polozka = new Polozka() { p94Amount = cc.column1.value, p94Date = Convert.ToDateTime(cc.column0.value) };
                if (cc.column5 != null)
                {
                    polozka.p94Code = cc.column5.value;
                }
                if (cc.column2 != null)
                {
                    polozka.Protiucet = cc.column2.value;
                    if (cc.column3 != null)
                    {
                        polozka.Protiucet += "/" + cc.column3.value;
                    }
                }

                if (cc.column10 != null && !string.IsNullOrEmpty(cc.column10.value))
                {
                    polozka.p94Description = cc.column10.value;
                }
                else
                {
                    if (cc.column7 != null)
                    {
                        polozka.p94Description = cc.column7.value;
                    }
                }
                //polozka.p94Description = $"{(cc.column10 != null ? cc.column10.value : "")} {(cc.column7 != null ? cc.column7.value : "")}";

                if (polozka.Protiucet != null)
                {
                    var recP91 = v.NajitFakturu(this.Factory, polozka);
                    if (recP91 != null)
                    {
                        polozka.p91Code = recP91.p91Code;
                        polozka.Dluh = recP91.p91Amount_Debt;
                    }
                    v.lisPolozky.Add(polozka);
                    var recTemp = new BO.p85Tempbox() { p85GUID = v.Guid, p85FreeText01 = polozka.p91Code, p85FreeNumber01 = polozka.Dluh,p85FreeText02=polozka.p94Description,p85FreeText03=polozka.Protiucet,p85FreeNumber02=polozka.p94Amount,p85FreeDate01=polozka.p94Date };
                    Factory.p85TempboxBL.Save(recTemp);
                }

            }
        }


        private void Handle_SaveUpload2File(BankaImportViewModel v)
        {
            
            var rs = new StreamReader(v.file1.OpenReadStream(), System.Text.Encoding.GetEncoding(1250),true);

            var strTempFile = this.Factory.TempFolder + "\\" + v.Guid + ".gpc";
            System.IO.File.WriteAllText(strTempFile, rs.ReadToEnd(), System.Text.Encoding.GetEncoding(1250));


        }

        private void Handle_GpcLoad(BankaImportViewModel v)
        {
            var strTempFile = Factory.TempFolder + "\\" + v.Guid + ".gpc";
            if (!System.IO.File.Exists(strTempFile))
            {
                return;
            }

            
            var cGpc = new BL.Code.GpcFileHandle();
            v.lisGpcPolozky = cGpc.ParseFile(strTempFile).Where(p => p.KodUctovani == "2").ToList();


            var lisP91 = Factory.p91InvoiceBL.GetList(new BO.myQueryP91());
            foreach (var row in v.lisGpcPolozky.Where(p => !string.IsNullOrEmpty(p.TheVariabilniSymbol)))
            {
                var qry = lisP91.Where(p => p.p91Code == row.TheVariabilniSymbol);
                if (qry.Count() > 0)
                {
                    row.p91ID = qry.First().pid;
                    row.CurrentDebt = qry.First().p91Amount_Debt;
                    
                }
            }
            var lisP28 = Factory.p28ContactBL.GetList(new BO.myQueryP28() { IsRecordValid = null }).Where(p => p.p28BankAccount != null);

            foreach (var row in v.lisGpcPolozky.Where(p => p.p91ID == 0))        //nespárované
            {
                var qryP28 = lisP28.Where(p => (p.p28BankAccount + "/" + p.p28BankCode) == row.TheProtistranaUcet);   //najít klienta podle bankovního účtu
                if (qryP28.Count() > 0)
                {
                    var recP28 = qryP28.First();
                    var qryP91 = lisP91.Where(p => p.p91Amount_Debt == row.TheCastka && p.p28ID == recP28.pid); //najít neuhrazenou fakturu klienta
                    if (qryP91.Count() > 0)
                    {
                        row.p91ID = qryP91.First().pid;
                        row.CurrentDebt = qryP91.First().p91Amount_Debt;
                    }
                }
            }

            v.lisPolozky = new List<Polozka>();
            foreach(var row in v.lisGpcPolozky.Where(p => p.p91ID > 0 || p.KodUctovani=="2"))
            {
                var polozka = new Polozka() { p91ID = row.p91ID, p94Amount = row.TheCastka, p94Description = row.ThePopis, Protiucet = row.TheProtistranaUcet, p94Code = row.TheVariabilniSymbol };
                polozka.Dluh = row.CurrentDebt;
                polozka.p94Date = row.TheDatum.Value;

                if (polozka.Protiucet != null)
                {
                    var recP91 = v.NajitFakturu(this.Factory, polozka);
                    if (recP91 != null)
                    {
                        polozka.p91Code = recP91.p91Code;
                        polozka.Dluh = recP91.p91Amount_Debt;
                    }                    
                }

                v.lisPolozky.Add(polozka);
            }
        }

    }
}
