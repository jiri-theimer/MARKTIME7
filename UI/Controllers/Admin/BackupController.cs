using Microsoft.AspNetCore.Mvc;
using UI.Models;
using ClosedXML.Excel;
using BO;
using System.Data;
using System.IO.Compression;


namespace UI.Controllers.Admin
{
    public class BackupController : BaseController
    {
        public BackupController()
        {

        }
        public IActionResult Index()
        {
            var v = new BaseViewModel();
            RefreshState();
            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState()
        {
            ViewBag.pwd = BO.Code.Bas.GetGuid().Substring(0, 5);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(BaseViewModel v)
        {
            RefreshState();
            if (v.IsPostback)
            {
                if (v.PostbackOper.Contains("generate|"))
                {
                    var arr = v.PostbackOper.Split("|");
                    ViewBag.pwd = arr[1].Trim();
                    ViewBag.apikey = arr[2];

                    if (string.IsNullOrEmpty(ViewBag.apikey) || ViewBag.apikey != Factory.Lic.x01ApiKey)
                    {
                        this.AddMessage("API key není správný."); return View(v);
                    }

                    ViewBag.backupfile = handle_export(ViewBag.pwd);
                    
                    this.AddMessage("Soubor zálohy byl vygenerován.", "info");
                    
                }
                

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

        private string handle_export(string pwd)
        {
            var workbook = new XLWorkbook();

            var files = BO.Code.File.GetFileNamesInDir($"{Factory.App.RootUploadFolder}\\_distribution\\dynamicsql", "backup_*.sql", true);
            foreach (string file in files)
            {
                var dt = Factory.FBL.GetDataTable(BO.Code.File.GetFileContent(file));
                string strListName = BO.Code.File.GetFileInfo(file).Name.Replace("backup_", "").Replace(".sql", "");
                handle_one_export(dt, workbook, strListName, pwd);
            }
            string strRet = $"{BO.Code.Bas.GetGuid()}.xlsx";
            string strDestFile = $"{Factory.TempFolder}\\{strRet}";

            workbook.SaveAs(strDestFile);

            return strRet;
    }

        private void handle_one_export(DataTable dt,XLWorkbook workbook,string strNewListName,string strPwd)
        {
            var worksheet = workbook.Worksheets.Add(strNewListName);
            if (!string.IsNullOrEmpty(strPwd))
            {
                worksheet.Protect(strPwd).DisallowElement(XLSheetProtectionElements.Everything);
            }
            
            
            int row = 1;
            int col = 1;
            
            var coltypes = new List<StringPair>();
            foreach (System.Data.DataColumn c in dt.Columns)
            {
                coltypes.Add(new BO.StringPair() { Key = c.ColumnName, Value = c.DataType.Name });
                worksheet.Cell(row, col).Value = c.ColumnName;
                col += 1;
            }

            row += 1;
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                col = 1;
                foreach (var c in coltypes)
                {

                    if (!Convert.IsDBNull(dr[c.Key]))
                    {
                        switch (dr[c.Key].GetType().ToString())
                        {

                            case "System.DateTime":
                                worksheet.Cell(row, col).Value = Convert.ToDateTime(dr[c.Key]);
                                break;
                            case "System.Double":
                                worksheet.Cell(row, col).Value = Convert.ToDouble(dr[c.Key]);
                                break;
                            case "System.Int32":
                                worksheet.Cell(row, col).Value = Convert.ToInt32(dr[c.Key]);
                                break;
                            case "System.Boolean":
                                worksheet.Cell(row, col).Value = Convert.ToBoolean(dr[c.Key]);
                                break;
                            default:
                                worksheet.Cell(row, col).Value = dr[c.Key].ToString();
                                break;
                        }




                    }
                    col += 1;
                }

                row += 1;

            }


        }
    }
}
