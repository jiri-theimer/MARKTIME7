using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UI.Code;
using UI.Models;
using UI.Models.p31view;
using UI.Models.Tab1;
using UI.Models.Widgets;

namespace UI.Controllers
{
    public class WidgetsController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private readonly BL.TheColumnsProvider _colsProvider;
        private readonly BL.AI.Core.ILlmClient _llm;
        public WidgetsController(BL.Singleton.ThePeriodProvider pp, BL.TheColumnsProvider cp, BL.AI.Core.ILlmClient llm)
        {
            _pp = pp;
            _colsProvider = cp;
            _llm = llm;
        }

        /// <summary>Virtuální skupina „Moje AI widgety“ – není v DB; plocha = všechny osobní AI widgety uživatele.</summary>
        public const int AiX54ID = -1;
        private const string AiDockStateParam = "Widgets-AiDockState";

        private BO.x54WidgetGroup VirtualAiGroup() => new BO.x54WidgetGroup()
        {
            pid = AiX54ID,
            x54Name = "✨ " + Factory.tra("Moje AI widgety"),
            x54IsAllowAutoRefresh = true
        };

        public IActionResult MiniAbsence(string d0)
        {
            if (string.IsNullOrEmpty(d0))
            {
                d0 = DateTime.Today.ToString("dd.MM.yyyy");
            }
            var v = new MiniAbsenceViewModel() {d0=BO.Code.Bas.String2Date(d0),j02ID=Factory.CurrentUser.pid };

            var lisP11 = Factory.p11AttendanceBL.GetList(v.d0, v.d0, v.j02ID, false);
            if (lisP11.Any(p => p.j02ID == v.j02ID && p.p11Date == v.d0))
            {
                var recP11 = lisP11.Where(p => p.j02ID == v.j02ID && p.p11Date == v.d0).First();
                v.Prichod = recP11.Prichod;
                v.Odchod = recP11.Odchod;
            }

            v.lisP32FUT = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { isabsence = true });
            v.lisP11 = Factory.p11AttendanceBL.GetList(new DateTime(v.d0.Year,v.d0.Month,1), new DateTime(v.d0.Year, v.d0.Month, 1).AddMonths(1).AddDays(-1), v.j02ID, false);
            v.lisP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31() { j02id = v.j02ID,global_d1=v.d0,global_d2=v.d0 });

            return View(v);
        }
        [HttpPost]
        public IActionResult MiniAbsence(MiniAbsenceViewModel v)
        {
            
            if (v.IsPostback)
            {
                
                return View(v);
            }
            if (ModelState.IsValid)
            {
                
                return View(v);

            }


            
            return View(v);
        }

        public IActionResult MiniLeftPanel()
        {
            var v = new BaseViewModel();
            return View(v);
            
        }

        public IActionResult MiniCalendar()
        {
            var v = new BaseViewModel();
            return View(v);
        }
        public IActionResult Index(string d,int j02id, int x54id, int p28id)
        {
            

            if (!Factory.CurrentUser.j04IsModule_Widgets)
            {
                if (Factory.CurrentUser.j04IsModule_p31)
                {
                    return Redirect("/p31calendar/Index");
                }
                else
                {
                    return Redirect("/Portal/Index");
                }
                

            }
            
            if (HttpContext.Request.Path.Value.Length <= 5)
            {
                //automatické spuštění stránky po přihlášení nebo bez uvedení URL adresy
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
            }

            if (!Factory.CurrentUser.j04IsModule_Widgets)
            {
                return Redirect("/Home/Index");
            }


            
            
            return Widgets(d,j02id,x54id,p28id);
        }

        public IActionResult Widgets(string d,int j02id,int x54id,int p28id)
        {
            
            var v = new WidgetsViewModel() { IsSubform = false, d0 = DateTime.Today,j02id_me=Factory.CurrentUser.pid,Skin= "index" };
            if (!string.IsNullOrEmpty(d))
            {
                v.d0 = BO.Code.Bas.String2Date(d);
            }
            if (j02id > 0)
            {
                v.j02id_me = j02id;
            }
            int intJ04ID = Factory.CurrentUser.j04ID;
            if (v.j02id_me != Factory.CurrentUser.pid)
            {
                intJ04ID = Factory.j02UserBL.Load(v.j02id_me).j04ID;
            }

            v.lisX54 = Factory.j04UserRoleBL.GetList_x54(intJ04ID).Where(p => !p.isclosed);
            //virtuální skupina osobních AI widgetů – na první místo, jen na vlastní ploše a jen když má smysl (AI zapnuté nebo už nějaký widget má)
            if (v.j02id_me == Factory.CurrentUser.pid && (_llm.Options.Enabled || Factory.x55WidgetBL.CountPersonal(Factory.CurrentUser.pid) > 0))
            {
                v.lisX54 = new[] { VirtualAiGroup() }.Concat(v.lisX54).ToList();
            }
            if (v.lisX54.Count() == 0)
            {
                return this.StopPage(false, "Vaše aplikační role nemá nastavenou widget skupinu.", "/Admin/Page?area=widgets&prefix=x54");
            }
            v.SelectedX54ID = x54id;
            if (v.SelectedX54ID == 0)
            {
                v.SelectedX54ID = Factory.CBL.LoadUserParamInt("Widgets-x54ID");
            }            
            if (v.SelectedX54ID==0 || v.lisX54.Where(p => p.pid == v.SelectedX54ID).Count() == 0)
            {
                v.SelectedX54ID = v.lisX54.First().pid;
            }
            v.RecX54 = v.SelectedX54ID == AiX54ID ? VirtualAiGroup() : Factory.x54WidgetGroupBL.Load(v.SelectedX54ID);
            v.IsAiGroup = v.SelectedX54ID == AiX54ID;
            v.IsAiEnabled = _llm.Options.Enabled;
                        
            if (v.RecX54.x54IsParamP28ID)
            {
                v.SelectedP28ID = p28id;
                if (v.SelectedP28ID == 0)
                {
                    v.SelectedP28ID = Factory.CBL.LoadUserParamInt("Widgets-p28ID");
                }
                v.IsAllScopeAllClients = Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p28_Owner);
                if (!v.IsAllScopeAllClients)
                {
                    v.lisP28 = Factory.p28ContactBL.GetList(new BO.myQueryP28() { isportal = true });
                    
                    if (v.SelectedP28ID == 0 && v.lisP28.Count() > 0)
                    {
                        v.SelectedP28ID = v.lisP28.First().pid;                       
                    }
                }
                else
                {
                    if (v.SelectedP28ID > 0)
                    {
                        v.SelectedP28Name = Factory.p28ContactBL.Load(v.SelectedP28ID).p28Name;
                    }
                }
            }
            if (v.RecX54.x54IsAllowSkins)
            {
                v.Skin = Factory.CBL.LoadUserParam($"Widgets-Skin-{v.SelectedX54ID}", "index");
            }
            
            if (string.IsNullOrEmpty(v.Skin))
            {
                v.Skin = "index";
            }

            v.lisJ02 = Factory.j02UserBL.GetList(new BO.myQueryJ02() { explicit_orderby="a.j02Name",MyRecordsDisponible=true});

            PrepareWidgets(v);

            return View(v);
        }

        public BO.Result ChangeX54ID(int x54id)
        {
            Factory.CBL.SetUserParam("Widgets-x54ID", x54id.ToString());
            return new BO.Result(false);
        }
        public BO.Result ChangeP28ID(int p28id)
        {
            Factory.CBL.SetUserParam("Widgets-p28ID", p28id.ToString());
            return new BO.Result(false);
        }

        public BO.Result ChangeSkin(string skin,int x54id)
        {
            Factory.CBL.SetUserParam($"Widgets-Skin-{x54id}", skin);
            return new BO.Result(false);
        }




        private string parse_badge(int intCount)
        {
            if (intCount > 0)
            {
                return intCount.ToString();
            }
            return null;
        }

        public BO.Result SaveWidgetState(string s, string skin,int x54id)
        {
            if (x54id == AiX54ID)
            {
                Factory.CBL.SetUserParam(AiDockStateParam, s);
                return new BO.Result(false);
            }
            var rec = Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id);
            rec.x56DockState = s;
            rec.x56Skin = skin;
            Factory.x55WidgetBL.SaveState(rec);
            return new BO.Result(false);
        }
        public BO.Result RemoveWidget(int x55id, string skin,int x54id)
        {
            if (x54id == AiX54ID) return new BO.Result(false);     //virtuální plocha ukazuje vždy všechny osobní widgety – odebrání = smazání (AiDelete)
            var recX55 = Factory.x55WidgetBL.Load(x55id);
            var recX56 = Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id);
            var boxes = BO.Code.Bas.ConvertString2List(recX56.x56Boxes);
            if (boxes.Where(p => p == recX55.x55Code).Count() > 0)
            {
                boxes.Remove(recX55.x55Code);
                recX56.x56Boxes = string.Join(",", boxes);
                Factory.x55WidgetBL.SaveState(recX56);
                return new BO.Result(false);
            }

            return new BO.Result(true, "widget not found");
        }
        public BO.Result InsertWidget(int x55id, string skin, int x54id)
        {
            if (x54id == AiX54ID) return new BO.Result(false);
            var recX55 = Factory.x55WidgetBL.Load(x55id);
            var recX56 = Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id);
            var boxes = BO.Code.Bas.ConvertString2List(recX56.x56Boxes);
            if (boxes.Where(p => p == recX55.x55Code).Count() == 0)
            {
                boxes.Add(recX55.x55Code);
                recX56.x56Boxes = string.Join(",", boxes);
                Factory.x55WidgetBL.SaveState(recX56);
                return new BO.Result(false);
            }
            return new BO.Result(true, "widget not found");
        }
        
        public BO.Result SavePocetSloupcu(int x, string skin,int x54id)
        {
            
            Factory.CBL.SetUserParam($"Widgets-ColumnsPerPage-{skin}-{x54id}", x.ToString());
            return new BO.Result(false);
        }
        public BO.Result SavePageAutoRefresh(int x, string skin,int x54id)
        {
            Factory.CBL.SetUserParam($"Widgets-PageAutoRefresh-{skin}-{x54id}", x.ToString());
            return new BO.Result(false);
        }

        public BO.Result Clear2FactoryState(string skin,int x54id)    //vyčistí plochu do továrního nastavení
        {
            if (x54id == AiX54ID) { Factory.CBL.SetUserParam(AiDockStateParam, ""); return new BO.Result(false); }
            Factory.x55WidgetBL.Clear2FactoryState(Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id));
            return new BO.Result(false);
        }

        public BO.Result SetAsDefaultGlobalBoxes(string skin,int x54id)    //vyčistí plochu do továrního nastavení
        {
            if (x54id == AiX54ID) return new BO.Result(true, "Not supported for personal AI widgets.");
            Factory.x55WidgetBL.SetAsDefaultGlobalBoxes(Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id).x56Boxes);
            return new BO.Result(false);
        }

        public BO.Result UpdateBoxes2AllUsers(string skin,int x54id)    //vyčistí plochu do továrního nastavení
        {
            if (x54id == AiX54ID) return new BO.Result(true, "Not supported for personal AI widgets.");
            Factory.x55WidgetBL.UpdateBoxes2AllUsers(Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id).x56Boxes,skin,x54id);
            return new BO.Result(false);
        }


        // ------------------------------------------------------------------ AI widgety (osobní, z „Zeptej se MARKTIME“)

        public const int MaxPersonalAiWidgets = 20;

        private AiWidgetRenderer AiRenderer() =>
            new AiWidgetRenderer(Factory, _colsProvider, _pp, HttpContext.RequestServices.GetRequiredService<System.Text.Encodings.Web.HtmlEncoder>());

        /// <summary>Podklady pro dialog „Na plochu“ v AiAsk: název, druh, pojmenované období, počet osobních widgetů.</summary>
        [HttpPost]
        public IActionResult AiPinInfo(string guid)
        {
            if (!Factory.CurrentUser.j04IsModule_Widgets) return Json(new { ok = false, error = Factory.tra("Nemáte přístup k dashboardu.") });
            var spec = BL.AI.Ask.AiAskService.LoadSpec(Factory, guid);
            if (spec == null) return Json(new { ok = false, error = Factory.tra("Výsledek nebyl nalezen.") });
            var per = AiRenderer().MatchNamedPeriod(spec);
            return Json(new
            {
                ok = true,
                title = string.IsNullOrEmpty(spec.Title) ? Factory.tra("AI widget") : spec.Title,
                kind = spec is BO.AI.AiListSpec ? "list" : "agg",
                period = per == null ? null : new { pid = per.pid, name = per.PeriodName },
                count = Factory.x55WidgetBL.CountPersonal(Factory.CurrentUser.pid),
                limit = MaxPersonalAiWidgets
            });
        }

        /// <summary>Vytvoří osobní AI widget ze zadání tabulky AiAsk (guid v p85). Zobrazí se ve virtuální skupině „Moje AI widgety“.</summary>
        [HttpPost]
        public IActionResult AiCreate(string guid, string name, bool live, string view)
        {
            if (!Factory.CurrentUser.j04IsModule_Widgets) return Json(new { ok = false, error = Factory.tra("Nemáte přístup k dashboardu.") });
            var spec = BL.AI.Ask.AiAskService.LoadSpec(Factory, guid);
            if (spec == null) return Json(new { ok = false, error = Factory.tra("Výsledek nebyl nalezen.") });
            if (Factory.x55WidgetBL.CountPersonal(Factory.CurrentUser.pid) >= MaxPersonalAiWidgets)
                return Json(new { ok = false, error = string.Format(Factory.tra("Můžete mít nejvýše {0} osobních AI widgetů. Nějaký nejdřív smažte."), MaxPersonalAiWidgets) });

            name = (name ?? "").Trim();
            if (name.Length == 0) name = string.IsNullOrEmpty(spec.Title) ? Factory.tra("AI widget") : spec.Title;
            if (name.Length > 100) name = name.Substring(0, 100);

            var task = new BO.AI.AiWidgetTask
            {
                Kind = spec is BO.AI.AiListSpec ? "list" : "agg",
                View = AiView(spec is BO.AI.AiListSpec ? "list" : "agg", view),
                PeriodPid = live ? (AiRenderer().MatchNamedPeriod(spec)?.pid ?? 0) : 0,
                Agg = spec as BO.AI.AiAggSpec,
                List = spec as BO.AI.AiListSpec,
                Description = new BL.AI.Tools.AiAggregate(Factory, _colsProvider).Describe(spec)
            };

            var rec = new BO.x55Widget
            {
                x01ID = Factory.CurrentUser.x01ID,
                x55Name = name,
                x55Code = $"ai_{Factory.CurrentUser.pid}_{BO.Code.Bas.GetGuid().Substring(0, 8)}",
                x55Image = "auto_awesome",
                x55AiTask = task.ToJson(),
                j02ID_Owner = Factory.CurrentUser.pid
            };
            int x55id = Factory.x55WidgetBL.Save(rec);
            if (x55id == 0) return Json(new { ok = false, error = Factory.tra("Widget se nepodařilo uložit.") });

            Factory.CBL.SetUserParam("Widgets-x54ID", AiX54ID.ToString());     //po otevření dashboardu rovnou „Moje AI widgety“
            return Json(new { ok = true, x55id = x55id, url = $"/Widgets/Index?x54id={AiX54ID}" });
        }

        /// <summary>Zobrazení AI widgetu: výpis jen tabulka; souhrn table | both | chart (výchozí table).</summary>
        private static string AiView(string kind, string view) => kind == "list" ? "table" : (view == "both" || view == "chart" ? view : "table");

        private BO.x55Widget LoadMyAiWidget(int x55id)
        {
            var rec = Factory.x55WidgetBL.Load(x55id);
            return rec != null && rec.IsAiWidget && rec.IsPersonal && rec.j02ID_Owner == Factory.CurrentUser.pid ? rec : null;
        }

        /// <summary>Rychlá úprava (bez AI) – podklady: název, období (pojmenovaná / pevné), počet řádků.</summary>
        public IActionResult AiEditInfo(int x55id)
        {
            var rec = LoadMyAiWidget(x55id);
            var task = rec == null ? null : BO.AI.AiWidgetTask.Parse(rec.x55AiTask);
            if (task == null) return Json(new { ok = false, error = Factory.tra("Widget nebyl nalezen.") });
            var spec = task.Spec;
            string fixedText = null;
            if (spec.Period != null && BL.AI.Grid.AiGridFilterCodec.TryDate(spec.Period.D1, out var d1) && BL.AI.Grid.AiGridFilterCodec.TryDate(spec.Period.D2, out var d2))
                fixedText = $"{d1:d.M.yyyy} – {d2:d.M.yyyy}";
            int max = task.Kind == "list" ? AiWidgetRenderer.MaxListRows : AiWidgetRenderer.MaxAggRows;
            return Json(new
            {
                ok = true,
                x55id = rec.pid,
                name = rec.x55Name,
                hasPeriod = spec.Period != null,
                periodPid = task.PeriodPid,
                fixedText = fixedText,
                periods = _pp.getPallete().Where(p => p.pid > 1 && p.pid <= 60).Select(p => new { pid = p.pid, name = p.PeriodName, interval = p.PeriodInterval }),
                limit = Math.Clamp(spec.Limit <= 0 ? max : spec.Limit, 1, max),
                limitMax = max,
                kind = task.Kind,
                view = AiView(task.Kind, task.View),
                canAsk = _llm.Options.Enabled
            });
        }

        /// <summary>Rychlá úprava (bez AI): název, období, počet řádků. Přechod z pojmenovaného na pevné období zmrazí aktuální data.</summary>
        [HttpPost]
        public IActionResult AiUpdate(int x55id, string name, int periodPid, int limit, string view)
        {
            var rec = LoadMyAiWidget(x55id);
            var task = rec == null ? null : BO.AI.AiWidgetTask.Parse(rec.x55AiTask);
            if (task == null) return Json(new { ok = false, error = Factory.tra("Widget nebyl nalezen.") });

            name = (name ?? "").Trim();
            if (name.Length > 0) rec.x55Name = name.Length > 100 ? name.Substring(0, 100) : name;

            if (task.Spec.Period != null)
            {
                if (periodPid > 1 && periodPid <= 60 && _pp.getPallete().Any(p => p.pid == periodPid))
                {
                    task.PeriodPid = periodPid;
                    AiRenderer().ApplyPeriod(task);       //uložit i aktuální data – kdyby se později přepnulo na pevné
                }
                else if (periodPid == 0 && task.PeriodPid > 0)
                {
                    AiRenderer().ApplyPeriod(task);       //zmrazit data aktuálního pojmenovaného období
                    task.PeriodPid = 0;
                }
            }
            int max = task.Kind == "list" ? AiWidgetRenderer.MaxListRows : AiWidgetRenderer.MaxAggRows;
            task.Spec.Limit = Math.Clamp(limit, 1, max);
            task.View = AiView(task.Kind, view);

            rec.x55AiTask = task.ToJson();
            if (Factory.x55WidgetBL.Save(rec) == 0) return Json(new { ok = false, error = Factory.tra("Widget se nepodařilo uložit.") });
            return Json(new { ok = true });
        }

        /// <summary>Úprava v „Zeptej se MARKTIME“: nové zadání (guid tabulky v p85) nahradí zadání widgetu; název a místo na ploše zůstávají.</summary>
        [HttpPost]
        public IActionResult AiReplace(int x55id, string guid)
        {
            var rec = LoadMyAiWidget(x55id);
            var old = rec == null ? null : BO.AI.AiWidgetTask.Parse(rec.x55AiTask);
            if (old == null) return Json(new { ok = false, error = Factory.tra("Widget nebyl nalezen.") });
            var spec = BL.AI.Ask.AiAskService.LoadSpec(Factory, guid);
            if (spec == null) return Json(new { ok = false, error = Factory.tra("Výsledek nebyl nalezen.") });

            string kind = spec is BO.AI.AiListSpec ? "list" : "agg";
            var task = new BO.AI.AiWidgetTask
            {
                Kind = kind,
                View = AiView(kind, old.View),
                //bylo „vždy aktuální“ → zůstane, pokud nové období odpovídá pojmenovanému; jinak pevné
                PeriodPid = old.PeriodPid > 0 ? (AiRenderer().MatchNamedPeriod(spec)?.pid ?? 0) : 0,
                Agg = spec as BO.AI.AiAggSpec,
                List = spec as BO.AI.AiListSpec,
                Description = new BL.AI.Tools.AiAggregate(Factory, _colsProvider).Describe(spec)
            };
            rec.x55AiTask = task.ToJson();
            if (Factory.x55WidgetBL.Save(rec) == 0) return Json(new { ok = false, error = Factory.tra("Widget se nepodařilo uložit.") });
            Factory.CBL.SetUserParam("Widgets-x54ID", AiX54ID.ToString());
            return Json(new { ok = true, url = $"/Widgets/Index?x54id={AiX54ID}" });
        }

        /// <summary>Smaže osobní AI widget (jen vlastník) a odebere ho z aktuální plochy.</summary>
        [HttpPost]
        public BO.Result AiDelete(int x55id, string skin, int x54id)
        {
            var rec = Factory.x55WidgetBL.Load(x55id);
            if (rec == null || !rec.IsPersonal || rec.j02ID_Owner != Factory.CurrentUser.pid) return new BO.Result(true, Factory.tra("Widget nelze smazat."));
            RemoveWidget(x55id, skin, x54id);
            Factory.x55WidgetBL.DeletePersonal(x55id, Factory.CurrentUser.pid);
            return new BO.Result(false);
        }

        /// <summary>Otevře zadání AI widgetu v Pivot-Exploreru (uloží ho do p85 jako v AiAsk).</summary>
        public IActionResult AiPivot(int x55id)
        {
            var rec = Factory.x55WidgetBL.Load(x55id);
            if (rec == null || !rec.IsAiWidget || (rec.IsPersonal && rec.j02ID_Owner != Factory.CurrentUser.pid)) return this.StopPage(true, Factory.tra("Widget nebyl nalezen."));
            var task = BO.AI.AiWidgetTask.Parse(rec.x55AiTask);
            if (task?.Agg == null) return this.StopPage(true, Factory.tra("Pivot-Explorer je dostupný jen pro souhrnné AI widgety."));
            AiRenderer().ApplyPeriod(task);
            if (string.IsNullOrEmpty(task.Agg.Title)) task.Agg.Title = rec.x55Name;
            string guid = BL.AI.Ask.AiAskService.SaveSpec(Factory, task.Agg);
            return Redirect("/AiAsk/Pivot?guid=" + guid);
        }


        //rozvržení prostoru na ploše
        private void PrepareWidgets(WidgetsViewModel v)
        {
            var mq = new BO.myQuery("x55") {x54id=v.SelectedX54ID, IsRecordValid = true, MyRecordsDisponible = true, CurrentUser = Factory.CurrentUser };
            var hodnoty = new List<string>() { null, v.Skin };

            if (v.IsAiGroup)
            {
                v.lisAllWidgets = Factory.x55WidgetBL.GetList_Personal(Factory.CurrentUser.pid).ToList();   //osobní AI widgety (nejsou ve skupinách x57)
            }
            else
            {
                v.lisAllWidgets = Factory.x55WidgetBL.GetList(mq).Where(p => !p.IsPersonal).OrderBy(p => p.x55Category).ThenBy(p => p.x55Ordinal);
            }
            
            string ss = null;
            foreach(var c in v.lisAllWidgets)
            {                
                if (c.x55Category != ss)
                {
                    c.Rezerva = c.x55Category;
                }
                ss = c.x55Category;
            }

            v.lisUserWidgets = new List<BO.x55Widget>();
            if (Factory.CurrentUser.IsMobileDisplay())
            {
                v.ColumnsPerPage = 1;
                v.PageAutoRefreshPerSeconds = 0;
                Factory.CurrentUser.j02GridCssBitStream = 564;   //napevno grid nastavení
            }
            else
            {
                v.ColumnsPerPage = Factory.CBL.LoadUserParamInt($"Widgets-ColumnsPerPage-{v.Skin}-{v.SelectedX54ID}", 2);
                if (v.RecX54.x54IsAllowAutoRefresh)
                {
                    v.PageAutoRefreshPerSeconds = Factory.CBL.LoadUserParamInt($"Widgets-PageAutoRefresh-{v.Skin}-{v.SelectedX54ID}", 0);
                }
                
            }
            
            
            if (v.IsAiGroup)
            {
                //virtuální plocha: všechny osobní widgety, rozložení v uživatelském parametru (x56 se pro ni nezakládá)
                v.recX56 = new BO.x56WidgetBinding()
                {
                    j02ID = Factory.CurrentUser.pid, x56Skin = v.Skin, x54ID = AiX54ID,
                    x56Boxes = string.Join(",", v.lisAllWidgets.Select(p => p.x55Code)),
                    x56DockState = Factory.CBL.LoadUserParam(AiDockStateParam)
                };
            }
            else
            {
                v.recX56 = Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, v.Skin, v.SelectedX54ID);
            }

            if (Factory.CurrentUser.j02LangIndex == 2)
            {
                v.DataTables_Localisation = "/lib/datatables/localisation/uk_UA.json";
            }
            else
            {
                v.DataTables_Localisation = "/lib/datatables/localisation/cs_CZ.json";
            }

            if (v.recX56 == null || v.recX56.x56Boxes == null)
            {
                return; //uživatel nemá na ploše žádný widget, dál není třeba pokračovat
            }

            var boxes = BO.Code.Bas.ConvertString2List(v.recX56.x56Boxes);
            foreach (string s in boxes)
            {
                if (v.lisAllWidgets.Where(p => p.x55Code == s).Count() > 0)
                {
                    v.lisUserWidgets.Add(v.lisAllWidgets.Where(p => p.x55Code == s).First());
                }
            }


            //rozložení: jedno pořadí + široké widgety (starý sloupcový stav se načte po řádcích – WidgetsGrid)
            v.Grid = WidgetsGrid.Build(v.recX56.x56DockState, v.lisUserWidgets);

            if (v.lisUserWidgets.Exists(p => p.x55TableSql != null && p.x55TableColHeaders != null))
            {
                v.IsDataTables = true;
            }
            if (v.IsDataTables)
            {
                if (v.lisUserWidgets.Exists(p => p.x55DataTablesButtons > BO.x55DataTablesBtns.None))
                {
                    v.IsExportButtons = true;   //zobrazovat tlačítka XLS/CSV/COPY
                }
                if (v.lisUserWidgets.Exists(p => p.x55DataTablesButtons == BO.x55DataTablesBtns.ExportPrintPdf))
                {
                    v.IsPdfButtons = true;      //zobrazovat i tlačítko PDF
                }
                if (v.IsPdfButtons || v.lisUserWidgets.Exists(p => p.x55DataTablesButtons == BO.x55DataTablesBtns.ExportPrint))
                {
                    v.IsPrintButton = true;      //zobrazovat i tlačítko PDF
                }
            }
            if (v.lisUserWidgets.Exists(p => p.x55ChartSql != null && p.x55ChartHeaders != null))
            {
                v.IsCharts = true;
            }

            switch (v.ColumnsPerPage)
            {
                case 1:
                    v.BoxColCss = "col-12";
                    break;
                case 2:
                    v.BoxColCss = "col-lg-6";
                    break;
                case 3:
                    v.BoxColCss = "col-sm-6 col-lg-4";
                    break;
            }
        }


        //načtení html obsahu jednoho boxu
        //Bezpečnost (nález 14): widget smí uživatel spustit jen ze své skupiny (nebo osobní vlastní); do prohlížeče jde jen název a HTML –
        //nikdy x55TableSql / x55ChartSql / x55AiTask.
        public IActionResult GetWidgetHtmlContent(int x55id, int columnsperpage, string d,int j02id,int p28id)
        {
            var rec = Factory.x55WidgetBL.Load(x55id);
            if (rec == null || !CanRunWidget(rec, ref j02id))
            {
                return Json(WidgetDto(new BO.x55Widget() { x55Name = "?", x55Code = "x", x55Content = Factory.tra("Widget nebyl nalezen.") }));
            }
            if (rec.IsAiWidget)
            {
                //AI widget: zadání → SQL přes AiAggregate, bez volání AI
                rec.x55Content = AiRenderer().Render(rec);
                return Json(WidgetDto(rec));
            }
            string strHtml = rec.x55Content;
            if (strHtml != null)
            {
                strHtml = strHtml.Replace("@j02id", j02id.ToString());
                strHtml = strHtml.Replace("@x01id", rec.x01ID.ToString());
                strHtml = strHtml.Replace("@p28id", p28id.ToString());
            }
            
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(strHtml);

            if (rec.x55ReportCodes != null)
            {
                rec.x55ReportCodes = rec.x55ReportCodes.Replace("|", ";").Replace(",", ";");
                var arr = BO.Code.Bas.ConvertString2List(rec.x55ReportCodes, ";");
                foreach(var strX31Code in arr)
                {
                    var recX31 = Factory.x31ReportBL.LoadByCode(strX31Code,0);
                    if (recX31 != null)
                    {
                        sb.AppendLine($"<div style='padding:4px'><a href=\"javascript:_window_open('/ReportsClient/reportNoContext?noframework=true&x31id={recX31.pid}')\"><span class='material-icons-outlined-btn'>print</span>{recX31.x31Name}</a></div>");
                    }
                }
            }

            DateTime datD0 = DateTime.Today;
            if (!string.IsNullOrEmpty(d))
            {
                datD0 = BO.Code.Bas.String2Date(d);
            }
            if (j02id == 0) j02id = Factory.CurrentUser.pid;


            if (rec.x55ChartSql != null && rec.x55ChartHeaders != null)
            {
                string s = rec.x55ChartSql;
                s = DL.BAS.ParseMergeSQL(s, j02id.ToString());
                s = s.Replace("@j02id", j02id.ToString());
                s = s.Replace("@x01id", rec.x01ID.ToString());
                s = s.Replace("@p28id", p28id.ToString());
                //s = s.Replace("$$", "''''");

                if (s.Contains("@d"))
                {
                    s = s.Replace("@d", BO.Code.Bas.GD(datD0));
                }
                var dt = Factory.gridBL.GetListFromPureSql(s);
                var cGen = new BL.Code.Datatable2Chart();
                sb.AppendLine(cGen.CreateGoogleChartHtml(dt, rec));
                
                
            }
            if (rec.x55TableSql != null && rec.x55TableColHeaders != null)
            {
                string s = rec.x55TableSql;
                s = DL.BAS.ParseMergeSQL(s, Factory.CurrentUser.pid.ToString());
                s = s.Replace("@j02id", j02id.ToString());
                s = s.Replace("@x01id", rec.x01ID.ToString());
                s = s.Replace("@p28id", p28id.ToString());
                

                if (s.Contains("@d"))
                {
                    s = s.Replace("@d", BO.Code.Bas.GD(datD0));
                }
                var dt = Factory.gridBL.GetListFromPureSql(s);
                if (dt.Rows.Count >= rec.x55DataTablesLimit && rec.x55DataTablesLimit > 0)
                {
                    rec.IsUseDatatables = true; //splněna podmínka pro zobrazení tabulky přes plugin DataTables

                }
               
                var cGen = new BL.Code.Datatable2Html(new BL.Code.HtmlTable() { IsCanExport = false, ColHeaders = rec.x55TableColHeaders, ColTypes = rec.x55TableColTypes, ClientID = rec.x55Code, IsUseDatatables = rec.IsUseDatatables,ColFlexSubtotals=rec.x55TableColTotals }, Factory);
                
                sb.AppendLine(cGen.CreateHtmlTable(dt, 1000));

                

            }

            switch (rec.x55Code.ToLower())
            {
                case "pandulak":
                    var strPandulakDir = $"{Factory.App.AppRootFolder}\\wwwroot\\images\\pandulak";
                    sb = new System.Text.StringBuilder();
                    sb.Append($"<img src='/images/pandulak/{Code.basUI.getPandulakImage(strPandulakDir)}'/>");
                    
                    if (columnsperpage <= 2)
                    {
                        sb.Append($"<img src='/images/pandulak/{Code.basUI.getPandulakImage(strPandulakDir)}'/>");
                        
                    }
                    break;
            }

            if (rec.x55Name.Contains("@d"))
            {
                rec.x55Name = rec.x55Name.Replace("@d", datD0.ToString("dd.MM.yyyy ddd"));
            }

            rec.x55Content = sb.ToString();
            return Json(WidgetDto(rec));
        }

        /// <summary>Odpověď pro widgets.js – jen to, co prohlížeč potřebuje.</summary>
        private static object WidgetDto(BO.x55Widget rec) => new
        {
            x55Name = rec.x55Name,
            x55Content = rec.x55Content,
            x55Code = rec.x55Code,
            isUseDatatables = rec.IsUseDatatables,
            x55DataTablesButtons = (int)rec.x55DataTablesButtons
        };

        /// <summary>
        /// Smí přihlášený spustit widget? Osobní = jen vlastník. Sdílený = je v x57 některé skupiny role přihlášeného
        /// (při prohlížení cizí plochy i skupiny role jejího uživatele). Cizí plocha (j02id) jen pro uživatele, které smí vidět
        /// (MyRecordsDisponible); jinak se j02id přepne na přihlášeného – @j02id v SQL widgetu nesmí jít libovolně podstrčit.
        /// </summary>
        private bool CanRunWidget(BO.x55Widget rec, ref int j02id)
        {
            int me = Factory.CurrentUser.pid;
            if (rec.IsPersonal) { j02id = me; return rec.j02ID_Owner == me; }

            var j04ids = new List<int> { Factory.CurrentUser.j04ID };
            if (j02id > 0 && j02id != me)
            {
                var other = Factory.j02UserBL.GetList(new BO.myQueryJ02() { pids = new List<int> { j02id }, MyRecordsDisponible = true }).FirstOrDefault();
                if (other == null) j02id = me;
                else j04ids.Add(other.j04ID);
            }
            foreach (int j04id in j04ids.Distinct())
            {
                foreach (var g in Factory.j04UserRoleBL.GetList_x54(j04id).Where(p => !p.isclosed))
                {
                    if (Factory.x54WidgetGroupBL.GetList_x57(g.pid).Any(p => p.x55ID == rec.pid)) return true;
                }
            }
            return false;
        }
    }
}