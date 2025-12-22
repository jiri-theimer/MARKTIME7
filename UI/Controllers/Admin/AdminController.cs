
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using UI.Models;
using UI.Models.Admin;
using UI.Code;
using UI.Views.Shared.Components.myGrid;

namespace UI.Controllers
{
    public class AdminController : BaseController
    {
        private readonly BL.TheColumnsProvider _colsProvider;
        public AdminController(BL.TheColumnsProvider cp)
        {
            _colsProvider = cp;
        }
       

        public IActionResult LogAsUser(string login, string code)
        {
            var v = new AdminLogAsUser() { Login = login, SecurityCode = code };

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }
        [HttpPost]
        public IActionResult LogAsUser(AdminLogAsUser v)
        {
            if (ModelState.IsValid)
            {

                if (string.IsNullOrEmpty(v.Login) || string.IsNullOrEmpty(v.SecurityCode))
                {
                    this.AddMessage("Login i API key je povinné zadat."); return View(v);
                }
                var recJ02 = Factory.j02UserBL.LoadByLogin(v.Login, 0,Factory.CurrentUser.IsHostingModeTotalCloud);
                if (recJ02 == null)
                {
                    this.AddMessage("Zadaný login neexistuje."); return View(v);
                }
                if (v.SecurityCode != Factory.Lic.x01ApiKey)
                {
                    this.AddMessage("API key není správný."); return View(v);
                }

                var strID = recJ02.j02Email;
                if (string.IsNullOrEmpty(strID)) { strID = "info@marktime.cz"; };
                var userClaims = new List<Claim>()
                {
                new Claim(ClaimTypes.Name, recJ02.j02Login),
                new Claim("access_token","inspis_core_token"),
                new Claim(ClaimTypes.Email, strID)
                 };

                var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");
                var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });

                var xx = new AuthenticationProperties() { IsPersistent = true, ExpiresUtc = DateTime.Now.AddHours(1) };
                HttpContext.SignInAsync(userPrincipal, xx);

                if (recJ02 != null)
                {
                    v.SetJavascript_CallOnLoad("/Home/Index");


                }

            }

