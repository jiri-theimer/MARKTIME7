using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using UI.Controllers;

namespace UI.Views.Shared.Components.myPivot
{
    /// <summary>Serverové služby MARKTIME pivotu (myPivot) – export do Excelu.</summary>
    public class myPivotController : BaseController
    {
        public class ExportRow
        {
            public int l { get; set; }              // úroveň (odsazení)
            public string t { get; set; }           // popisek řádku
            public bool b { get; set; }             // tučně (celkový součet)
            public List<double?> v { get; set; }    // hodnoty
        }
        public class ExportInput
        {
            public string title { get; set; }
            public List<List<string>> headers { get; set; }   // řádky hlavičky; první buňka = popisek sloupce řádků
            public List<ExportRow> rows { get; set; }
            public List<int> decimals { get; set; }           // počet desetinných míst pro každý sloupec hodnot
        }

        /// <summary>Tabulka pivotu (vždy se všemi rozbalenými úrovněmi) → XLSX. Volá mypivot.js (POST JSON v těle požadavku – formulářové pole má limit 4 MB).</summary>
        [HttpPost]
        public IActionResult ExportExcel([FromBody] ExportInput d)
        {
            if (d?.rows == null) return BadRequest();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Pivot");
            int r = 1;
            if (!string.IsNullOrEmpty(d.title))
            {
                ws.Cell(r, 1).Value = d.title;
                ws.Cell(r, 1).Style.Font.Bold = true;
                ws.Cell(r, 1).Style.Font.FontSize = 13;
                r += 2;
            }

            int headerFrom = r;
            foreach (var hr in d.headers ?? new())
            {
                for (int c = 0; c < hr.Count; c++) ws.Cell(r, c + 1).Value = hr[c] ?? "";
                r++;
            }
            if (r > headerFrom)
            {
                var hrng = ws.Range(headerFrom, 1, r - 1, Math.Max(1, (d.headers ?? new()).Max(p => p.Count)));
                hrng.Style.Font.Bold = true;
                hrng.Style.Fill.BackgroundColor = XLColor.FromHtml("#EDEDED");
            }
            int dataFrom = r;

            foreach (var row in d.rows)
            {
                var lc = ws.Cell(r, 1);
                lc.Value = row.t ?? "";
                if (row.l > 0) lc.Style.Alignment.Indent = Math.Min(row.l, 15);
                for (int c = 0; c < (row.v?.Count ?? 0); c++)
                {
                    var cell = ws.Cell(r, c + 2);
                    if (row.v[c].HasValue)
                    {
                        cell.Value = row.v[c].Value;
                        int dec = d.decimals != null && c < d.decimals.Count ? d.decimals[c] : 2;
                        cell.Style.NumberFormat.Format = dec <= 0 ? "#,##0" : "#,##0." + new string('0', dec);
                    }
                }
                if (row.b) ws.Row(r).Style.Font.Bold = true;
                r++;
            }

            ws.Column(1).AdjustToContents(dataFrom, r, 10, 80);
            for (int c = 2; c <= ws.LastColumnUsed()?.ColumnNumber(); c++) ws.Column(c).AdjustToContents(headerFrom, r, 8, 40);
            ws.SheetView.FreezeRows(dataFrom - 1);
            ws.SheetView.FreezeColumns(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            string fn = "Soucty_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fn);
        }
    }
}
