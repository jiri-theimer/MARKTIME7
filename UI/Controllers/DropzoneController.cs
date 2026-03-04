using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace UI.Controllers
{
    public class DropzoneController : BaseController
    {
        [HttpPost]
        [DisableRequestSizeLimit] // volitelné – když řešíš větší soubory
        public async Task<IActionResult> UploadFiles()
        {
            if (!Request.HasFormContentType)
                return BadRequest("Request není multipart/form-data");

            var tempguid = Request.Form["tempguid"];
            var dropzoneuids = Request.Form["dropzone_uids"];
            var dropzonenames = Request.Form["dropzone_names"];

            var files = Request.Form.Files;

            if (files == null || files.Count == 0)
                return BadRequest("Žádné soubory nebyly přijaty.");

            int x = 0;
            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;


                var fileuid = dropzoneuids[x];  //uid přidělené z dropzone
                var filename = dropzonenames[x];
                var archivefilename = fileuid + System.IO.Path.GetExtension(file.FileName);


                var archiveDestPath = System.IO.Path.Combine(Factory.TempFolder, archivefilename);

                using (var stream = new FileStream(archiveDestPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var rec = new BO.p85Tempbox() { p85GUID = tempguid, p85FreeText01 = file.FileName, p85FreeText02 = file.ContentType, p85FreeText03 = archivefilename, p85FreeNumber01 = file.Length };
                if (filename == file.FileName)
                {
                    rec.p85FreeText04 = fileuid;
                }
                var p85id = Factory.p85TempboxBL.Save(rec);
                x += 1;
            }

            return Ok(new { count = files.Count });
        }


        public IActionResult DeleteTempFile(string filename, string fileuid, string tempguid)
        {
            if (string.IsNullOrEmpty(tempguid))
            {
                return BadRequest("Chybí tempguid");
            }
            if (string.IsNullOrEmpty(filename) && string.IsNullOrEmpty(fileuid))
            {
                return BadRequest("Chybí fileuid nebo filename");
            }

            string strPath = null;

            var lis = Factory.p85TempboxBL.GetList(tempguid);
            BO.p85Tempbox rec = null;
            if (!string.IsNullOrEmpty(fileuid))
            {
                rec = lis.FirstOrDefault(p => p.p85FreeText04 == fileuid);
                if (rec is null)
                {
                    return BadRequest($"Soubor s UID [{fileuid}] neexistuje");
                }
                strPath = $"{Factory.TempFolder}\\{rec.p85FreeText03}";
            }
            else
            {
                rec = lis.FirstOrDefault(p => p.p85FreeText01 == filename);
                if (rec is null)
                {
                    return BadRequest($"Soubor [{filename}] neexistuje");
                }
                strPath = $"{Factory.TempFolder}\\{rec.p85FreeText03}";
            }

            if (!System.IO.File.Exists(strPath)) return NotFound();
            System.IO.File.Delete(strPath);


            Factory.p85TempboxBL.VirtualDelete(rec.pid);

            return Ok(new { ok = true });
        }


        public ActionResult DownloadTempFile(string tempfilename,string tempguid)
        {
            var lis = Factory.p85TempboxBL.GetList(tempguid);
            var rec = lis.FirstOrDefault(p => p.p85FreeText03 == tempfilename);

            if (rec == null)
            {
                return NotFound();
            }

            string strFullPath = $"{Factory.TempFolder}\\{rec.p85FreeText03}";
            string contenttype = rec.p85FreeText02;

            if (string.IsNullOrEmpty(contenttype)) contenttype = "application/octet-stream";

            

            if (!System.IO.File.Exists(strFullPath))
            {
                return NotFound();
            }
            //Response.Headers["Content-Disposition"] = string.Format("inline; filename={0}", rec.p85FreeText01);

            //Response.Headers["Content-Disposition"] =$"inline; filename=\"{rec.p85FreeText01}\"; filename*=UTF-8''{Uri.EscapeDataString(rec.p85FreeText01)}";

            Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{HttpUtility.UrlEncode(rec.p85FreeText01)}";


            var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(strFullPath), contenttype);
            

            return fileContentResult;
        }
    }
}
