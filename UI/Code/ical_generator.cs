
using BO;

namespace UI.Code
{
    public class ical_generator
    {
        private System.Text.StringBuilder _sb { get; set; }
        private BL.Factory _f { get; set; }
        private IEnumerable<BO.j02User> _lisJ02 { get; set; }


        public ical_generator(BL.Factory f)
        {
            _f = f;
            _sb = new System.Text.StringBuilder();

            _lisJ02 = _f.j02UserBL.GetList(new BO.myQueryJ02() { IsRecordValid = false });

        }

        public string GetCalendarToString()
        {
            handle_ical_header();


            sr("END:VCALENDAR");

            return _sb.ToString();
        }
        public void Generate_p31_Calendar(List<int> pids, int j02id, int j11id, List<int> p32ids, int p61id, int p41id, DateTime d1, DateTime d2, string person_name_format)
        {
            handle_ical_header();

            var mq = new BO.myQueryP31() { p32ids = p32ids, p61id = p61id, p41id = p41id, tabquery = "time", j02id = j02id, j11id = j11id, period_field = "p31Date", global_d1 = d1, global_d2 = d2 };
            if (pids != null && pids.Count() > 0)
            {
                mq.pids = pids;
            }

            bool bolShowClient = false; bool bolShowActivityName = false;
            var lis = _f.p31WorksheetBL.GetList(mq);

            if (lis.Count() > 1)
            {
                if (lis.Select(p => p.p28ID_Client).Distinct().Count() > 1) bolShowClient = true;
                if (lis.Select(p => p.p32ID).Distinct().Count() > 1) bolShowActivityName = true;
            }
            foreach (var c in lis)
            {
                p31_record(c, bolShowClient, bolShowActivityName, person_name_format);
            }


        }

        public void Generate_o22_Calendar(List<int> pids, int j02id, List<int> j02ids, int o21id, int p41id, DateTime d1, DateTime d2, int x67id, string person_name_format)
        {
            handle_ical_header();

            var mq = new BO.myQueryO22() { IsRecordValid = null, o21id = o21id, p41id = p41id, j02id = j02id, j02ids = j02ids, period_field = "o22PlanUntil", global_d1 = d1, global_d2 = d2 };
            if (pids != null && pids.Count() > 0)
            {
                mq.pids = pids;
            }


            var lis = _f.o22MilestoneBL.GetList_Dayline(mq, x67id);

            foreach (var c in lis)
            {
                o22_record(c, person_name_format, j02id);
            }


        }

        public void Generate_p56_Calendar(List<int> pids, int j02id, List<int> j02ids, int p57id, int p41id, DateTime d1, DateTime d2, int x67id, string person_name_format)
        {
            handle_ical_header();

            var mq = new BO.myQueryP56() { x67id = x67id, IsRecordValid = null, p41id = p41id, p57id = p57id, j02id = j02id, j02ids = j02ids, period_field = "p56PlanUntil", global_d1 = d1, global_d2 = d2 };
            if (pids != null && pids.Count() > 0)
            {
                mq.pids = pids;
            }
            bool bolShowP57Name = false;
            var lis = _f.p56TaskBL.GetList(mq);
            if (lis.Count() > 1 && lis.Select(p => p.p57ID).Distinct().Count() > 1) bolShowP57Name = true;
            foreach (var c in lis)
            {
                p56_record(c, bolShowP57Name,j02id);
            }


        }

        private void p31_record(BO.p31Worksheet rec, bool bolShowClient, bool bolShowActivityName, string person_name_format)
        {
            sr("BEGIN:VEVENT");

            string strName = $"{rec.j02FirstName} {rec.j02LastName}: {rec.p32Name}";
            if (person_name_format == "inicialy")
            {
                strName = $"{rec.j02FirstName.Substring(0, 1)}{rec.j02LastName.Substring(0, 1)}";
            }
            if (person_name_format == "nic")
            {
                strName = rec.p32Name;
            }
            if (!bolShowActivityName)
            {
                strName = rec.Person;
            }
            if (rec.p33ID == BO.p33IdENUM.Cas)
            {
                strName = $"{strName} [{rec.p31HHMM_Orig}]";
            }
            var memos = new List<string>();


            if (person_name_format != "jmeno")
            {
                memos.Add($"{rec.j02FirstName} {rec.j02LastName}");
            }
            memos.Add($"Aktivita: {rec.p32Name}");


            //memos.Add(BO.Code.Bas.OM2(rec.p32Name, 30));

            if (rec.p28ID_Client > 0 && bolShowClient)
            {
                memos.Add($"{BO.Code.Bas.OM2(rec.ClientName, 20)}");
            }
            memos.Add(BO.Code.Bas.OM2(rec.p41Name, 30));
            if (rec.p56ID > 0) memos.Add("Úkol: " + rec.p56Name);


            sr($"UID:p31-{rec.pid}");
            sr($"DTSTAMP:{Convert.ToDateTime(rec.DateInsert).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")}");

