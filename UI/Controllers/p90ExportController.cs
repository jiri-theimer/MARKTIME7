using Microsoft.AspNetCore.Mvc;
using UI.Models.p90oper;

namespace UI.Controllers
{
    public class p90ExportController : BaseController
    {
        public IActionResult Index(string p90ids, string guid_pids,string destformat)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                p90ids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var lis = BO.Code.Bas.ConvertString2ListInt(p90ids);
            if (lis.Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí zálohy.");
            }
            var v = new exportViewModel() { p90ids = p90ids };
            if (string.IsNullOrEmpty(destformat))
            {
                v.destformat = Factory.CBL.LoadUserParam("p90export-destformat", "pohoda");
            }
            else
            {
                v.destformat = destformat;
            }
            
            v.iszip = Factory.CBL.LoadUserParamBool("p90export-iszip", false);
            RefreshState(v);
            foreach (var c in v.lisP90)
            {
                if (c.p90IsDraft)
                {
                    return this.StopPage(true, $"{c.p90Code} je DRAFT faktura.");
                }

               
            }

            if (v.destformat != null && v.lisP90.Count() > 0)
            {
                handle_generate(v);
            }

            return View(v);
        }

        private void RefreshState(exportViewModel v)
        {
            var lis = BO.Code.Bas.ConvertString2ListInt(v.p90ids);
            v.lisP90 = Factory.p90ProformaBL.GetList(new BO.myQueryP90() { pids = lis });
            if (v.tempsubfolder == null)
            {
                v.tempsubfolder = $"p90export-{Factory.CurrentUser.j02Login}-{BO.Code.Bas.ObjectDateTime2String(DateTime.Now, "dd-MM-yyyy-HH-mm-ss-fff")}";
            }
        }


        [HttpPost]
        public IActionResult Index(exportViewModel v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "destformat")
                {
                    Factory.CBL.SetUserParam("p90export-destformat", v.destformat);
                    handle_generate(v);
                }
                if (v.PostbackOper == "iszip")
                {
                    Factory.CBL.SetUserParam("p90export-iszip", BO.Code.Bas.GB(v.iszip));
                    handle_generate(v);
                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                handle_generate(v);
            }




            return View(v);
        }



        private void handle_generate(exportViewModel v)
        {
            v.FileNames = new List<string>();
            
            if (v.tempsubfolder != null)
            {
                if (!System.IO.Directory.Exists($"{Factory.TempFolder}\\{v.tempsubfolder}"))
                {
                    System.IO.Directory.CreateDirectory($"{Factory.TempFolder}\\{v.tempsubfolder}");
                }
            }

            var httpclient = new HttpClient();
            var lisRecs = new List<BO.Integrace.InputZaloha>();

            foreach(var c in v.lisP90)
            {
                var rec = Factory.p90ProformaBL.CreateIntegraceRecord(c);
                lisRecs.Add(rec);
            }

            
           
            if (v.destformat == "pohoda")
            {
                //pro POHODu generovat celý Pack
                v.FileNames = new List<string>();

                var strPohodaPack = BL.Code.p90Support.GeneratePohodaXml(lisRecs, httpclient, $"{Factory.TempFolder}\\{v.tempsubfolder}");
                if (strPohodaPack != null)
                {
                    v.FileNames.Add($"POHODA_ZALOHA.xml");
                }

            }



            if (v.iszip && v.lisP90.Count() > 0)
            {
                string strZipFileName = $"Export-{DateTime.Now.ToString("dd-MM-yyyy-HH-mm")}.ZIP";
                if (System.IO.File.Exists($"{Factory.TempFolder}\\{strZipFileName}"))
                {
                    System.IO.File.Delete($"{Factory.TempFolder}\\{strZipFileName}");
                }
                System.IO.Compression.ZipFile.CreateFromDirectory($"{Factory.TempFolder}\\{v.tempsubfolder}", $"{Factory.TempFolder}\\{strZipFileName}");
                v.FileNames.Clear();
                v.FileNames.Add(strZipFileName);
            }


        }
    }
}
