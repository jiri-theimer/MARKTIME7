using BO.Code;
using BO.Rejstrik;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class ImportController : BaseController
    {
        public IActionResult p28_by_ico()
        {
            var v = new ImportViewModel() { Prefix = "p28" };

            RefreshState_p28_by_ico(v);
            return View(v);
        }

        private void RefreshState_p28_by_ico(ImportViewModel v)
        {
            if (v.Cols == null)
            {
                v.Cols = new List<string>();
            }

            if (v.Clipboard != null)
            {
                v.ClipLines = BO.Code.Bas.ConvertString2List(v.Clipboard, Environment.NewLine);
                string s = v.ClipLines[0];
                v.FirstLine = s.Split('\t').ToList();
                v.ClipColsCount = v.FirstLine.Count();


                
            }

            

        }

        private void Handle_TryImportContacts(ImportViewModel v)
        {
            var cr = new BL.Code.RejstrikySupport();
            var hc = new HttpClient();

            v.lisRejstrik = new List<BO.Rejstrik.DefaultZaznam>();
            foreach (string strICO in v.ClipLines)
            {                
                if (!string.IsNullOrEmpty(strICO))
                {
                    
                    var c = cr.LoadDefaultZaznam("ico", strICO.Trim(), v.CountryCode, hc).Result;
                    if (c != null && c.errormessage == null)
                    {
                        var dupl = Factory.p28ContactBL.LoadByICO(c.ico, 0);
                        if (dupl == null)
                        {
                            v.lisRejstrik.Add(c);
                        }
                        else
                        {
                            this.AddMessageTranslated($"Duplitní kontakt, IČO={c.ico}, {c.name}.");
                        }


                    }
                }
               
            }
        }

        [HttpPost]
        public IActionResult p28_by_ico(ImportViewModel v)
        {
            RefreshState_p28_by_ico(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "clear")
                {
                    return this.RedirectToAction("p28_by_ico", new { prefix = v.Prefix });
                }
                
                if (v.PostbackOper == "prepare")
                {
                    Handle_TryImportContacts(v);
                    if (v.lisRejstrik.Count() > 0)
                    {
                        v.StepIndex = 1;
                    }
                    
                }
                

                return View(v);
            }

            if (ModelState.IsValid)
            {

                if (v.ClipLines==null || v.ClipLines.Count() == 0)
                {
                    return View(v);
                }
                Handle_TryImportContacts(v);

                if (v.lisRejstrik.Count() == 0)
                {
                    this.AddMessageTranslated("Žádný načtený záznam z rejistříků.");
                    return View(v);
                }

                List<string> errs = new List<string>();

                foreach (var c in v.lisRejstrik)
                {
                    var rec = new BO.p28Contact() {p28CountryCode=v.CountryCode, j02ID_Owner=Factory.CurrentUser.pid, p28RegID = c.ico,p28CompanyName=c.name,p28IsCompany=true, p28VatID = c.dic,p28Street1=c.street,p28PostCode1=c.zipcode,p28City1=c.city };
                    rec.p29ID = v.SelectedP29ID;
                    if (v.CountryCode == "CZ")
                    {
                        rec.p28Country1 = "Česká republika";
                    }
                    else
                    {
                        rec.p28Country1 = "Slovensko";
                    }
                    int intPID=Factory.p28ContactBL.Save(rec, null, null, null, null, null);
                    if (intPID == 0)
                    {
                        errs.Add($"{rec.p28RegID}/{rec.p28CompanyName}: {Factory.CurrentUser.GetLastMessageNotify()}");
                    }
                    
                }
                

                if (errs.Count() == 0)
                {
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }
                else
                {
                    this.AddMessageTranslated(string.Join("<hr>", errs));
                }


            }

            Notify_RecNotSaved();
            return View(v);

        }
        public IActionResult Index(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = "p31";
            }
            var v = new ImportViewModel() {Prefix=prefix,DefaultJ02ID=Factory.CurrentUser.pid,DefaultComboJ02=Factory.CurrentUser.FullnameDesc, IsFirstColHeaders=true, Guid = BO.Code.Bas.GetGuid(),Delimiter= @"\tab" };
            v.DefaultP32ID = Factory.CBL.LoadUserParamInt($"import-p32id-{v.Prefix}");
            if (v.DefaultP32ID > 0)
            {
                v.DefaultComboP32 = Factory.p32ActivityBL.Load(v.DefaultP32ID).p32Name;
            }
            v.DefaultP41ID = Factory.CBL.LoadUserParamInt($"import-p41id-{v.Prefix}");
            if (v.DefaultP41ID > 0)
            {
                v.DefaultComboP41 = Factory.p41ProjectBL.Load(v.DefaultP41ID).FullName;
            }
            v.p31ExternalCode = $"import-{DateTime.Now.ToString("dd.MM.yyyy hh:mm")}";

            v.Cols = new List<string>();


            RefreshState(v);

            return View(v);
        }

        private void RefreshState(ImportViewModel v)
        {
            if (v.Cols == null)
            {
                v.Cols = new List<string>();
            }

            if (v.Clipboard != null)
            {
                v.ClipLines = BO.Code.Bas.ConvertString2List(v.Clipboard, Environment.NewLine);
                string s = v.ClipLines[0];
                v.FirstLine = s.Split('\t').ToList();
                v.ClipColsCount = v.FirstLine.Count();

                
                if (v.Cols.Count() == 0)
                {
                    LoadLastSettings(v);
                }
                if (v.Cols.Count() != v.ClipColsCount)
                {
                    v.Cols.Clear();
                    for (int i = 0; i < v.ClipColsCount; i++)
                    {
                        v.Cols.Add("");
                    }
                }
            }

            if (v.Clipboard !=null && v.Cols.Count() > 0)
            {
                Refresh_Parse_Content(v);
            }
            





        }

        

        [HttpPost]
        public IActionResult Index(ImportViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "clear")
                {
                    return this.RedirectToAction("Index", new { prefix = v.Prefix });
                }
                
                RefreshState(v);
                
                return View(v);
            }

            if (ModelState.IsValid)
            {
                SaveLastSettings(v);

                List<string> errs=RunImport(v);

                if (errs.Count() == 0)
                {
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }
                else
                {
                    this.AddMessageTranslated(string.Join("<hr>", errs));
                }

                
            }

            Notify_RecNotSaved();
            return View(v);
        }

        private void SaveLastSettings(ImportViewModel v)
        {
            string s = string.Join(",", v.Cols);
            Factory.CBL.SetUserParam($"import-cols-{v.Prefix}", s);
            Factory.CBL.SetUserParam($"import-p32id-{v.Prefix}", v.DefaultP32ID.ToString());
            Factory.CBL.SetUserParam($"import-p41id-{v.Prefix}", v.DefaultP41ID.ToString());

        }
        private void LoadLastSettings(ImportViewModel v)
        {
            if (v.Cols == null)
            {
                v.Cols = new List<string>();
            }

            string s = Factory.CBL.LoadUserParam($"import-cols-{v.Prefix}");
            if (s == null) return;

            foreach(string col in BO.Code.Bas.ConvertString2List(s))
            {
                v.Cols.Add(col);
            }
        }

        private List<string> RunImport(ImportViewModel v)
        {
            var errs = new List<string>();
            
            foreach(var c in v.lisP31.Where(p=>p.IsCanImport))
            {
                var rec = new BO.p31WorksheetEntryInput() {p31Text = c.p31Text, p31TextInternal = c.p31TextInternal,j02ID=c.RecJ02.pid,p41ID=c.RecP41.pid,p32ID=c.RecP32.pid,p31ExternalCode=v.p31ExternalCode };
                rec.p31Date = new List<DateTime>();
                rec.p31Date.Add(c.p31Date.Value);

                rec.Value_Orig = c.p31Value_Orig.ToString();
                rec.p31HoursEntryflag = BO.p31HoursEntryFlagENUM.Hodiny;
                rec.p34ID = Factory.p32ActivityBL.Load(rec.p32ID).p34ID;

                rec.TimeFrom = c.CasOd;
                rec.TimeUntil = c.CasDo;

                int intPID=Factory.p31WorksheetBL.SaveOrigRecord(rec, BO.p33IdENUM.Cas, null);
                if (intPID == 0)
                {
                    errs.Add($"#{c.Index}: {Factory.GetFirstNotifyMessage()}");
                   
                }
            }

            return errs;
        }

        private void Refresh_Parse_Content(ImportViewModel v)
        {
            v.lisP31 = new List<TempRowP31>();
            var lisP41 = Factory.p41ProjectBL.GetList(new BO.myQueryP41("le5") { IsRecordValid=null});
            var lisP28 = Factory.p28ContactBL.GetList(new BO.myQueryP28() { IsRecordValid = null });
            var lisJ02 = Factory.j02UserBL.GetList(new BO.myQueryJ02() { IsRecordValid = null });
            var lisP32 = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { p33id=1 });

            int x = 0;

            foreach (string s in v.ClipLines)
            {
                x += 1;
                if (v.IsFirstColHeaders && x==1)
                {                    
                    continue;   //první řádek obsahuje názvy sloupců
                }
                
                var arr = s.Split('\t').ToList();
                var c = new TempRowP31() { Index = (v.IsFirstColHeaders ? x-1: x) };
                v.lisP31.Add(c);
                
                for (int i = 0; i < arr.Count(); i++)
                {
                    

                    if (i>(v.ClipColsCount-1) || string.IsNullOrEmpty(v.Cols[i]) || string.IsNullOrEmpty(arr[i]))
                    {
                        continue;
                    }
                    switch (v.Cols[i])
                    {
                        case "p31Date":
                            c.p31Date = BO.Code.Bas.String2Date(arr[i]);
                            break;
                        case "CasOd":
                            c.CasOd = arr[i];
                            break;
                        case "CasDo":
                            c.CasDo = arr[i];
                            break;
                        case "p31Value_Orig":
                            c.p31Value_Orig = BO.Code.Bas.InDouble(arr[i]);
                            break;
                        case "p31Text":
                            c.p31Text = arr[i];
                            break;
                        case "p31TextInternal":
                            c.p31TextInternal = arr[i];
                            break;
                        case "j02Name":
                            if (lisJ02.Any(p => p.FullnameDesc.Contains(arr[i])))
                            {
                                c.RecJ02 = lisJ02.First(p => p.FullnameDesc.Contains(arr[i]));
                            }
                            break;
                        case "j02Code":
                            if (lisJ02.Any(p => p.j02Code.ToLower() == arr[i].ToLower()))
                            {
                                c.RecJ02 = lisJ02.First(p => p.j02Code.ToLower() == arr[i].ToLower());
                            }
                            break;
                        case "p32Name":
                            if (lisP32.Any(p => p.p32Name.ToLower() == arr[i].ToLower()))
                            {
                                c.RecP32 = lisP32.First(p => p.p32Name.ToLower() == arr[i].ToLower());
                            }
                            break;
                        case "p41Code":
                            if (lisP41.Any(p => p.p41Code.ToLower() == arr[i].ToLower()))
                            {
                                c.RecP41 = lisP41.First(p => p.p41Code.ToLower() == arr[i].ToLower());
                            }

                            break;
                        case "p41Name":
                            if (c.RecP28 == null)
                            {
                                var qry = lisP41.Where(p => p.p41Name == arr[i]);

                                if (lisP41.Any(p => p.p41Name.ToLower() == arr[i].ToLower()))
                                {
                                    c.RecP41 = lisP41.First(p => p.p41Name.ToLower() == arr[i].ToLower());
                                }
                            }
                            else
                            {
                                var qry = lisP41.Where(p =>p.p28ID_Client==c.RecP28.pid && p.p41Name == arr[i]);

                                if (lisP41.Any(p =>p.p28ID_Client==c.RecP28.pid &&  p.p41Name.ToLower() == arr[i].ToLower()))
                                {
                                    c.RecP41 = lisP41.First(p => p.p41Name.ToLower() == arr[i].ToLower());
                                }
                            }
                            
                            break;
                        case "p28Name":
                            if (c.RecP28==null && lisP28.Any(p => p.p28Name.ToLower() == arr[i].ToLower()))
                            {
                                c.RecP28 = lisP28.First(p => p.p28Name.ToLower() == arr[i].ToLower());
                            }
                            break;
                        case "p28RegID":
                            if (c.RecP28 == null && lisP28.Any(p => p.p28RegID.ToLower() == arr[i].ToLower()))
                            {
                                c.RecP28 = lisP28.First(p => p.p28RegID.ToLower() == arr[i].ToLower());
                            }
                            break;

                    }
                }
                if (c.RecP41==null && v.DefaultP41ID > 0)
                {
                    c.RecP41 = Factory.p41ProjectBL.Load(v.DefaultP41ID);
                }
                if (c.RecJ02==null && v.DefaultJ02ID > 0)
                {
                    c.RecJ02 = Factory.j02UserBL.Load(v.DefaultJ02ID);
                }                
                if (c.RecP32 == null && v.DefaultP32ID > 0)
                {
                    c.RecP32 = Factory.p32ActivityBL.Load(v.DefaultP32ID);
                }
                if (c.RecP41 == null || c.RecJ02 == null || c.RecP32 == null || c.p31Value_Orig == 0)
                {
                    c.CssStyle = "color:red";
                    c.IsCanImport = false;
                }
                else
                {
                    c.IsCanImport = true;
                }
            }

        }


    }
}