            if (rec.TimeFrom == rec.TimeUntil)
            {
                sr("DTSTART;VALUE=DATE:" + rec.p31Date.ToString("yyyyMMdd"));
                sr("DTEND;VALUE=DATE:" + new DateTime(rec.p31Date.Year, rec.p31Date.Month, rec.p31Date.Day).AddDays(1).ToString("yyyyMMdd"));
            }
            else
            {
                DateTime d1 = Convert.ToDateTime(rec.p31DateTimeFrom_Orig);
                DateTime d2 = Convert.ToDateTime(rec.p31DateTimeUntil_Orig);
                sr("DTSTART;VALUE=DATE:" + d1.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                sr("DTEND;VALUE=DATE:" + d2.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
            }



            sr($"SUMMARY:{strName}");
            if (memos.Count() > 0)
            {
                //sr($"DESCRIPTION:{String.Join("\n\r", memos)}");
                sr($"DESCRIPTION:{String.Join(Environment.NewLine, memos)}");
            }

            var qryOwner = _lisJ02.Where(p => p.pid == rec.j02ID_Owner);
            if (qryOwner.Count() > 0)
            {
                sr($"ATTENDEE;CN={qryOwner.First().FullNameAsc};PARTSTAT=NEEDS-ACTION;ROLE=REQ-PARTICIPANT:mailto:{qryOwner.First().j02Email}");
                sr($"ORGANIZER;CN={qryOwner.First().FullNameAsc}:mailto:{qryOwner.First().j02Email}");
            }

            sr("TRANSP:OPAQUE");

            sr("END:VEVENT");
        }

        private void p56_record(BO.p56Task rec, bool bolShowP57Name, int j02id_only)
        {
            sr("BEGIN:VEVENT");

            string strName = bolShowP57Name ? rec.p57Name + ": " + rec.p56Name : rec.p56Name;
            var memos = new List<string>();
            if (rec.b02ID > 0) memos.Add("Aktuální stav: " + rec.b02Name);
            if (rec.ProjectClient != null) memos.Add("Klient: " + rec.ProjectClient);
            if (rec.p41ID > 0) memos.Add("Projekt: " + rec.ProjectCodeAndName);
            if (rec.p56Notepad != null)
            {
                memos.Add(BO.Code.Bas.Html2Text(rec.p56Notepad));
            }

            sr("UID:p56-" + rec.pid.ToString());
            sr("DTSTAMP:" + Convert.ToDateTime(rec.DateInsert).ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));

            DateTime d = Convert.ToDateTime(rec.p56PlanUntil);
            sr("DTSTART;VALUE=DATE:" + d.ToString("yyyyMMdd"));
            sr("DTEND;VALUE=DATE:" + new DateTime(d.Year, d.Month, d.Day).AddDays(1).ToString("yyyyMMdd"));

            if (j02id_only == 0)
            {
                var lisJ02 = _f.j02UserBL.GetList(new BO.myQueryJ02() { x67Entity = "p56", x69RecordPid = rec.pid });
                foreach (var c in lisJ02)
                {
                    sr($"ATTENDEE;CN={c.FullNameAsc};PARTSTAT=NEEDS-ACTION;ROLE=REQ-PARTICIPANT:mailto:{c.j02Email}");
                }
            }
            else
            {
                var recJ02 = _f.j02UserBL.Load(j02id_only);
                sr($"ATTENDEE;CN={recJ02.FullNameAsc};PARTSTAT=NEEDS-ACTION;ROLE=REQ-PARTICIPANT:mailto:{recJ02.j02Email}");
            }


            sr("SUMMARY:" + strName);
            if (memos.Count() > 0)
            {
                sr("DESCRIPTION:" + String.Join("\n", memos));
            }

            var qryOwner = _lisJ02.Where(p => p.pid == rec.j02ID_Owner);
            if (qryOwner.Count() > 0)
            {
                sr($"ORGANIZER;CN={qryOwner.First().FullNameAsc}:mailto:{qryOwner.First().j02Email}");
            }

            sr("TRANSP:OPAQUE");

            sr("END:VEVENT");
        }



