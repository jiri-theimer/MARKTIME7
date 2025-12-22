using BL;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using System.Data.SqlTypes;
using UI.Models;

namespace UI.Controllers
{
    public class PasswordController : Controller
    {
        private BL.Factory _f;
        private readonly IHttpClientFactory _httpclientfactory; //client pro SMS
        public PasswordController(BL.Factory f, IHttpClientFactory hcf)
        {
            _f = f;
            _httpclientfactory = hcf;
        }

        public IActionResult Index(int langindex)
        {
            var v = new PasswordViewModel() { LangIndex = langindex };

            
            if (_f.App.HostingMode == BL.Singleton.HostingModeEnum.None && !_f.Lic.x01IsAllowPasswordRecovery)
            {
                v.ErrorMessage = $"V databázi není povolena samo-obslužná obnova zapomenutého hesla.";
                return View(v);
            }
            
            return View(v);
        }


        [HttpPost]
        public IActionResult Index(PasswordViewModel v)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(v.Login))
                {
                    return View(v);
                }
                                   
                var fileNewsPwd1 = BO.Code.File.GetNewstFileFromDir(_f.App.RootUploadFolder, "*.pwd1");
                if (fileNewsPwd1 !=null && fileNewsPwd1.CreationTime.AddSeconds(120) > DateTime.Now)
                {
                    v.ErrorMessage = $"Z bezpečnostních důvodů lze novou žádost vygenerovat až za 120 sekund od: {fileNewsPwd1.CreationTime}";
                    return View(v);
                }
                var fileNewsPwd2 = BO.Code.File.GetNewstFileFromDir(_f.App.RootUploadFolder, "*.pwd2");
                if (fileNewsPwd2 !=null && fileNewsPwd2.CreationTime.AddSeconds(120) > DateTime.Now)
                {
                    v.ErrorMessage = $"Z bezpečnostních důvodů lze novou žádost vygenerovat až za 120 sekund od: {fileNewsPwd2.CreationTime}";
                    return View(v);
                }
                try
                {
                    _f.InhaleUserByLogin(v.Login);
                    
                    if (!_f.Lic.x01IsAllowPasswordRecovery)
                    {
                        v.ErrorMessage = $"V databázi není povoleno samo-obslužná obnova zapomenutého hesla.";
                        return View(v);
                    }

                }
                catch(Exception ex)
                {
                    if (_f.CurrentUser == null)
                    {
                        v.ErrorMessage = $"Uživatelský účet [{v.Login}] pravděpodobně neexistuje.";
                        return View(v);
                    }

                    v.ErrorMessage = ex.Message;
                    return View(v);
                }
                
                

                var recJ02 = _f.j02UserBL.LoadByLogin(v.Login,0,false);
                if (recJ02 == null)
                {
                    recJ02 = _f.j02UserBL.LoadByEmail(v.Login, 0,false);
                }

                if (recJ02 == null)
                {
                    v.ErrorMessage =v.Login+": "+ _f.trawi("Nelze dohledat účet s uživatelským jménem nebo e-mail adresou.", v.LangIndex);
                    return View(v);
                }
                if (recJ02.isclosed)
                {
                    v.ErrorMessage = v.Login+": "+_f.trawi("Uživatelský účet byl uzavřen, kontaktujte MARKTIME administrátora.", v.LangIndex);
                    return View(v);
                }
                if (recJ02.j02IsLoginAutoLocked || recJ02.j02IsLoginManualLocked)
                {
                    v.ErrorMessage =v.Login+": "+ _f.trawi("Uživatelský účet je zablokován, kontaktujte MARKTIME administrátora.", v.LangIndex);
                    return View(v);
                }

                _f.InhaleUserByLogin(recJ02.j02Login);

                string strGUID = BO.Code.Bas.GetGuid();                
                System.IO.File.WriteAllText($"{_f.App.RootUploadFolder}\\{strGUID}.pwd1", recJ02.j02Login);

                

