using BL;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using UI.Models;

namespace UI.Controllers.ical
{
    public class icalController : Controller
    {
        private Factory _f;
        private readonly IHttpClientFactory _httpclientfactory; //client pro SMS
        public icalController(Factory f, IHttpClientFactory hcf)
        {
            _f = f;
            _httpclientfactory = hcf;
        }

        public IActionResult Index(string key,int j02id,int j11id,string p32ids, int p61id,int p41id, string d1, string d2,int x67id_p56,int x67id_o22,string person_name_format)
        {
            string strLogin = new BO.Code.Cls.Crypto().EasyDecrypt(key);  //v key je uložen login uživatele
            _f.InhaleUserByLogin(strLogin);

            //if (j02id==0 && j11id == 0)
            //{
            //    j02id = _f.CurrentUser.pid;
            //}

            DateTime datD1 = DateTime.Today.AddYears(-1);
            DateTime datD2 = DateTime.Today.AddYears(1);
            if (!string.IsNullOrEmpty(d1) && !string.IsNullOrEmpty(d2))
            {
                datD1 = BO.Code.Bas.String2Date(d1);
                datD2 = BO.Code.Bas.String2Date(d2);
            }

            

            List<int> j02ids = null;
            if (j11id > 0)
            {
                j02ids = _f.j02UserBL.GetList(new BO.myQueryJ02() { j11id = j11id }).Select(p => p.pid).ToList();
            }
            
            var ical = new Code.ical_generator(_f);

            if (!string.IsNullOrEmpty(p32ids) || p61id>0)
            {                
                ical.Generate_p31_Calendar(null, j02id, j11id, BO.Code.Bas.ConvertString2ListInt(p32ids),p61id, p41id, datD1, datD2, person_name_format);
            }

            if (x67id_p56>0)
            {
                ical.Generate_p56_Calendar(null, j02id, j02ids,0,p41id, datD1, datD1, x67id_p56, person_name_format);
            }
            if (x67id_o22>0)
            {
                ical.Generate_o22_Calendar(null, j02id, j02ids, 0,p41id, datD1, datD2,x67id_o22, person_name_format);
            }
            

            string s = ical.GetCalendarToString();

            //return File(Encoding.ASCII.GetBytes(s), "text/calendar", "marktime_icalendar.ics");
            return File(Encoding.UTF8.GetBytes(s), "text/calendar", "marktime_icalendar.ics");
        }



    }
}
