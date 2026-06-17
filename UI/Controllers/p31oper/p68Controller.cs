using Microsoft.AspNetCore.Mvc;
using System.Data;
using UI.Models;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p68Controller : BaseController
    {
        private readonly IHttpClientFactory _httpclientfactory;
        public p68Controller(IHttpClientFactory hcf)
        {
            _httpclientfactory = hcf;
        }
        public IActionResult Index()
        {
            var v = new p68ViewModel() { };
            //v.IsUseTimeApi = Factory.CBL.LoadUserParamBool("p68-IsUseTimeApi", false);
            v.IsUseTimeApi = false;

            RefreshState(v);

            return View(v);
        }

        private void RefreshState(p68ViewModel v)
        {
            v.TimeApiRecord = LoadCurrentTime(v.IsUseTimeApi);

            if (v.TimeApiRecord == null && v.IsUseTimeApi)
            {
                this.AddMessageTranslated("Selhalo volání služby https://timeapi.io.<hr>To může způsobit problém s načítáním aktuálního času.");
                v.TimeApiRecord = new BO.TimeApi.Record() { dateTime = DateTime.Now };
            }

            var lisP68 = Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid);
            v.lisRows = new List<StopWatchRow>();

            foreach (var rec in lisP68)
            {
                v.lisRows.Add(ConvertRec2Row(rec));
                if (rec.p68IsRunning)
                {
                    v.StartStopWatchUtc = rec.p68LastStart.Value.AddSeconds(-1 * rec.p68Duration).ToUniversalTime().ToString("o");
                }
            }
        }

        private StopWatchRow ConvertRec2Row(BO.p68StopWatch rec)
        {
            var c = new StopWatchRow() { pid = rec.pid, p41ID = rec.p41ID, p32ID = rec.p32ID, p56ID = rec.p56ID, j02ID = rec.j02ID, p68Text = rec.p68Text, p68Ordinary = rec.p68Ordinary, p68IsRunning = rec.p68IsRunning, p68LastStart = rec.p68LastStart, p68LastEnd = rec.p68LastEnd, p68Duration = rec.p68Duration };
            c.Project = rec.p41Name;
            c.Activity = rec.p32Name;
            if (rec.p28Name != null)
            {
                c.Project = $"{rec.p28Name} - {rec.p41Name}";
            }
            c.duration_hhmm = BO.Code.Time.GetTimeFromSeconds((double)c.p68Duration);

            if (c.p41ID > 0)
            {
                var recP41 = Factory.p41ProjectBL.Load(c.p41ID);
                c.p61ID = (recP41.p61ID > 0 ? recP41.p61ID : recP41.p61ID_Byp42ID);
            }

            return c;
        }

        public StopWatchRow SaveClientChanges(int pid, string p68text, int p68ordinary, string durhhmm, int p32id)
        {
            var c = Factory.p68StopWatchBL.Load(pid);
            c.p68Text = p68text;
            c.p68Ordinary = p68ordinary;
            c.p32ID = p32id;

            if (!string.IsNullOrEmpty(durhhmm)) //ruční změna času
            {
                int secs = BO.Code.Time.ConvertTimeToSeconds(durhhmm);
                if (secs < 0) secs = 0;
                if (secs > 24 * 60 * 60) secs = 24 * 60 * 60;

                
                var xx = LoadCurrentTime(Factory.CBL.LoadUserParamBool("p68-IsUseTimeApi", false));
                c.p68LastStart = xx.dateTime.AddSeconds((double)secs * -1);
                c.p68LastEnd = xx.dateTime;
                c.p68Duration = secs;

            }
            Factory.p68StopWatchBL.Save(c);

            return ConvertRec2Row(Factory.p68StopWatchBL.Load(pid));
        }


        [HttpPost]
        public IActionResult Index(p68ViewModel v, int p68id, int p41id, int p68ordinary)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                BO.p68StopWatch rec = null;
                if (p68id > 0)
                {
                    rec = Factory.p68StopWatchBL.Load(p68id);
                    if (rec == null)
                    {
                        rec = new BO.p68StopWatch() { j02ID = Factory.CurrentUser.pid };
                    }

                }
                else
                {
                    rec = new BO.p68StopWatch() { j02ID = Factory.CurrentUser.pid };
                }
                bool bolRecoveryCache = false;

                switch (v.PostbackOper)
                {
                    case "IsUseTimeApi":
                        Factory.CBL.SetUserParam("p68-IsUseTimeApi", v.IsUseTimeApi.ToString());
                        RefreshState(v);
                        return View(v);

                    case "reload":
                        break;

                    case "start":
                    case "stop":
                        var lis = Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid).Where(p => p.p68IsRunning || p.pid == p68id);
                        foreach (var rec1 in lis)
                        {
                            rec1.p68IsRunning = false;
                            rec1.p68LastEnd = v.TimeApiRecord.dateTime;
                            if (rec1.p68LastStart == null)
                            {
                                rec1.p68LastStart = rec1.p68LastEnd;
                            }
                            
                            TimeSpan span = rec1.p68LastEnd.Value - rec1.p68LastStart.Value;
                            rec1.p68Duration += (span.Hours * 60 * 60) + (span.Minutes * 60) + span.Seconds;
                            Factory.p68StopWatchBL.Save(rec1);
                        }

                        if (v.PostbackOper == "start")
                        {
                            rec.p68LastStart = v.TimeApiRecord.dateTime;
                            rec.p68IsRunning = true;
                            Factory.p68StopWatchBL.Save(rec);
                        }


                        break;
                    case "p41id":
                        rec.p41ID = p41id;
                        Factory.p68StopWatchBL.Save(rec);
                        break;
                    case "ordinary":
                        rec.p68Ordinary = p68ordinary;
                        Factory.p68StopWatchBL.Save(rec);
                        break;
                    case "add_row":
                        var c = new BO.p68StopWatch() { j02ID = Factory.CurrentUser.pid, p68Ordinary = NewRowIndex(v) };

                        p68id = Factory.p68StopWatchBL.Save(c);
                        break;
                    case "clone_row":

                        var cc = new BO.p68StopWatch() { j02ID = rec.j02ID, p41ID = rec.p41ID, p68Text = rec.p68Text, p56ID = rec.p56ID, p32ID = rec.p32ID, p68Ordinary = NewRowIndex(v) };

                        p68id = Factory.p68StopWatchBL.Save(cc);
                        break;
                    case "delete_row":
                        Factory.CBL.DeleteRecord("p68", p68id);

                        p68id = 0;
                        break;
                    case "clear":
                        bolRecoveryCache = true;
                        p68id = 0;
                        break;
                }
                if (bolRecoveryCache)
                {
                    Factory.p68StopWatchBL.Clear(Factory.CurrentUser.pid);
                }
                v.Jump2Pid = p68id;
                RefreshState(v);
                return View(v);
            }

            if (ModelState.IsValid)
            {

                if (1 == 1)
                {
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }



            }

            this.Notify_RecNotSaved();
            return View(v);
        }

        private int NewRowIndex(p68ViewModel v)
        {
            int x = v.lisRows.Count() + 1;
            if (v.lisRows.Count() > 0)
            {
                if (v.lisRows.Max(p => p.p68Ordinary) > x)
                {
                    x = v.lisRows.Max(p => p.p68Ordinary) + 1;
                }
            }
            return x;
        }

        public BO.TimeApi.Record LoadCurrentTime(bool bolIsUseTimeApi)
        {
            if (bolIsUseTimeApi)
            {
                //načíst čas ze služby https://timeapi.io/
                try
                {
                    return new BL.Code.TimeApiSupport().LoadCurrentTime(_httpclientfactory.CreateClient()).Result;
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return new BO.TimeApi.Record() { dateTime = DateTime.Now }; //věříme systémovému času serveru
            }
            
            

            //if (Factory.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp || Factory.App.HostingMode == BL.Singleton.HostingModeEnum.TotalCloud)
            //{
            //    return new BO.TimeApi.Record() { dateTime = DateTime.Now }; //v cloudu věříme systémovému času serveru
            //}

            



        }
    }

}