        private void o22_record(BO.o22MilestoneDayline rec, string person_name_format, int j02id_only)
        {
            sr("BEGIN:VEVENT");

            string strName = rec.o22Name;
            if (rec.o22Name == null)
            {
                strName = rec.o21Name;
            }
            if (rec.j02ID > 0)
            {
                switch (person_name_format)
                {
                    case "nic":
                        break;
                    case "inicialy":
                        strName = $"{rec.j02FirstName.Substring(0, 1)}{rec.j02LastName.Substring(0, 1)}: {strName}";
                        break;
                    case "jmeno":
                        strName = $"{rec.j02FirstName}{rec.j02LastName}: {strName}";
                        break;
                }

            }


            var memos = new List<string>();
            if (person_name_format != "jmeno")
            {
                memos.Add($"{rec.j02FirstName} {rec.j02LastName}");
            }
            if (rec.ProjectClient != null) memos.Add($"Klient: {BO.Code.Bas.OM2(rec.ProjectClient, 20)}");
            if (rec.p41ID > 0) memos.Add($"Projekt: {rec.ProjectCodeAndName}");


            if (rec.o22Notepad != null)
            {
                memos.Add(BO.Code.Bas.Html2Text(rec.o22Notepad));
            }

            sr($"UID:o22-{rec.pid}");
            sr($"DTSTAMP:{Convert.ToDateTime(rec.DateInsert).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")}");

            if (rec.o22PlanFrom == null) rec.o22PlanFrom = rec.o22PlanUntil;
            DateTime d1 = Convert.ToDateTime(rec.o22PlanFrom);
            DateTime d2 = Convert.ToDateTime(rec.o22PlanUntil);
            if (rec.o22IsAllDay)
            {
                sr($"DTSTART;VALUE=DATE:{d1.ToString("yyyyMMdd")}");
                sr($"DTEND;VALUE=DATE:{new DateTime(d2.Year, d2.Month, d2.Day).AddDays(1).ToString("yyyyMMdd")}");
            }
            else
            {
                sr($"DTSTART;VALUE=DATE:{d1.ToUniversalTime().ToString("yyyyMMddTHHmmssZ")}");
                sr($"DTEND;VALUE=DATE:{d2.ToUniversalTime().ToString("yyyyMMddTHHmmssZ")}");
            }

            if (j02id_only == 0)
            {
                var lisJ02 = _f.j02UserBL.GetList(new BO.myQueryJ02() { x67Entity = "o22", x69RecordPid = rec.pid });
                foreach (var c in lisJ02)
                {
                    sr($"ATTENDEE;CN={c.FullNameAsc};PARTSTAT=NEEDS-ACTION;ROLE=REQ-PARTICIPANT:mailto:{c.j02Email}");
                }
            }
            else
            {
                var recJ02 = _f.j02UserBL.Load(j02id_only);
                sr($"ATTENDEE;CN={recJ02.FullNameAsc};PARTSTAT=NEEDS-ACTION;ROLE=REQ-PARTICIPANT:mailto:{recJ02.j02Email}");
            }


            sr("SUMMARY:" + strName);
            if (memos.Count() > 0)
            {
                sr($"DESCRIPTION:{String.Join(Environment.NewLine, memos)}");
            }
            if (rec.o22Location != null)
            {
                sr($"LOCATION:{rec.o22Location}");
            }

            var qryOwner = _lisJ02.Where(p => p.pid == rec.j02ID_Owner);
            if (qryOwner.Count() > 0)
            {
                sr($"ORGANIZER;CN={qryOwner.First().FullNameAsc}:mailto:{qryOwner.First().j02Email}");
            }

            sr("TRANSP:OPAQUE");

            sr("END:VEVENT");

        }

        private void sr_vcalendar_header()
        {

            sr("BEGIN:VCALENDAR");
            sr("VERSION:2.0");
            sr("METHOD:PUBLISH");
            sr("PRODID:marktime.net");
            sr("X-SZN-COLOR:#088acc");
            sr("X-WR-CALNAME:MARKTIME");
        }

        private void sr_timezone()
        {
            sr("BEGIN:VTIMEZONE");
            sr("TZID:Europe/Prague");
            sr("BEGIN:STANDARD");
            sr("DTSTART:20001029T030000");
            sr("RRULE:FREQ=YEARLY;BYDAY=-1SU;BYMONTH=10");
            sr("TZNAME:CET");
            sr("TZOFFSETFROM:+0200");
            sr("TZOFFSETTO:+0100");
            sr("END:STANDARD");
            sr("BEGIN:DAYLIGHT");
            sr("DTSTART:20000326T020000");
            sr("RRULE:FREQ=YEARLY;BYDAY=-1SU;BYMONTH=3");
            sr("TZNAME:CEST");
            sr("TZOFFSETFROM:+0100");
            sr("TZOFFSETTO:+0200");
            sr("END:DAYLIGHT");
            sr("END:VTIMEZONE");
        }

        private void sr(string s)
        {

            _sb.AppendLine(s);
        }
        private void handle_ical_header()
        {
            if (_sb.Length == 0)
            {
                sr_vcalendar_header();
                sr_timezone();
            }
        }
        private BO.j02User FindUsers(int j02id, int j11id)
        {

            if (_lisJ02.Any(p => p.pid == j02id))
            {
                return _lisJ02.First(p => p.pid == j02id);
            }

            return null;
        }
    }
}