                string strHtml = BO.Code.File.GetFileContent($"{_f.App.RootUploadFolder}\\_distribution\\mail\\send_message_template_password_index.html");
                strHtml = strHtml.Replace("#username#", v.Login);
                strHtml = strHtml.Replace("#link#", $"{_f.Lic.x01AppHost}/Password/ConfirmRecovery?guid={strGUID}");
                string strSubject = "Žádost o obnovení přihlašovacího hesla v aplikaci MARKTIME";

                var ret=_f.MailBL.SendMessageWithoutFactory(strHtml, strSubject, recJ02.j02Email, null,(_f.Lic.x01ContactEmail==null ? "noreply@marktime.net": _f.Lic.x01ContactEmail), "MARKTIME");
                if (!ret.issuccess)
                {
                    v.ErrorMessage = _f.trawi("Žádost o obnovení hesla byla založena, ale při odesílání potvrzovací e-mail zprávy došlo k chybě", v.LangIndex)+":<hr>"+ret.Message;
                    return View(v);
                }

                v.SuccessMessage = _f.trawi("Žádost o obnovení hesla byla odeslána na e-mail adresu", v.LangIndex) + ": " + recJ02.j02Email;

                

            }

            return View(v);
        }


        public IActionResult ConfirmRecovery(string guid)
        {
            var v = new PasswordViewModel();

            if (string.IsNullOrEmpty(guid))
            {
                v.ErrorMessage = "Na vstupu chybí GUID.";
                return View(v);
            }

            if (System.IO.File.Exists($"{_f.App.RootUploadFolder}\\{guid}.pwd2"))
            {
                v.ErrorMessage = _f.trawi("Tato žádost o obnovení hesla byla již dříve zpracována.", v.LangIndex);
                v.ErrorMessage += "<hr>Musíte vygenerovat novou žádost.";
                return View(v);
            }

            string strFile = $"{_f.App.RootUploadFolder}\\{guid}.pwd1";
            if (!System.IO.File.Exists(strFile))
            {
                v.ErrorMessage = _f.trawi("Na serveru nelze najít vyplněnou žádost o obnovu hesla!", v.LangIndex);return View(v);
            }
            var fi = BO.Code.File.GetFileInfo(strFile);
            if (fi.CreationTime.AddMinutes(30) < DateTime.Now)
            {
                v.ErrorMessage=_f.trawi("Tato žádost o obnovení přihlašovacího hesla je starší než 30 minut.",v.LangIndex);
                v.ErrorMessage += "<hr>Musíte vygenerovat novou žádost.";
                return View(v);
            }
            

            v.Login = System.IO.File.ReadAllText(strFile);
            if (_f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
            {
                _f.InhaleUserByLogin(v.Login);
            }
            var recJ02 = _f.j02UserBL.LoadByLogin(v.Login, 0, false);
            _f.InhaleUserByLogin(recJ02.j02Login);

            string strNewPwd = new BL.Code.PasswordSupport().GetRandomPassword();
            _f.j02UserBL.SaveNewPassword(recJ02.pid, strNewPwd,true);            
            

            string strHtml = BO.Code.File.GetFileContent($"{_f.App.RootUploadFolder}\\_distribution\\mail\\send_message_template_password_confirm.html");
            strHtml = strHtml.Replace("#username#", recJ02.j02Login);
            strHtml = strHtml.Replace("#newpwd#", strNewPwd);
            string strSubject = "Potvrzení nového hesla v aplikaci MARKTIME";

            var ret = _f.MailBL.SendMessageWithoutFactory(strHtml, strSubject, recJ02.j02Email, null, (_f.Lic.x01ContactEmail == null ? "noreply@marktime.net": _f.Lic.x01ContactEmail), "MARKTIME");

            if (!ret.issuccess)
            {
                v.ErrorMessage = _f.trawi("Potvrzení o novém heslu bylo založeno, ale při odesílání e-mail zprávy došlo k chybě", v.LangIndex) + ":<hr>" + ret.Message;
                return View(v);
            }

            v.SuccessMessage = _f.trawi("Nové heslo bylo vygenerováno.",v.LangIndex)+"<hr>"+_f.trawi("Informace o heslu byla odeslána na e-mail adresu", v.LangIndex) + ": " + recJ02.j02Email;
            System.IO.File.Move(strFile, strFile.Replace(".pwd1", ".pwd2"));

            return View(v);
        }
    }
}
