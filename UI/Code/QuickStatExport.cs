using ClosedXML.Excel;

namespace UI.Code
{
    /// <summary>Rychlá statistika úkonů záznamu jako XLSX (stažení i příloha e-mailu). Bere stejné nastavení jako box statistiky: seskupení, druhou úroveň a období.</summary>
    public class QuickStatExport
    {
        public string Title { get; set; }       //Rychlá statistika: název záznamu
        public string FileName { get; set; }
        public byte[] Bytes { get; set; }

        /// <summary>Nabídka seskupení pro daný typ záznamu (stejná pro první i druhou úroveň).</summary>
        public static List<BO.StringPair> GetOptions(BL.Factory f, string prefix)
        {
            var lis = new List<BO.StringPair>();
            if (prefix != "p41") lis.Add(new BO.StringPair() { Key = "p41", Value = f.tra("Projekt") });
            if (prefix == "j02")
            {
                lis.Add(new BO.StringPair() { Key = "p28", Value = f.tra("Klient projektu") });
            }
            else
            {
                lis.Add(new BO.StringPair() { Key = "j02", Value = f.tra("Uživatel") });
            }
            lis.Add(new BO.StringPair() { Key = "p32", Value = f.tra("Aktivita") });
            lis.Add(new BO.StringPair() { Key = "p34", Value = f.tra("Sešit") });
            lis.Add(new BO.StringPair() { Key = "p95", Value = f.tra("Fakturační oddíl") });
            if (prefix != "p91") lis.Add(new BO.StringPair() { Key = "p91", Value = f.tra("Vyúčtování (Faktura)") });
            lis.Add(new BO.StringPair() { Key = "p31rate_billing_orig", Value = f.tra("Výchozí fakturační sazba") });
            if (prefix != "p56") lis.Add(new BO.StringPair() { Key = "p56", Value = f.tra("Úkol") });
            lis.Add(new BO.StringPair() { Key = "j27", Value = f.tra("Měna") });
            return lis;
        }

