using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers.Admin
{
    public class x01Controller : BaseController
    {
        private readonly BL.AI.Core.AiConfig _ai;
        public x01Controller(BL.AI.Core.AiConfig ai)
        {
            _ai = ai;
        }

        /// <param name="tab">otevřít záložku (1 Karta licence, 2 Billing, 3 Různé, 4 AI)</param>
        public IActionResult Record(int pid, bool isclone, int tab = 0)
        {
            var v = new x01Record() { rec_pid = pid, rec_entity = "x01" };
            if (tab >= 1 && tab <= 4) v.ActiveTabIndex = tab;
            v.Rec = new BO.x01License();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x01LicenseBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Rec.x04ID_Default > 0)
                {
                    v.ComboX04 = Factory.x04NotepadConfigBL.GetList(new BO.myQuery("x04")).First(p => p.pid == v.Rec.x04ID_Default).x04Name;

                }

               

            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            v.Toolbar.AllowDelete = false;
            v.Toolbar.AllowClone = false;
            v.Toolbar.AllowArchive = false;
            
           

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(x01Record v)
        {
            v.AiGlobal = _ai.Global;
            string key = null;
            if (v.rec_pid > 0) key = Factory.x01LicenseBL.Load(v.rec_pid)?.x01AiApikey;
            v.AiApiKeyMasked = string.IsNullOrEmpty(key) ? null : "…" + (key.Length > 4 ? key.Substring(key.Length - 4) : "");
        }

        /// <summary>Uložený klíč, nebo nově zadaný / smazaný z formuláře.</summary>
        private string ResolveAiApiKey(x01Record v)
        {
            if (v.AiApiKeyClear) return null;
            if (!string.IsNullOrWhiteSpace(v.AiApiKeyNew)) return v.AiApiKeyNew.Trim();
            return v.rec_pid > 0 ? Factory.x01LicenseBL.Load(v.rec_pid)?.x01AiApikey : null;
        }

        /// <summary>Test připojení k AI s hodnotami z formuláře (bez uložení). Krátký požadavek, spotřeba v řádu desítek tokenů.</summary>
        private void TestAi(x01Record v)
        {
            var tmp = new BO.x01License()
            {
                x01AiUsageFlag = v.Rec.x01AiUsageFlag,
                x01AiApikey = ResolveAiApiKey(v),
                x01AiModel = v.Rec.x01AiModel,
                x01AiMaxTokens = v.Rec.x01AiMaxTokens,
                x01AiTimeoutSeconds = v.Rec.x01AiTimeoutSeconds
            };
            var o = _ai.ForLicense(tmp);
            if (!o.Enabled)
            {
                v.AiTestOk = false; v.AiTestResult = o.DisabledReason; return;
            }
            o.MaxTokens = 64;      // test nepotřebuje víc, nastavený limit se tím nekontroluje
            var req = new BL.AI.Core.LlmRequest()
            {
                Schema = (System.Text.Json.Nodes.JsonObject)System.Text.Json.Nodes.JsonNode.Parse("{\"type\":\"object\",\"properties\":{\"ok\":{\"type\":\"boolean\"}},\"required\":[\"ok\"],\"additionalProperties\":false}")
            };
            req.System.Add(new BL.AI.Core.LlmSystemBlock() { Text = "Test připojení. Odpověz {\"ok\": true}." });
            req.Messages.Add(new BL.AI.Core.LlmMessage() { Role = "user", Content = "ping" });

            var r = _ai.CreateClient(o).CompleteJsonAsync(req).GetAwaiter().GetResult();
            v.AiTestOk = r.Ok;
            v.AiTestResult = r.Ok ? $"Spojení funguje: model {r.Model}, {r.ElapsedMs} ms." : r.Error;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(x01Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "ping")
                {
                    var cping = new BL.Code.PingSupport();
                    var ret=cping.SendPing(Factory).Result;
                    this.AddMessageTranslated($"Ping vrátil: {ret}", "info");
                }
                if (v.PostbackOper == "aitest")
                {
                    TestAi(v);
                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.x01License c = new BO.x01License();
                if (v.rec_pid > 0) c = Factory.x01LicenseBL.Load(v.rec_pid);
                c.x01AppName = v.Rec.x01AppName;
                c.x01LangIndex = v.Rec.x01LangIndex;
                c.x01BillingLang1 = v.Rec.x01BillingLang1;
                c.x01BillingLang2 = v.Rec.x01BillingLang2;
                c.x01BillingLang3 = v.Rec.x01BillingLang3;
                c.x01BillingLang4 = v.Rec.x01BillingLang4;
                c.x01BillingFlag1 = v.Rec.x01BillingFlag1;
                c.x01BillingFlag2 = v.Rec.x01BillingFlag2;
                c.x01BillingFlag3 = v.Rec.x01BillingFlag3;
                c.x01BillingFlag4 = v.Rec.x01BillingFlag4;
                c.x01Round2Minutes = v.Rec.x01Round2Minutes;
                c.x01IsAllowPasswordRecovery = v.Rec.x01IsAllowPasswordRecovery;
                c.j27ID = v.Rec.j27ID;
                c.x01CountryCode = v.Rec.x01CountryCode;
                c.x01AppHost = v.Rec.x01AppHost;
                c.x01RobotLogin = v.Rec.x01RobotLogin;
                c.x01PasswordRecovery_Answer = v.Rec.x01PasswordRecovery_Answer;
                c.x01PasswordRecovery_Question = v.Rec.x01PasswordRecovery_Question;
              
                c.x01IsAllowDuplicity_RegID = v.Rec.x01IsAllowDuplicity_RegID;
                c.x01IsAllowDuplicity_VatID = v.Rec.x01IsAllowDuplicity_VatID;
                c.x01IsAllowDuplicity_p86 = v.Rec.x01IsAllowDuplicity_p86;

                c.x04ID_Default = v.Rec.x04ID_Default;
                c.x15ID = v.Rec.x15ID;
                c.x01InvoiceMaturityDays = v.Rec.x01InvoiceMaturityDays;
                c.x01IsCapacityFaNefa = v.Rec.x01IsCapacityFaNefa;
                c.x01LockFlag = v.Rec.x01LockFlag;
                c.x01InfosoudToken = v.Rec.x01InfosoudToken;

                c.x01AiUsageFlag = v.Rec.x01AiUsageFlag;
                c.x01AiApikey = ResolveAiApiKey(v);
                c.x01AiModel = v.Rec.x01AiModel;
                c.x01AiMaxTokens = v.Rec.x01AiMaxTokens;
                c.x01AiTimeoutSeconds = v.Rec.x01AiTimeoutSeconds;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.x01LicenseBL.Save(c);
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
