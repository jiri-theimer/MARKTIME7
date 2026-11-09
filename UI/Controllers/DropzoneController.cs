using ceTe.DynamicPDF.PageElements;
using Microsoft.AspNetCore.Mvc;
using Rebex.Mail;

using System.Web;
using UI.Models;

namespace UI.Controllers
{
    public class DropzoneController : BaseController
    {
        /// <summary>Povolené přípony – stejné jako acceptedFiles v Views/Shared/_dropzone.cshtml (prohlížeč filtruje jen pro pohodlí, rozhoduje server).</summary>
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".msg", ".eml", ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".tif", ".tiff", ".heic", ".svg",
            ".xls", ".xlsx", ".doc", ".docx", ".txt", ".zip", ".rar", ".7z", ".ppt", ".pptx", ".csv",
            ".mp3", ".mp4", ".avi", ".html", ".htm", ".odt", ".ods", ".rtf", ".ics", ".isdoc", ".isdocx", ".xml"
        };
        public const long MaxFileBytes = 20L * 1024 * 1024;     //= maxFilesize v _dropzone.cshtml
        /// <summary>Přípony, které se z temp nikdy nevracejí „inline“ (prohlížeč by je vykreslil v kontextu aplikace).</summary>
        private static readonly HashSet<string> NeverInline = new(StringComparer.OrdinalIgnoreCase) { ".html", ".htm", ".svg", ".xml", ".xhtml" };
        private static readonly System.Text.RegularExpressions.Regex SafeUid = new("^[A-Za-z0-9-]{1,64}$");

        public IActionResult UploadInFrame(string guid, string entity, int recpid)
        {
            if (string.IsNullOrEmpty(entity) || string.IsNullOrEmpty(guid))
            {
                return this.StopPageSubform("entity or guid is missing");
            }

            var v = new BaseViewModel();

            ViewData["pid"] = recpid;
            ViewData["entity"] = entity;
            ViewData["guid"] = guid;

            return View(v);
        }

        [HttpPost]
        [DisableRequestSizeLimit] // volitelné – když řešíš větší soubory
        public async Task<IActionResult> UploadFiles()
        {
            if (!Request.HasFormContentType)
                return BadRequest("Request není multipart/form-data");

            var tempguid = Request.Form["tempguid"];
            var dropzoneuids = Request.Form["dropzone_uids"];
            var dropzonenames = Request.Form["dropzone_names"];
            var isautosave = BO.Code.Bas.BG(Request.Form["isautosave"]);
            var recprefix = Request.Form["recprefix"];
            var recpid = BO.Code.Bas.InInt(Request.Form["recpid"]);

            var files = Request.Form.Files;

            if (files == null || files.Count == 0)
                return BadRequest("Žádné soubory nebyly přijaty.");

            var rejected = new List<string>();
            for (int x = 0; x < files.Count; x++)    //index i u prázdného/odmítnutého souboru – uid a názvy z dropzone jsou ve stejném pořadí
            {
                var file = files[x];
                if (file.Length == 0)
                    continue;

                string ext = System.IO.Path.GetExtension(file.FileName ?? "");
                if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext) || file.Length > MaxFileBytes)
                {
                    rejected.Add(System.IO.Path.GetFileName(file.FileName ?? ""));
                    continue;
                }

                //uid z prohlížeče jde do názvu souboru na disku → jen bezpečné znaky (jinak path traversal), jinak nové
                string fileuid = x < dropzoneuids.Count ? dropzoneuids[x] : null;
                if (string.IsNullOrEmpty(fileuid) || !SafeUid.IsMatch(fileuid)) fileuid = Guid.NewGuid().ToString();
                string filename = x < dropzonenames.Count ? dropzonenames[x] : null;
                var archivefilename = fileuid + ext.ToLowerInvariant();


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


                if (System.IO.Path.GetExtension(file.FileName).ToLower()==".msg")
                {
                    var msg = Factory.o27AttachmentBL.LoadMsgFile(archiveDestPath);
                    rec.p85Prefix = "msg";
                    rec.p85FreeText05 = msg.Subject;                    
                }
                if (System.IO.Path.GetExtension(file.FileName).ToLower() == ".eml")
                {
                    var eml = Factory.o27AttachmentBL.LoadMsgFile(archiveDestPath);
                    rec.p85Prefix = "eml";
                    rec.p85FreeText05 = eml.Subject;
                }

                var p85id = Factory.p85TempboxBL.Save(rec);
            }

            if (isautosave)
            {
                SaveToFilebox(tempguid,recprefix,recpid);
            }

            return Ok(new { count = files.Count - rejected.Count, rejected });
        }

        
        public IActionResult SaveToFilebox(string tempguid,string recprefix,int recpid)
        {
            var bolOK = Factory.o27AttachmentBL.SaveDropzoneFromTemp(tempguid, recprefix, recpid);
            if (bolOK)
            {
                return Ok(new { ok = true });
            }
            else
            {
                return BadRequest(new { ok = false });
            }
            
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
            //typ podle přípony, ne podle Content-Type z prohlížeče; HTML/SVG/XML jen ke stažení (jinak by se vykreslily v kontextu aplikace)
            string ext = System.IO.Path.GetExtension(rec.p85FreeText03 ?? "");
            if (!new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(rec.p85FreeText03 ?? "", out string contenttype))
            {
                contenttype = "application/octet-stream";
            }
            string disposition = NeverInline.Contains(ext) ? "attachment" : "inline";
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            

            if (!System.IO.File.Exists(strFullPath))
            {
                return NotFound();
            }
            //Response.Headers["Content-Disposition"] = string.Format("inline; filename={0}", rec.p85FreeText01);

            //Response.Headers["Content-Disposition"] =$"inline; filename=\"{rec.p85FreeText01}\"; filename*=UTF-8''{Uri.EscapeDataString(rec.p85FreeText01)}";

            Response.Headers["Content-Disposition"] = $"{disposition}; filename*=UTF-8''{HttpUtility.UrlEncode(rec.p85FreeText01)}";


            var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(strFullPath), contenttype);
            

            return fileContentResult;
        }



        

    }
}