            return View(v);
        }

        public string RunRecovery_Permissions()
        {
            Factory.FBL.RunRecovery_Permissions();
            return "1";
        }
        public IActionResult Index(bool? signpost)
        {
            if (!TUP(BO.PermValEnum.GR_Admin))
            {
                return this.StopPage(false, "Nemáte oprávnění Administrátora.");
            }
            if (signpost != null)
            {
                if (signpost == true)
                {   //odkaz do administrace z hlavního menu -> najít naposledy zobrazovanou sekci admin stránek
                    string strArea = Factory.CBL.LoadUserParam("Admin/last-area");
                    if (strArea != null && strArea != "index")
                    {
                        return RedirectToAction("Page", new { area = strArea });
                    }
                }
                else
                {
                    //kliknutí na [Úvod] v administraci
                    Factory.CBL.SetUserParam("Admin/last-area", "index");
                }
            }

            var v = new AdminHome() { RecX01 = Factory.x01LicenseBL.Load(Factory.CurrentUser.x01ID) };

            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            if (!string.IsNullOrEmpty(env.ContentRootPath))
            {
                v.IISBinding = env.ContentRootPath.Split("\\").Last();
            }
            
            

            v.lisP07 = Factory.p07ProjectLevelBL.GetList(new BO.myQuery("p07") { IsRecordValid = null });
            return View(v);
        }
        public IActionResult Page(string area, string prefix, int go2pid, string myqueryinline)
        {
            if (!TUP(BO.PermValEnum.GR_Admin))
            {
                return this.StopPage(false, "Nemáte oprávnění Administrátora.");
            }
            var v = new AdminPage() { area = area, prefix = prefix, go2pid = go2pid };
            if (area != null && prefix == null)
            {
                if (Factory.CBL.LoadUserParam("Admin/last-area") != area)
                {
                    Factory.CBL.SetUserParam("Admin/last-area", area);  //uložit naposledy navštívenou area
                }
            }
            string defprefix = null;
            switch (area)
            {
                case "users": defprefix = "j02"; break;
                case "projects": defprefix = "p42"; break;
                case "billing": defprefix = "p92"; break;
                case "proforma": defprefix = "p89"; break;
                case "contacts": defprefix = "p29"; break;
                case "worksheet": defprefix = "p32"; break;
                case "docs": defprefix = "o18"; break;
                case "mails": defprefix = "j40"; break;
                case "tasks": defprefix = "p57"; break;
                case "o22s": defprefix = "o21"; break;
                case "tags": defprefix = "o53"; break;
                case "reps": defprefix = "x31"; break;
                case "misc": defprefix = "j18"; break;
                
                case "index":
                    return RedirectToAction("Index");

            }
            handle_default_link(v, area, defprefix, ref myqueryinline);

            inhale_entity(v, v.prefix);

            v.gridinput = GetGridInput(v.entity, v.prefix, go2pid, null, myqueryinline);

            if (v.prefix != "x40" && v.prefix != "j90" && v.prefix != "j92" && v.prefix != "j06" && v.prefix != "j05" && v.prefix !="j06")
            {
                v.recordbinquery = new RecordBinQueryViewModel() { UserParamKey = "admin-" + v.prefix + "-recordbinquery" };
                v.recordbinquery.Value = Factory.CBL.LoadUserParamInt(v.recordbinquery.UserParamKey, 0);

                switch (v.recordbinquery.Value)
                {
                    case 1:
                        v.gridinput.query.IsRecordValid = true; break;
                    case 2:
                        v.gridinput.query.IsRecordValid = false; break;
                    default:
                        v.gridinput.query.IsRecordValid = null; break;
                }
            }
                                   

            return View(v);
        }
        public IActionResult CompanyLogo()
        {
            var v = new AdminCompanyLogo() { UploadGuidLogo = BO.Code.Bas.GetGuid(), IsMakeResize = true };
            return View(v);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompanyLogo(AdminCompanyLogo v)
        {
            if (ModelState.IsValid)
            {

                if (Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidLogo).Count() == 0)
                {
                    this.AddMessage("Pro změnu loga musíte nahrát soubor grafického loga.");
                    return View(v);
                }
                else
                {
                    var tempfile = Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidLogo).First();
                   
                    if (!(tempfile.o27FileExtension == ".png" || tempfile.o27FileExtension == ".jpg" || tempfile.o27FileExtension == ".gif" || tempfile.o27FileExtension == ".jpeg"))
                    {
                        this.AddMessage("Jako grafické logo lze nahrát pouze PNG, JPG nebo GIF soubor.");
                        return View(v);
                    }
                    var strOrigFileName = "company_logo_original" + tempfile.o27FileExtension;
                    System.IO.File.Copy(tempfile.FullPath, Factory.PluginsFolder + "\\" + strOrigFileName, true);

                    var strDestFileName = "company_logo";

                    var files2delete = BO.Code.File.GetFileListFromDir(Factory.PluginsFolder, strDestFileName + ".*", System.IO.SearchOption.TopDirectoryOnly, true);
                    foreach (string file2delete in files2delete)
                    {
                        System.IO.File.Delete(file2delete); //odstranit stávající logo soubory
                    }

                    strDestFileName += tempfile.o27FileExtension;
                    BO.Code.File.LogInfo(tempfile.o27FileExtension);
                    BO.Code.File.LogInfo(strDestFileName);

                    if (v.IsMakeResize)
                    {
                        basUI.ResizeImage(tempfile.FullPath, Factory.PluginsFolder + "\\" + strDestFileName, 250, 100);
                    }
                    else
                    {
                        System.IO.File.Copy(tempfile.FullPath, Factory.PluginsFolder + "\\" + strDestFileName, true);
                    }

                    Factory.Lic.x01LogoFileName = strDestFileName;
                    Factory.x01LicenseBL.Save(Factory.Lic);
                    Factory.App.RefreshX01List();

                    v.SetJavascript_CallOnLoad(1);
                    return View(v);
                }

            }
            return View(v);
        }

        private void handle_default_link(AdminPage v, string module, string defprefix, ref string myqueryinline)
        {
            if (v.prefix == null)
            {
                v.prefix = Factory.CBL.LoadUserParam($"Admin/{module}-prefix", defprefix);
                myqueryinline = Factory.CBL.LoadUserParam($"Admin/{module}-{v.prefix}-myqueryinline");
            }
            else
            {
                if (Factory.CBL.LoadUserParam($"Admin/{module}-prefix") != v.prefix)
                {
                    Factory.CBL.SetUserParam($"Admin/{module}-prefix", v.prefix);
                    Factory.CBL.SetUserParam($"Admin/{module}-{v.prefix}-myqueryinline", myqueryinline);
                }
            }
        }
        private void inhale_entity(AdminPage v, string prefix)
        {
            if (prefix != null)
            {
                BO.TheEntity c = null;
                try
                {
                    c = Factory.EProvider.ByPrefix(prefix);
                }
                catch(Exception e)
                {
                    this.AddMessageTranslated(e.Message+", prefix: "+prefix);
                    return;
                }
                
                v.entity = c.TableName;
                v.entityTitleSingle = c.AliasSingular;

                switch (Factory.CurrentUser.j02LangIndex)
                {
                    case 1:
                        v.entityTitle = c.TranslateLang1;
                        break;
                    case 2:
                        v.entityTitle = c.TranslateLang2;
                        break;
                    default:
                        v.entityTitle = c.AliasPlural;
                        break;
                }

            }
        }



        public BO.Result GenerateSpGenerateCreateUpdateScript(string scope)
        {
            var lis = Factory.FBL.Sys_GetList_SysObjects();
            if (scope == "_core")
            {
                lis = lis.Where(p => p.Name.StartsWith("_core"));
            }
            Factory.FBL.Sys_GenerateCreateUpdateScript(lis);

            return new BO.Result(false, "Soubor byl vygenerován (do TEMPu)");
        }

        private myGridInput GetGridInput(string entity, string prefix, int go2pid, List<string> viewstate, string myqueryinline)
        {
            string strMyQueryInline = null;

            if (!string.IsNullOrEmpty(myqueryinline))
            {
                if (strMyQueryInline == null)
                {
                    strMyQueryInline = myqueryinline;
                }
                else
                {
                    strMyQueryInline += "|" + myqueryinline;
                }

            }

            var gi = new myGridInput();
            //gi.controllername = "Admin";
            gi.entity = entity;
            gi.go2pid = go2pid;
            gi.ondblclick = "handle_dblclick";
            gi.myqueryinline = strMyQueryInline;

            gi.query = new BO.InitMyQuery(Factory.CurrentUser).Load(prefix, null, 0, strMyQueryInline);
            gi.query.IsRecordValid = null;

            gi.j72id = Factory.CBL.LoadUserParamInt("Admin/" + prefix + "-j72id");

            

            return gi;
        }




    }
}