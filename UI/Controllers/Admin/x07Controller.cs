using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.Admin
{
    public class x07Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new x07Record() { rec_pid = pid, rec_entity = "x07" };
            v.Rec = new BO.x07Integration() { x07Flag = BO.x07FlagEnum.Ecomail };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x07IntegrationBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.Config = Factory.x07IntegrationBL.LoadConfig(v.Rec);
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }
            //heslo a token se do stránky zpět neposílají
            v.HasPassword = !string.IsNullOrEmpty(v.Rec.x07Password) && !isclone;
            v.HasToken = !string.IsNullOrEmpty(v.Rec.x07Token) && !isclone;
            v.Rec.x07Password = null;
            v.Rec.x07Token = null;

            RefreshSate(v);
            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshSate(x07Record v)
        {
            if (v.Config == null) v.Config = new BO.x07Config();
            if (v.Rec.x07Flag == BO.x07FlagEnum.Ecomail)
            {
                v.IsShowToken = true;
            }
            if (v.Rec.x07Flag == BO.x07FlagEnum.AbraFlexi)
            {
                v.IsShowLoginPassword = true;

                //řádek mapování pro každý typ dokumentu se šablonou Účtenka
                var maps = new List<BO.x07ConfigDocMap>();
                v.DocMapNames = new List<string>();
                foreach (var recO18 in Factory.o18DocTypeBL.GetList(new BO.myQueryO18()).Where(p => p.o18TemplateFlag == BO.o18TemplateENUM.Uctenka))
                {
                    var map = v.Config.DocMaps?.FirstOrDefault(p => p.o18ID == recO18.pid) ?? new BO.x07ConfigDocMap() { o18ID = recO18.pid };
                    maps.Add(map);
                    v.DocMapNames.Add(recO18.o18Name);
                }
                v.Config.DocMaps = maps;
            }
            if (v.rec_pid > 0)
            {
                v.lisLog = Factory.x07IntegrationBL.GetList_x09(v.rec_pid, 20);
            }
        }

        //uložené heslo/token doplnit tam, kde je uživatel nezměnil
        private BO.x07Integration MergeSecrets(x07Record v)
        {
            var c = new BO.x07Integration();
            if (v.rec_pid > 0) c = Factory.x07IntegrationBL.Load(v.rec_pid);
            c.x07Name = v.Rec.x07Name;
            c.x07Login = v.Rec.x07Login;
            if (!string.IsNullOrEmpty(v.Rec.x07Password)) c.x07Password = v.Rec.x07Password;
            if (!string.IsNullOrEmpty(v.Rec.x07Token)) c.x07Token = v.Rec.x07Token.Trim();
            c.x07Ordinary = v.Rec.x07Ordinary;
            c.x07Flag = v.Rec.x07Flag;
            c.x07Url = v.Rec.x07Url;
            return c;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(Models.Record.x07Record v)
        {
            RefreshSate(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "test")
                {
                    var t = MergeSecrets(v);    //test běží nad hodnotami z formuláře, nic se neukládá
                    if (t.x07Flag == BO.x07FlagEnum.AbraFlexi)
                    {
                        v.TestMessage = new UI.Code.Integrace.AbraFlexiService(t, v.Config).TestConnection();
                    }
                    else
                    {
                        var lists = new UI.Code.Integrace.EcomailService(t).GetLists(out string strErr);
                        v.TestMessage = strErr;
                        if (lists != null) v.TestMessage = null;
                    }
                    v.TestOk = v.TestMessage == null;
                    if (v.TestOk) v.TestMessage = "Spojení je v pořádku.";
                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.x07Integration c = MergeSecrets(v);
                if (c.x07Flag == BO.x07FlagEnum.AbraFlexi)
                {
                    v.Config.DocMaps = v.Config.DocMaps?.Where(p => !string.IsNullOrEmpty(p.Evidence)).ToList() ?? new List<BO.x07ConfigDocMap>();
                    if (string.IsNullOrWhiteSpace(v.Config.HomeCurrency)) v.Config.HomeCurrency = "CZK";
                    v.Config.HomeCurrency = v.Config.HomeCurrency.Trim().ToUpper();
                }
                var old = Factory.x07IntegrationBL.LoadConfig(c);
                v.Config.LastListID = old.LastListID;
                c.x07Config = System.Text.Json.JsonSerializer.Serialize(v.Config);

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.x07IntegrationBL.Save(c);
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }
                RefreshSate(v);     //po neúspěšném uložení znovu sestavit řádky mapování
            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
