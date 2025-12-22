using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections;
using System.Diagnostics;
using UI.Code;
using UI.Models;
using UI.Models.Home;

namespace UI.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IWebHostEnvironment _env;
        private readonly BL.TheColumnsProvider _colsProvider;
        public HomeController(IWebHostEnvironment env, BL.TheColumnsProvider cp)
        {
            _env = env;
            _colsProvider = cp;
        }

        
       

        public IActionResult Index()
        {

            if (HttpContext.Request.Path.Value.Length <= 5)
            {
                //úvodní spuštění: otestovat nastavení domovské stránky
                if (basUI.DetectIfMobileFromUserAgent(Request))
                {
                    string s = Factory.CBL.LoadUserParam("j02-homepageurl-mobile");
                    if (!string.IsNullOrEmpty(s))
                    {
                        return Redirect(s);  //pryč na jinou stránku
                    }
                    return Redirect("/Home/Mobile");  //stránky pro mobilní zařízení
                }

                

                if (!string.IsNullOrEmpty(Factory.CurrentUser.j02HomePageUrl))
                {

                    return Redirect(Factory.CurrentUser.j02HomePageUrl);  //pryč na jinou stránku
                }

                if (Factory.CurrentUser.IsPortalUserOnly())
                {
                    return Redirect("/Widgets/Index");
                }
            }

            
            return View(new BaseViewModel());
        }
        
        [Route("[controller]/[action]")]
        [Route("mobile")]
        public IActionResult Mobile()
        {
            

            return View(new BaseViewModel());
        }

        public IActionResult About()
        {



            return View(new BaseViewModel());
        }
        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync("Identity.Application");

            return View(new BaseViewModel());

        }

      
        public IActionResult MySearchSetting()
        {
            var v = new BaseViewModel();
            return View(v);
        }

        public IActionResult ChangePassword()
        {
            var v = new ChangePasswordViewModel();
            if (Factory.CurrentUser.j02IsMustChangePassword)
            {
                this.AddMessage("Administrátor nastavil, že si musíte změnit přihlašovací heslo.", "info");
            }
            return View(v);
        }
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel v)
        {
            if (string.IsNullOrEmpty(v.NewPassword) || string.IsNullOrEmpty(v.VerifyPassword))
            {
                this.AddMessageTranslated("???");
                return View(v);
            }
            var cPwdSupp = new BL.Code.PasswordSupport();
            var res = cPwdSupp.CheckPassword(v.NewPassword);
            if (res.Flag == BO.ResultEnum.Failed)
            {
                this.AddMessage(res.Message); return View(v);
            }
            if (v.NewPassword != v.VerifyPassword)
            {
                this.AddMessage("Heslo nesouhlasí s jeho ověřením."); return View(v);
            }



            res = cPwdSupp.VerifyUserPassword(v.CurrentPassword, Factory.CurrentUser.j02Login, Factory.CurrentUser);
            if (res.Flag == BO.ResultEnum.Success)
            {
                
                if (Factory.j02UserBL.SaveNewPassword(Factory.CurrentUser.pid, v.NewPassword,false).Flag == BO.ResultEnum.Success)
                {
                    var recJ02 = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
                    recJ02.j02IsMustChangePassword = false;
                    Factory.j02UserBL.Save(recJ02,null);
                    this.AddMessage("Heslo bylo změněno.", "info");
                    return RedirectToAction("Index");
                }

            }
            else
            {
                this.AddMessage(res.Message);
            }
            return View(v);

        }

        //public IActionResult MyMainMenuLinks(string newurl,string newname,string href)
        //{
           
        //    var v = new MyMainMenuLinksViewModel() { ResultValue = Factory.CurrentUser.j02MyMenuLinks };
        //    v.lisLinks = new List<MenuItemMyLink>();
        //    var cmenu = new UI.Menu.TheMenuSupport(Factory);

        //    if (Factory.CurrentUser.j02MyMenuLinks != null)
        //    {
        //        v.lisLinks = cmenu.getMyMenuLinks();
        //    }
        //    if (!string.IsNullOrEmpty(newurl))
        //    {
        //        newurl = HttpUtility.UrlDecode(newurl).Replace("**", "&");
        //        if (newurl.Contains("p31calendar") || newurl.Contains("p31dayline") || newurl.Contains("widgets"))  //ořezat querystring za otazníkem
        //        {
        //            newurl = newurl.Split("?")[0];
        //        }
               
        //        string strID = cmenu.TryEstimateMyLinkID(newurl);
                
        //        v.lisLinks.Add(new MenuItemMyLink() {IsJustNew=true, Url = newurl,Name=newname,Ordinary=v.lisLinks.Count()+1,TempGuid=BO.Code.Bas.GetGuid(),ID=strID});
        //    }

        //    return View(v);

        //}
        //[HttpPost]
        //public IActionResult MyMainMenuLinks(MyMainMenuLinksViewModel v, string guid)
        //{
        //    if (v.lisLinks == null)
        //    {
        //        v.lisLinks = new List<MenuItemMyLink>();
        //    }
        //    if (v.IsPostback)
        //    {
        //        if (v.PostbackOper == "add")
        //        {
        //            var c = new MenuItemMyLink() { TempGuid = BO.Code.Bas.GetGuid(),Ordinary=v.lisLinks.Count()+1 };
        //            v.lisLinks.Add(c);

        //        }
        //        if (v.PostbackOper == "delete")
        //        {
        //            v.lisLinks.First(p => p.TempGuid == guid).IsTempDeleted = true;

        //        }

        //        return View(v);
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        if (v.lisLinks.Where(p=>p.IsTempDeleted==false && string.IsNullOrEmpty(p.Name)).Count() > 0)
        //        {
        //            this.AddMessage("Minimálně v jednom odkazu chybí vyplnit název.");return View(v);
        //        }
        //        if (v.lisLinks.Where(p => p.IsTempDeleted == false && string.IsNullOrEmpty(p.Url)).Count() > 0)
        //        {
        //            this.AddMessage("Minimálně v jednom odkazu chybí vyplnit Url."); return View(v);
        //        }
        //        if (v.lisLinks.Where(p=>p.IsTempDeleted==false).GroupBy(p => p.Url).Any(p=>p.Count()>1))
        //        {
        //            this.AddMessage("V seznamu nesmí být odkazy s duplicitní url adresou."); return View(v);
        //        }
        //        if (v.lisLinks.Where(p => p.IsTempDeleted == false).GroupBy(p => p.Name.ToLower()).Any(p => p.Count() > 1))
        //        {
        //            this.AddMessage("V seznamu nesmí být odkazy s duplicitním názvem."); return View(v);
        //        }
        //        var lis = new List<string>();int x = 1;
        //        foreach (var c in v.lisLinks.Where(p => p.IsTempDeleted == false).OrderBy(p=>p.Ordinary))
        //        {                    
        //            lis.Add(c.Name + "|" + c.Url + "|" + c.Target + "|"+c.ID);
        //            x += 1;
        //        }

        //        var recJ02 = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
        //        recJ02.j02MyMenuLinks = String.Join("**", lis);
        //        if (!string.IsNullOrEmpty(recJ02.j02MyMenuLinks) && recJ02.j02MyMenuLinks.Length > 400)
        //        {
        //            this.AddMessage("Nelze uložit tolik odkazů. Uberte."); return View(v);
        //        }
                
        //        Factory.j02UserBL.Save(recJ02, null);

        //        v.SetJavascript_CallOnLoad(0);
        //        return View(v);

        //    }


        //    this.Notify_RecNotSaved();
        //    return View(v);
        //}

        public IActionResult MyProfile()
        {


            var v = new MyProfileViewModel() { RecJ02 = Factory.j02UserBL.Load(Factory.CurrentUser.pid) };
            

            RefreshState_MyProfile(v);

            return this.ViewTup(v, BO.PermValEnum.GR_MyProfile);

        }
        private void RefreshState_MyProfile(MyProfileViewModel v)
        {
            v.lisJ40 = Factory.j40MailAccountBL.GetList(new BO.myQueryJ40() { IsRecordValid = null, j02id = Factory.CurrentUser.pid });

           

            if (!string.IsNullOrEmpty(Factory.CurrentUser.j11IDs))
            {
                var mq = new BO.myQueryJ11() { j02id = Factory.CurrentUser.pid };
                v.Teams = string.Join(", ", Factory.j11TeamBL.GetList(mq).Select(p => p.j11Name));
            }

            v.userAgent = Request.Headers["User-Agent"];
            var uaParser = UAParser.Parser.GetDefault();
            v.client_info = uaParser.Parse(v.userAgent);

            v.SearchboxTopRecs = Factory.CBL.LoadUserParamInt("searchbox-toprecs", 50);

            v.SearchboxJ02 = Factory.CBL.LoadUserParam("searchbox-j02", Factory.j72TheGridTemplateBL.getDefaultPalleteSearchbox("j02User"));
            v.lisSearchboxJ02 = _colsProvider.ParseTheGridColumns("j02", v.SearchboxJ02, Factory);

            v.SearchboxP41 = Factory.CBL.LoadUserParam("searchbox-p41", Factory.j72TheGridTemplateBL.getDefaultPalleteSearchbox("p41Project"));
            v.lisSearchboxP41 = _colsProvider.ParseTheGridColumns("p41", v.SearchboxP41, Factory);

            v.SearchboxP28 = Factory.CBL.LoadUserParam("searchbox-p28", Factory.j72TheGridTemplateBL.getDefaultPalleteSearchbox("p28Contact"));
            v.lisSearchboxP28 = _colsProvider.ParseTheGridColumns("p28", v.SearchboxP28, Factory);

            v.SearchboxP56 = Factory.CBL.LoadUserParam("searchbox-p56", Factory.j72TheGridTemplateBL.getDefaultPalleteSearchbox("p56Task"));
            v.lisSearchboxP56 = _colsProvider.ParseTheGridColumns("p56", v.SearchboxP56, Factory);

            v.SearchboxP32 = Factory.CBL.LoadUserParam("searchbox-p32", Factory.j72TheGridTemplateBL.getDefaultPalleteSearchbox("p32Activity"));
            v.lisSearchboxP32 = _colsProvider.ParseTheGridColumns("p32", v.SearchboxP32, Factory);

            v.SearchboxP91 = Factory.CBL.LoadUserParam("searchbox-p91", Factory.j72TheGridTemplateBL.getDefaultPalleteSearchbox("p91Invoice"));
            v.lisSearchboxP91 = _colsProvider.ParseTheGridColumns("p91", v.SearchboxP91, Factory);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MyProfile(MyProfileViewModel v, string oper,string bgcolor,string selcolor,string forecolor)
        {
            RefreshState_MyProfile(v);
            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "resetmenu":
                        var c = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
                        c.j02MyMenuLinks = Factory.j02UserBL.GetDefaultMenuLinks();
                        Factory.j02UserBL.Save(c, null);
                        return RedirectToAction("MyProfile");
                    case "clearmenu":
                        var cc = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
                        cc.j02MyMenuLinks = null;
                        Factory.j02UserBL.Save(cc, null);
                        return RedirectToAction("MyProfile");

                }
                return View(v);
            }
            if (oper == "clearparams")
            {
                Factory.j02UserBL.TruncateUserParams(0);
                this.AddMessage("Server cache vyčištěna.", "info");
                return MyProfile();
            }
            if (oper == "skin")
            {
                v.RecJ02.j02SkinBackColor = "#"+bgcolor;
                v.RecJ02.j02SkinSelColor = "#"+selcolor;
                v.RecJ02.j02SkinForeColor = "#"+forecolor;
                Factory.CurrentUser.j02SkinBackColor = v.RecJ02.j02SkinBackColor;
                Factory.CurrentUser.j02SkinSelColor = v.RecJ02.j02SkinSelColor;
                Factory.CurrentUser.j02SkinForeColor = v.RecJ02.j02SkinForeColor;

                this.AddMessage("Změna barevné kombinace je pouze náhled. Změnu barev je třeba potvrdit tlačítkem [Uložit změny].", "info");
                
                return View(v);
            }
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(v.RecJ02.j02Email))
                {
                    this.AddMessage("Chybí e-mail adresa.");
                    return View(v);
                }
                var c = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
                c.j02Email = v.RecJ02.j02Email;
                c.j02Mobile = v.RecJ02.j02Mobile;
                c.j02EmailSignature = v.RecJ02.j02EmailSignature;
                c.j02ModalWindowsFlag = (BO.j02ModalWindowsFlagENUM)v.RecJ02.j02ModalWindowsFlag;
                
                c.j02SkinBackColor = v.RecJ02.j02SkinBackColor;
                c.j02SkinForeColor = v.RecJ02.j02SkinForeColor;
                c.j02SkinSelColor = v.RecJ02.j02SkinSelColor;

                c.j02NotifySubscriberFlag = v.RecJ02.j02NotifySubscriberFlag;
                c.j02AutocompleteFlag = v.RecJ02.j02AutocompleteFlag;

                if (Factory.j02UserBL.Save(c, null) > 0)
                {
                    Factory.InhaleUserByLogin(c.j02Login);
                    this.AddMessage("Změny uloženy", "info");
                    return View(v);
                }

            }

            this.Notify_RecNotSaved();
            return View(v);

        }

        public IActionResult SmsVerify()
        {

            var v = new SmsVerifyViewModel();
            v.recJ02 = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            return View(v);
        }
        [HttpPost]
        public IActionResult SmsVerify(Models.Home.SmsVerifyViewModel v, string returnurl)
        {
            v.recJ02 = Factory.j02UserBL.Load(Factory.CurrentUser.pid);

            if (Factory.CurrentUser.j02SmsVerifyCode == null)
            {
                this.AddMessage("V systému neevidujeme požadavek zadat SMS kód."); return View(v);
            }
            if (string.IsNullOrEmpty(v.SmsCode))
            {
                this.AddMessage("Musíte zadata SMS kód."); return View(v);
            }

            if (v.SmsCode == Factory.CurrentUser.j02SmsVerifyCode)
            {
                Factory.j02UserBL.ClearSmsVerifyCode(Factory.CurrentUser.pid);
                if (string.IsNullOrEmpty(returnurl) || returnurl.Length < 3)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Redirect(returnurl);
                }
            }
            else
            {
                this.AddMessage("Chybný SMS kód.");
            }
            return View(v);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            var v = new ErrorViewModel() { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            
            var errFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (errFeature != null)
            {
                v.Error = errFeature.Error;
            }

            

            var path = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (path != null)
            {
                v.OrigFullPath = path.Path;
            }



            var statusFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusFeature != null)
            {
                v.OrigFullPath = statusFeature.OriginalPath;

            }

            v.OrigFullPath += HttpContext.Request.QueryString;

            return View(v);
        }

        public int SendError(string stack,string message,string url)
        {
            var c = new BO.p85Tempbox() { p85Message = stack, p85FreeText01 = message,p85Prefix="error",p85FreeText02=Factory.CurrentUser.j02Login,p85FreeText03=Factory.CurrentUser.FullNameAsc,p85FreeText04=Factory.CurrentUser.j02Email,p85FreeText05=url,p85FreeText06=Factory.Lic.x01Name };

            return Factory.p85TempboxBL.Save(c);


        }



        public BO.Result AppendMenuLink(string menukey)
        {
            return Handle_AppendRemoveMenuLink(menukey, true);            
        }
        public BO.Result RemoveMenuLink(string menukey)
        {
            return Handle_AppendRemoveMenuLink(menukey, false);
        }

        private BO.Result Handle_AppendRemoveMenuLink(string menukey,bool bolAppend)
        {
            var s = Factory.CurrentUser.j02MyMenuLinks;
            var rec = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            if (bolAppend)
            {
                if (s == null)
                {
                    s = menukey;
                }
                else
                {
                    var arr = BO.Code.Bas.ConvertString2List(s, "|");
                    if (arr.Contains(menukey))
                    {
                        return new BO.Result(true, "duplicity");
                    }
                    s = s + "|" + menukey;
                }
            }
            else
            {
                if (s == null)
                {
                    return new BO.Result(true, "menu is empty");
                }
                var arr = BO.Code.Bas.ConvertString2List(s, "|");
                if (!arr.Contains(menukey))
                {
                    return new BO.Result(true, "??");
                }
                arr.Remove(menukey);
                s = String.Join("|", arr);
            }
            

            rec.j02MyMenuLinks = s;
            if (Factory.j02UserBL.Save(rec, null) > 0)
            {
                return new BO.Result(false);
            }
            else
            {
                return new BO.Result(true, "error");
            }
        }

        public BO.Result SaveCurrentUserHomePage(string homepageurl,string device)
        {
            
            if (!String.IsNullOrEmpty(homepageurl))
            {
                if (homepageurl.Substring(0, 1) != "/")
                {
                    homepageurl = "/" + homepageurl;
                }
                if (homepageurl.Contains("/RecPage"))                    
                {   //ořezat parametry za otazníkem
                    var uri = new Uri(new Uri("http://dummy"), homepageurl);
                    var query = QueryHelpers.ParseQuery(uri.Query);
                    var prefix = query.TryGetValue("prefix", out var value)? value.ToString(): null;
                    homepageurl = homepageurl.Split("?")[0] + "?prefix=" + prefix;
                }
                else
                {
                    homepageurl = homepageurl.Replace("pid=", "xxx=");
                }
            }
            else
            {
                //vyčištění homepage stránky
            }
            
            if (device == "Phone")
            {
                Factory.CBL.SetUserParam("j02-homepageurl-mobile",homepageurl); //mobilní homepage se ukládá do userparams
            }
            else
            {
                
                var c = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
                c.j02HomePageUrl = homepageurl;
                Factory.j02UserBL.Save(c, null);
            }
            
            return new BO.Result(false);
        }

        public BO.Result LiveChat(bool zapnout)
        {
            var c = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            if (zapnout)
            {
                c.j02LiveChatTimestamp = DateTime.Now;
            }
            else
            {
                c.j02LiveChatTimestamp = null;
            }            
            if (Factory.j02UserBL.Save(c, null) > 0)
            {
                return new BO.Result(false);
            }
            else
            {
                return new BO.Result(true);
            }
            
        }

        public BO.Result SaveMySearchSetting(int intMySearchBitStream)
        {
            var rec = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            rec.j02MySearchBitStream = intMySearchBitStream;
            Factory.j02UserBL.Save(rec, null);
            return new BO.Result(false);
        }
        public BO.Result SaveCurrentUserUI_LeftPanel(bool leftpanel,string prefix)
        {
           

            BL.Code.UserUI.SaveLeftPanel(Factory, prefix, leftpanel);

            
            
            return new BO.Result(false);
        }
        public BO.Result SaveCurrentUserUI_FlatView(bool flatview, string prefix)
        {
            
            BL.Code.UserUI.SaveFlatView(Factory, prefix, flatview);
            
            return new BO.Result(false);
        }
        public BO.Result SaveCurrentUserTableLayout(bool grid,string layoutname,bool zebra,bool mrizka,string lineheight)
        {
            int x = 0;
            if (layoutname == "resize") x += (grid ? 2: 1024);
            if (layoutname == "fixed") x += (grid ? 4: 2048);
            if (layoutname == "auto") x += (grid ? 8 : 4096);
            if (zebra) x += (grid ? 16 : 8192);
            if (mrizka) x += (grid ? 32 : 16384);
            if (lineheight == "lh15" && grid) x += 256;
            if (lineheight == "lh15" && !grid) x += 131072;
            if (lineheight == "lh20" && grid) x += 512;
            if (lineheight == "lh20" && !grid) x += 262144;

            var c = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            c.j02GridCssBitStream = x;
            Factory.j02UserBL.Save(c, null);
            return new BO.Result(false);
        }
        public BO.Result SaveCurrentUserModalWindowsFlag(int flag)
        {
            var c = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            c.j02ModalWindowsFlag = (BO.j02ModalWindowsFlagENUM) flag;
            Factory.j02UserBL.Save(c, null);
            return new BO.Result(false);
        }
        public BO.Result SaveCurrentUserFontSize(int fontsize)
        {
            var c = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            c.j02FontSizeFlag = fontsize;
            Factory.j02UserBL.Save(c, null);
            return new BO.Result(false);
        }
        public BO.Result SaveCurrentUserLangIndex(int langindex)
        {
            var c = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            c.j02LangIndex = langindex;
            Factory.j02UserBL.Save(c, null);
            var co = new CookieOptions() { Expires = DateTime.Now.AddDays(100) };
            Response.Cookies.Append("marktime.langindex", langindex.ToString(), co);
            return new BO.Result(false);
        }

        public BO.Result UpdateNews_Timestamp()
        {
            Factory.j02UserBL.UpdateLastReadNews_Timestamp(Factory.CurrentUser.pid);
            return new BO.Result(false);
        }
        public BO.Result UpdateCurrentUserPing(BO.j92PingLog c)
        {
                       
            var uaParser = UAParser.Parser.GetDefault();
            UAParser.ClientInfo client_info = uaParser.Parse(c.j92BrowserUserAgent);
            c.j92BrowserOS = client_info.OS.Family + " " + client_info.OS.Major;
            c.j92BrowserFamily = client_info.UA.Family + " " + client_info.UA.Major;
            c.j92BrowserDeviceFamily = client_info.Device.Family;
            
            Factory.j02UserBL.UpdateCurrentUserPing(c);

            return new BO.Result(false);
        }


        //níže je pouze testování HTML editoru

        public ActionResult upload_file(List<IFormFile> files)
        {

            return null;

        }

        public ActionResult upload_image(List<IFormFile> files)
        {

            return null;

        }

        [HttpPost("UploadFiles")]
        [Produces("application/json")]
        public async Task<IActionResult> Post(List<IFormFile> files)
        {
            // Get the file from the POST request

            var theFile = HttpContext.Request.Form.Files.GetFile("file");

            string strO27Guid = HttpContext.Request.Form["o27guid"];

            System.IO.File.WriteAllText("c:\\temp\\hovado.txt", "files count: " + HttpContext.Request.Form.Files.ToString());


            // Get the server path, wwwroot
            string webRootPath = _env.WebRootPath;

            // Building the path to the uploads directory
            string strUploadRoot = Path.Combine(webRootPath, "froala_upload");

            // Get the mime type
            var mimeType = theFile.ContentType;

            // Get File Extension
            string extension = System.IO.Path.GetExtension(theFile.FileName);

            // Generate Random name.
            string name = strO27Guid + BO.Code.Bas.GetGuid().Substring(0, 20) + extension;


            // Build the full path inclunding the file name
            string link = Path.Combine(strUploadRoot, name);


            // Create directory if it does not exist.
            FileInfo dir = new FileInfo(strUploadRoot);
            dir.Directory.Create();



            try
            {
                // Copy contents to memory stream.
                Stream stream;
                stream = new MemoryStream();
                theFile.CopyTo(stream);
                stream.Position = 0;
                String serverPath = link;



                // Save the file
                using (FileStream writerFileStream = System.IO.File.Create(serverPath))
                {
                    await stream.CopyToAsync(writerFileStream);
                    writerFileStream.Dispose();
                }

                // Return the file path as json
                Hashtable fileUrl = new Hashtable();
                fileUrl.Add("link", "/froala_upload/" + name);

                return Json(fileUrl);


            }

            catch (ArgumentException ex)
            {
                return Json(ex.Message);
            }


           
        }

        public IActionResult Bells()
        {
            return View(new BaseViewModel());
        }

       
        
    }
}