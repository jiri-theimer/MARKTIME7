
using Microsoft.AspNetCore.Mvc;


namespace UI.Controllers
{
    public class HtmlTable2XlsController : BaseController
    {
        public IActionResult Index(string guid)
        {
            var recP85 = Factory.p85TempboxBL.LoadByGuid(guid);

            var arr = recP85.p85Message.Split("$$$$$$$");
            var def = new BL.Code.HtmlTable() { Sql = arr[1], ColHeaders = arr[0], ColTypes = recP85.p85FreeText02, ColFlexSubtotals = recP85.p85FreeText03, ColStyles = recP85.p85FreeText04, ColClasses = recP85.p85FreeText05, HeaderBgColor = recP85.p85FreeText06 };
            if (string.IsNullOrEmpty(def.ColHeaders))
            {
                def.ColHeaders = recP85.p85FreeText01;
            }
            var dt = Factory.gridBL.GetListFromPureSql(def.Sql);

            string strTempFileName = new BL.Code.Datatable2Html(def, Factory).CreateXlsFile(dt);


            Response.Headers["Content-Disposition"] = string.Format("inline; filename={0}", strTempFileName);
            var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(Factory.TempFolder + "\\" + strTempFileName), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return fileContentResult;
        }
    }
}
