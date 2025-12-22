using BL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Cryptography.Xml;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TimesheetController : BaseApiController
    {
        [SwaggerOperation(Summary = "Vykázat časový úkon")]
        [HttpPost]
        [Route("SaveTimeRecord")]
        public BO.Result SaveTimeRecord(DateTime p31date,int j02id,int p41id,int p34id, int p32id,string value_orig,string p31text)
        {
            
            var rec = new BO.p31WorksheetEntryInput() {p31Date=new List<DateTime> { p31date}, j02ID = j02id, p41ID = p41id,p34ID=p34id, p32ID = p32id, p31Text = p31text,Value_Orig= value_orig };
            rec.p31HoursEntryflag = BO.p31HoursEntryFlagENUM.Hodiny;

            var intP31ID=GetFactory().p31WorksheetBL.SaveOrigRecord(rec, BO.p33IdENUM.Cas, null);

            var ret = new BO.Result(false, intP31ID.ToString()) { pid = intP31ID };
            if (intP31ID == 0)
            {
                ret.Flag = BO.ResultEnum.Failed;
               
                ret.Message = GetFactory().GetFirstNotifyMessage();
            }

            return ret;
        }

        [SwaggerOperation(Summary = "Uložit záznam stopky")]
        [HttpPost]
        [Route("SaveP68Record")]
        public BO.Result SaveP68Record(string oper,int p68id,int p41id,int p32id,string durhhmm, string p68text,int p68ordinary)
        {
            var ret = new BO.Result(false);
            int intJ02ID = this.GetFactory().CurrentUser.pid;

            var rec = new BO.p68StopWatch() { j02ID = intJ02ID };
            if (p68id > 0)
            {
                rec = this.GetFactory().p68StopWatchBL.Load(p68id);
            }
           

            var curtime = LoadCurrentTime();

            if (oper=="start" || oper == "stop")
            {
                var lis = this.GetFactory().p68StopWatchBL.GetList(intJ02ID).Where(p => p.p68IsRunning || p.pid == p68id);
                foreach (var rec1 in lis)
                {
                    rec1.p68IsRunning = false;
                    rec1.p68LastEnd =curtime.dateTime;
                    if (rec1.p68LastStart == null)
                    {
                        rec1.p68LastStart = rec1.p68LastEnd;
                    }
                    TimeSpan span = rec1.p68LastEnd.Value - rec1.p68LastStart.Value;
                    rec1.p68Duration += (span.Hours * 60 * 60) + (span.Minutes * 60) + span.Seconds;
                    p68id=this.GetFactory().p68StopWatchBL.Save(rec1);
                }
                if (oper == "start")
                {
                    rec.p68LastStart = curtime.dateTime;
                    rec.p68IsRunning = true;
                    p68id=this.GetFactory().p68StopWatchBL.Save(rec);
                }
            }
            if (oper == "p41id")
            {
                rec.p41ID = p41id;
                p68id=this.GetFactory().p68StopWatchBL.Save(rec);
            }
            if (oper== "clone_row")
            {
                var lis = this.GetFactory().p68StopWatchBL.GetList(intJ02ID);
                var cc = new BO.p68StopWatch() { j02ID = intJ02ID, p41ID = rec.p41ID, p68Text = rec.p68Text, p56ID = rec.p56ID, p32ID = rec.p32ID, p68Ordinary = lis.Count()+1 };

                p68id = this.GetFactory().p68StopWatchBL.Save(cc);
            }
            if (oper == "delete_row")
            {
                this.GetFactory().CBL.DeleteRecord("p68", p68id);
                return ret;
            }
            if (oper == "clear")
            {
                this.GetFactory().p68StopWatchBL.Clear(intJ02ID);
                return ret;
            }

            if (oper == "change")
            {                
                rec.p32ID = p32id;
                rec.p68Text = p68text;
                rec.p68Ordinary = p68ordinary;
                if (!string.IsNullOrEmpty(durhhmm)) //ruční změna času
                {
                    int secs = BO.Code.Time.ConvertTimeToSeconds(durhhmm);
                    if (secs < 0) secs = 0;
                    if (secs > 24 * 60 * 60) secs = 24 * 60 * 60;
                    
                    rec.p68LastStart = curtime.dateTime.AddSeconds((double)secs * -1);
                    rec.p68LastEnd = curtime.dateTime;
                    rec.p68Duration = secs;

                }
            }
            
            if (p68id == 0)
            {
                ret.Flag = BO.ResultEnum.Failed;

                ret.Message = GetFactory().GetFirstNotifyMessage();
            }

            return ret;
        }



        private BO.TimeApi.Record LoadCurrentTime()
        {
            return new BL.Code.TimeApiSupport().LoadCurrentTime().Result;
        }
    }
}
