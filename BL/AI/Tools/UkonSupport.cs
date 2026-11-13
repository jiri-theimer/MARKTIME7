using System.Globalization;

namespace BL.AI.Tools
{
    /// <summary>
    /// Společná pravidla nástrojů pro vykazování hodin (ukon_nabidka, ukon_zapis): které projekty, sešity a aktivity
    /// smí uživatel použít. Stejné dotazy jako formulář úkonu (p31/Record, mobilní aplikace) – žádné vlastní SQL.
    /// </summary>
    internal static class UkonSupport
    {
        public const int ProjectLimit = 30;

        /// <summary>Uživatel má modul úkonů.</summary>
        public static bool HasModule(BL.Factory f) => f.CurrentUser.j04IsModule_p31;

        /// <summary>Projekty, do kterých smí uživatel vykazovat hodiny (podle oprávnění role a projektových rolí).</summary>
        public static List<BO.p41Project> Projects(BL.Factory f, string search, int top)
        {
            var mq = new BO.myQueryP41("p41") { j02id_query = f.CurrentUser.pid, p33id_for_p31_entry = (int)BO.p33IdENUM.Cas, TopRecordsOnly = top };
            if (!string.IsNullOrWhiteSpace(search)) mq.SearchString = search;
            return f.p41ProjectBL.GetList(mq)?.ToList() ?? new List<BO.p41Project>();
        }

        /// <summary>Projekt podle ID – jen když do něj uživatel smí vykazovat hodiny. Jinak null.</summary>
        public static BO.p41Project Project(BL.Factory f, int p41id)
        {
            if (p41id <= 0) return null;
            var mq = new BO.myQueryP41("p41") { j02id_query = f.CurrentUser.pid, p33id_for_p31_entry = (int)BO.p33IdENUM.Cas, pids = new List<int> { p41id } };
            return f.p41ProjectBL.GetList(mq)?.FirstOrDefault(p => p.pid == p41id);
        }

        /// <summary>Časové sešity, které projekt nabízí při vykazování.</summary>
        public static List<BO.p34ActivityGroup> TimeSheets(BL.Factory f, BO.p41Project recP41)
        {
            return f.p34ActivityGroupBL.GetList_WorksheetEntryIn_OneProject(recP41, f.CurrentUser.pid)?
                .Where(p => p.p33ID == BO.p33IdENUM.Cas).ToList() ?? new List<BO.p34ActivityGroup>();
        }

        /// <summary>Aktivity časových sešitů projektu (se zúžením podle klastru aktivit projektu, pokud nějaké zbydou).</summary>
        public static List<BO.p32Activity> Activities(BL.Factory f, BO.p41Project recP41, List<BO.p34ActivityGroup> sheets)
        {
            var ids = sheets.Select(p => p.pid).ToHashSet();
            int p61id = recP41.p61ID > 0 ? recP41.p61ID : recP41.p61ID_Byp42ID;
            var lis = f.p32ActivityBL.GetList(new BO.myQueryP32 { p41id = recP41.pid, p61id = p61id })?.Where(p => ids.Contains(p.p34ID)).ToList() ?? new List<BO.p32Activity>();
            if (lis.Count == 0 && p61id > 0)
            {
                lis = f.p32ActivityBL.GetList(new BO.myQueryP32 { p41id = recP41.pid })?.Where(p => ids.Contains(p.p34ID)).ToList() ?? new List<BO.p32Activity>();
            }
            return lis.OrderBy(p => p.p34ID).ThenBy(p => p.p32Ordinary).ThenBy(p => p.p32Name).ToList();
        }

        public static string ProjectName(BO.p41Project p) => p.PrefferedName;

        /// <summary>Projekt zakazuje vykazovat hodiny (p41WorksheetOperFlag 1, 2) – stejné pravidlo jako p31_test_beforesave.</summary>
        public static bool IsHoursForbidden(BO.p41Project p) =>
            p.p41WorksheetOperFlag == BO.p41WorksheetOperFlagEnum.ZakazVykazovat || p.p41WorksheetOperFlag == BO.p41WorksheetOperFlagEnum.ZakazVykazovatHodiny;

        /// <summary>V projektu lze hodiny vykázat jen přes úkol (p41WorksheetOperFlag 3, 4).</summary>
        public static bool IsTaskRequired(BO.p41Project p) =>
            p.p41WorksheetOperFlag == BO.p41WorksheetOperFlagEnum.UkolPovinnyUkony || p.p41WorksheetOperFlag == BO.p41WorksheetOperFlagEnum.UkolPovinnyHodiny;

        /// <summary>Aktivita s ručně zadávaným honorářem (p32ManualFeeFlag = 1) – nástroj částku honoráře nezadává, takže ji nenabízí.</summary>
        public static bool IsManualFee(BO.p32Activity a) => a.p32ManualFeeFlag == 1;

        /// <summary>
        /// Hodiny od modelu ("1.5", "1,5", "1:30", 1.5) → minuty. Nezávislé na národním nastavení serveru
        /// (BO.Code.Time.ConvertTimeToSeconds čte desetinné číslo podle kultury serveru).
        /// </summary>
        public static bool TryParseHours(string s, out int minutes)
        {
            minutes = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim().ToLowerInvariant().Replace("hod.", "").Replace("hod", "").Replace("h", "").Trim();
            if (s.Contains(':'))
            {
                var arr = s.Split(':');
                if (arr.Length != 2 || !int.TryParse(arr[0], NumberStyles.None, CultureInfo.InvariantCulture, out int h) || !int.TryParse(arr[1], NumberStyles.None, CultureInfo.InvariantCulture, out int m) || m > 59) return false;
                minutes = h * 60 + m;
            }
            else
            {
                if (!double.TryParse(s.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double d)) return false;
                minutes = (int)Math.Round(d * 60, MidpointRounding.AwayFromZero);
            }
            return minutes > 0 && minutes <= 24 * 60;
        }

        /// <summary>Čas dne "8:30" / "08:30" → "08:30".</summary>
        public static bool TryParseClock(string s, out string hhmm)
        {
            hhmm = null;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var arr = s.Trim().Split(':');
            if (arr.Length != 2 || !int.TryParse(arr[0], NumberStyles.None, CultureInfo.InvariantCulture, out int h) || !int.TryParse(arr[1], NumberStyles.None, CultureInfo.InvariantCulture, out int m) || h > 23 || m > 59) return false;
            hhmm = $"{h:00}:{m:00}";
            return true;
        }

        public static string HHMM(int minutes) => $"{minutes / 60:00}:{minutes % 60:00}";

        private static readonly string[] _days = { "neděle", "pondělí", "úterý", "středa", "čtvrtek", "pátek", "sobota" };
        public static string DateText(DateTime d) => $"{_days[(int)d.DayOfWeek]} {d:d.M.yyyy}";
    }
}