        public static QuickStatExport Create(BL.Factory f, BL.Singleton.ThePeriodProvider pp, string prefix, int pid)
        {
            var opts = GetOptions(f, prefix);
            string statby = f.CBL.LoadUserParam($"recpage-{prefix}-statprefix", prefix == "j02" ? "p28" : "j02");
            if (!opts.Any(p => p.Key == statby)) statby = (prefix == "j02" ? "p28" : "j02");
            string statby2 = f.CBL.LoadUserParam($"recpage-{prefix}-statprefix2", null);
            if (statby2 == statby || !opts.Any(p => p.Key == statby2)) statby2 = null;

            var per = new UI.Views.Shared.Components.myPeriod.myPeriodViewModel() { UserParamKey = "tabstat-period" };
            per.LoadUserSetting(pp, f);
            DateTime? d1 = null; DateTime? d2 = null; string strPeriod = null;
            if (per.PeriodValue > 0 && per.d1 != null && per.d2 != null)
            {
                d1 = per.d1; d2 = per.d2; strPeriod = per.PeriodAlias;
            }

            var rows = new List<BO.p31QuickStat>();
            var qry = f.p31WorksheetBL.GetList_QuickStat(statby, prefix, pid, d1, d2, null, statby2);
            if (qry != null) rows = qry.OrderBy(p => p.groupby_alias).ThenBy(p => p.groupby2_alias).ToList();

            string strAlias = f.CBL.GetObjectAlias(BO.Code.Entity.GetPrefixDb(prefix), pid);
            var ret = new QuickStatExport() { Title = f.tra("Rychlá statistika") + ": " + strAlias };
            string strNoName = f.tra("(bez zařazení)");
            string strGrouping = opts.First(p => p.Key == statby).Value;
            if (statby2 != null) strGrouping += " › " + opts.First(p => p.Key == statby2).Value;

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Statistika");
            int r = 1;
            ws.Cell(r, 1).Value = ret.Title;
            ws.Cell(r, 1).Style.Font.Bold = true;
            ws.Cell(r, 1).Style.Font.FontSize = 13;
            r++;
            ws.Cell(r, 1).Value = (strPeriod == null ? "" : strPeriod + " | ") + f.tra("Seskupit podle") + ": " + strGrouping;
            r += 2;

            //hlavička: řádek skupin sloupců + řádek sloupců
            int rh = r;
            ws.Cell(r, 2).Value = f.tra("Vykázáno"); ws.Range(r, 2, r, 4).Merge();
            ws.Cell(r, 5).Value = f.tra("Nevyúčtováno"); ws.Range(r, 5, r, 6).Merge();
            ws.Cell(r, 7).Value = f.tra("Vyúčtováno"); ws.Range(r, 7, r, 10).Merge();
            r++;
            string[] heads = { strGrouping, f.tra("Hodiny"), f.tra("Částka"), f.tra("Měna"), f.tra("Hodiny"), f.tra("Částka"), f.tra("Hodiny"), f.tra("V paušálu"), f.tra("Odepsáno"), f.tra("Částka"), f.tra("Počet úkonů") };
            for (int c = 0; c < heads.Length; c++) ws.Cell(r, c + 1).Value = heads[c];
            var hrng = ws.Range(rh, 1, r, heads.Length);
            hrng.Style.Font.Bold = true;
            hrng.Style.Fill.BackgroundColor = XLColor.FromHtml("#EDEDED");
            ws.Range(rh, 2, r, heads.Length).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            r++;
            int rdata = r;

            if (statby2 == null)
            {
                foreach (var c in rows)
                {
                    WriteRow(ws, r, string.IsNullOrEmpty(c.groupby_alias) ? strNoName : c.groupby_alias, new[] { c }, 0, false);
                    r++;
                }
            }
            else
            {
                foreach (var grp in rows.GroupBy(p => p.groupby_pid + "|" + p.groupby_alias))
                {
                    string strName = grp.First().groupby_alias;
                    WriteRow(ws, r, string.IsNullOrEmpty(strName) ? strNoName : strName, grp, 0, true);    //mezisoučet skupiny
                    r++;
                    foreach (var c in grp)
                    {
                        WriteRow(ws, r, string.IsNullOrEmpty(c.groupby2_alias) ? strNoName : c.groupby2_alias, new[] { c }, 1, false);
                        ws.Row(r).OutlineLevel = 1;     //v Excelu jde skupina sbalit
                        r++;
                    }
                }
                ws.Outline.SummaryVLocation = XLOutlineSummaryVLocation.Top;
            }

            //celkem: částky ve vykázané měně se nesčítají napříč měnami
            var curs = rows.Select(p => p.j27code_orig).Where(p => !string.IsNullOrEmpty(p)).Distinct().OrderBy(p => p).ToList();
            int rtotal = r;
            WriteRow(ws, r, f.tra("Celkem"), rows, 0, true);
            r++;
            if (curs.Count > 1)
            {
                foreach (string cur in curs)
                {
                    var lis = rows.Where(p => p.j27code_orig == cur).ToList();
                    ws.Cell(r, 1).Value = f.tra("Celkem") + " " + cur;
                    SetNum(ws, r, 3, lis.Sum(p => p.bezdph_vyka));
                    ws.Cell(r, 4).Value = cur;
                    SetNum(ws, r, 6, lis.Sum(p => p.nevyuct_castka));
                    ws.Row(r).Style.Font.Bold = true;
                    r++;
                }
            }
            ws.Range(rtotal, 1, rtotal, heads.Length).Style.Border.TopBorder = XLBorderStyleValues.Thin;

            ws.Range(rdata, 2, r, 10).Style.NumberFormat.Format = "#,##0.00";
            ws.Column(4).Style.NumberFormat.Format = "@";
            ws.Column(1).AdjustToContents(rh, r, 12, 70);
            for (int c = 2; c <= heads.Length; c++) ws.Column(c).AdjustToContents(rh, r, 9, 30);
            ws.SheetView.FreezeRows(rdata - 1);
            ws.SheetView.FreezeColumns(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ret.Bytes = ms.ToArray();

            string strSafe = string.Join("_", (strAlias ?? "").Split(Path.GetInvalidFileNameChars().Concat(new[] { ' ', ',', ';', '&', '#', '%', '+' }).ToArray(), StringSplitOptions.RemoveEmptyEntries));
            if (strSafe.Length > 40) strSafe = strSafe.Substring(0, 40);
            ret.FileName = "Statistika_" + (strSafe == "" ? "" : strSafe + "_") + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";

            return ret;
        }

        private static void SetNum(IXLWorksheet ws, int r, int c, double val)
        {
            if (val != 0) ws.Cell(r, c).Value = val;
        }

        private static void WriteRow(IXLWorksheet ws, int r, string strName, IEnumerable<BO.p31QuickStat> lis, int intIndent, bool bolBold)
        {
            ws.Cell(r, 1).Value = strName ?? "";
            if (intIndent > 0) ws.Cell(r, 1).Style.Alignment.Indent = intIndent * 2;

            var curs = lis.Select(p => p.j27code_orig).Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
            SetNum(ws, r, 2, lis.Sum(p => p.hvyk));
            if (curs.Count <= 1)
            {
                //částky jen tehdy, když je řádek v jedné měně
                SetNum(ws, r, 3, lis.Sum(p => p.bezdph_vyka));
                if (curs.Count == 1) ws.Cell(r, 4).Value = curs[0];
                SetNum(ws, r, 6, lis.Sum(p => p.nevyuct_castka));
            }
            SetNum(ws, r, 5, lis.Sum(p => p.nevyuct_h));
            SetNum(ws, r, 7, lis.Sum(p => p.hvyf));
            SetNum(ws, r, 8, lis.Sum(p => p.hvyf6));
            SetNum(ws, r, 9, lis.Sum(p => p.hvyf23));
            SetNum(ws, r, 10, lis.Sum(p => p.bezdph_vyfa));
            ws.Cell(r, 11).Value = lis.Sum(p => p.pocet);
            if (bolBold) ws.Row(r).Style.Font.Bold = true;
        }
    }
}
