
using Microsoft.AspNetCore.Mvc;

using UI.Models;

using System.Web;


namespace UI.Controllers
{

    public class FileUploadController : BaseController
    {

        public IActionResult Index(string guid, string entity, int recpid)
        {
            var v = new FileUploadViewModel() { Guid = guid, Entity = entity, RecPid = recpid };
            if (entity != null)
            {
                var ce = Factory.EProvider.ByPrefix(entity);
                if (ce == null)
                {
                    return this.StopPageSubform("entity not found");
                }

                v.Entity = ce.Prefix;
            }
            if (v.RecPid > 0)
            {
                var mq = new BO.myQueryO27() { entity = v.Entity, recpid = v.RecPid, tempguid = v.Guid };
                v.lisO27 = Factory.o27AttachmentBL.GetList(mq);

            }
            if (Factory.o27AttachmentBL.GetTempFiles(guid).Count() > 0)
            {
                return RedirectToAction("DoUpload", new { guid = v.Guid, Entity = v.Entity, recpid = v.RecPid });
            }
            else
            {
                return View(v);
            }

        }
        public IActionResult SingleUpload(string guid,string javascript_after_upload)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return this.StopPageSubform("guid missing");
            }
            var v = new FileUploadSingleViewModel() { Guid = guid,JavascriptParentSite= javascript_after_upload };
            v.lisTempFiles = Factory.o27AttachmentBL.GetTempFiles(v.Guid);
            return View(v);
        }
        [HttpPost]
        public async Task<IActionResult> SingleUpload(FileUploadSingleViewModel v, List<IFormFile> files)
        {

            var tempDir = Factory.TempFolder + "\\";

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    // full path to file in temp location
                    //var strTempFullPath = Path.GetTempFileName();
                    //příklad infox: image/png|99330|2019-11-13_104250.png|8ab14290cd8d4a929d518d2f9e663ecd_2019-11-13_104250.png|8ab14290cd8d4a929d518d2f9e663ecd|1|Popisek|Číslo jednací|101

                    string ss = BO.Code.File.ConvertToSafeFileName(formFile.FileName);
                    
                    var strTempFullPath = tempDir + v.Guid + "_" + ss;


                    System.IO.File.WriteAllText(tempDir + v.Guid + ".infox", formFile.ContentType + "|" + formFile.Length.ToString() + "|" + ss + "|" + v.Guid + "_" + ss + "|" + v.Guid + "|0|||0");


                    using (var stream = new FileStream(strTempFullPath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            v.lisTempFiles = Factory.o27AttachmentBL.GetTempFiles(v.Guid);

            if (v.JavascriptParentSite != null)
            {
                v.SetJavascript_CallOnLoad(1, "upload", v.JavascriptParentSite);
            }
            
            return View(v);

        }

        public IActionResult DoUpload(string guid, string entity, int recpid)
        {
            if (string.IsNullOrEmpty(entity) || string.IsNullOrEmpty(guid))
            {
                return this.StopPageSubform("entity or guid is missing");
            }

            var v = new FileUploadViewModel() { Guid = guid, Entity = entity, RecPid = recpid };


            RefreshStateDoUpload(v);

            return View(v);
        }
        [HttpPost]
        public async Task<IActionResult> DoUpload(FileUploadViewModel v, List<IFormFile> files,string oper)
        //public IActionResult DoUpload(FileUploadViewModel v, List<IFormFile> files, string oper)
        {
            if (oper == "postback")
            {
                RefreshStateDoUpload(v);
                return View(v);
            }
            if (files == null)
            {
                RefreshStateDoUpload(v);
                return View(v);
            }

            var tempDir = Factory.TempFolder + "\\";
            
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    // full path to file in temp location
                    //var strTempFullPath = Path.GetTempFileName();
                    //příklad infox: image/png|99330|2019-11-13_104250.png|8ab14290cd8d4a929d518d2f9e663ecd_2019-11-13_104250.png|8ab14290cd8d4a929d518d2f9e663ecd|1|Popisek|Číslo jednací|101

                    var strTempFullPath = tempDir + v.Guid + "_" + formFile.FileName;
                    if (v.o27Name != null)
                    {
                        v.o27Name = v.o27Name.Replace("|", "");
                    }
                    else
                    {
                        v.o27Name = "";
                    }


                    System.IO.File.AppendAllText(tempDir + v.Guid + "_" + formFile.FileName + ".infox", formFile.ContentType + "|" + formFile.Length.ToString() + "|" + formFile.FileName + "|" + v.Guid + "_" + formFile.FileName + "|" + v.Guid + "|" + v.Entity + "|" + v.o27Name);


                    using (var stream = new FileStream(strTempFullPath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            RefreshStateDoUpload(v);

            
            // process uploaded files
            // Don't rely on or trust the FileName property without validation.
            return View(v);
            //return Ok(new { count = files.Count, size, tempfiles });
        }


        private void RefreshStateDoUpload(FileUploadViewModel v)
        {



            v.lisTempFiles = Factory.o27AttachmentBL.GetTempFiles(v.Guid);
            if (v.RecPid > 0)
            {

                v.lisO27 = Factory.o27AttachmentBL.GetList(new BO.myQueryO27() { entity = v.Entity, recpid = v.RecPid, tempguid = v.Guid });
            }
        }

        [HttpGet]
        public ActionResult FileDownloadInline(string guid)
        {
            var c = Factory.o27AttachmentBL.LoadByGuid(guid);
            
            string fullPath = Factory.UploadFolder + "\\" + c.o27ArchiveFolder + "\\" + c.o27ArchiveFileName;

            if (c.o27Entity == "x31" && !System.IO.File.Exists(fullPath))
            {
                //tisková sestava
                fullPath = Factory.App.RootUploadFolder + "\\_distribution\\trdx\\" + c.o27ArchiveFileName;
            }

            if (System.IO.File.Exists(fullPath))
            {
                Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{HttpUtility.UrlEncode(c.o27OriginalFileName)}";
                if (c.o27ContentType != null)
                {
                    var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(fullPath), c.o27ContentType);

                    return fileContentResult;
                }
                else
                {
                    return new FileContentResult(System.IO.File.ReadAllBytes(fullPath), "application/octet-stream");
                }
                
            }
            else
            {
                return FileDownloadNotFound(c);
            }

        }


        [HttpGet]
        public ActionResult FileDownloadImprint(string guid)
        {
            var c = Factory.p91InvoiceBL.GetList_p96(0, guid).First();

            string fullPath = $"{Factory.UploadFolder}\\p91\\PdfImprint\\{c.p96ArchiveFolder}\\{c.p96FileName}";
           

            if (System.IO.File.Exists(fullPath))
            {
                Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{HttpUtility.UrlEncode(c.p96FileName)}";
                var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(fullPath), "application/pdf");
                return fileContentResult;

            }
            else
            {
                return FileDownloadNotFound();
            }

        }


        //[HttpGet]
        public ActionResult LogoFile(string logofilename)
        {
            string fullPath = Factory.PluginsFolder + "\\" + logofilename;
            if (!System.IO.File.Exists(fullPath))
            {
                fullPath = Factory.App.WwwRootFolder + "\\PLUGINS\\company_logo_default.png";
            }
            
            if (System.IO.File.Exists(fullPath))
            {
                var bytes = System.IO.File.ReadAllBytes(fullPath);
                MemoryStream ms = new MemoryStream(bytes);
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;

                string strContentType = "image/" + BO.Code.File.GetFileInfo(fullPath).Extension.Replace(".", "").Replace("jpg", "jpeg");

                return new FileStreamResult(ms, strContentType);

            }
            else
            {
                return FileDownloadNotFound();
            }
        }

        public ActionResult FileDownloadInbox(int pid,string format)
        {
            //ve format je název souboru přilohy
            format = format.Trim();
            var files = Factory.o43InboxBL.GetInboxFiles(pid, false);
            var rec = Factory.o43InboxBL.Load(pid);

            if (files.Any(p => p.Key.ToLower() == format.ToLower()))
            {
                string strPath = files.First(p => p.Key.ToLower() == format.ToLower()).Value;
                
                if (format=="eml" || format == "msg")
                {                   
                    format = $"{rec.o43Subject}.{format}";
                }
                Response.Headers["Content-Disposition"] = $"inline; filename={format}";
                var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(strPath), BO.Code.File.GetContentType(strPath));
                return fileContentResult;
            }



            //var rec = Factory.o43InboxBL.Load(pid);
            //if (rec.o43ArchiveFolder == null) rec.o43ArchiveFolder = $"{rec.o43DateMessage.Value.Year}\\{rec.o43DateMessage.Value.Month}";
            //string strFolder = $"{Factory.UploadFolder}\\{rec.o43ArchiveFolder}";
            //if (rec.o43Subject == null) rec.o43Subject = "Zpráva";

            //if (format=="msg" || format=="eml")
            //{                
            //    Response.Headers["Content-Disposition"] = $"inline; filename={rec.o43Subject}.{format}";
            //    var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes($"{strFolder}\\{rec.o43MessageID}.{format}"), "application/octet-stream");
            //    return fileContentResult;
            //}
            
            ////ve format je název souboru přilohy
            //if (!string.IsNullOrEmpty(format))
            //{
            //    string strPath = $"{strFolder}\\{rec.o43MessageID}-{format}";
            //    if (System.IO.File.Exists(strPath))
            //    {
            //        Response.Headers["Content-Disposition"] = $"inline; filename={format}";
                    
            //        var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(strPath), BO.Code.File.GetContentType(strPath));
            //        return fileContentResult;
            //    }
            //}
            

            return FileDownloadNotFound();

        }

        public ActionResult FileDownloadTempFile(string tempfilename,bool directdownload)
        {
            if (!System.IO.File.Exists(Factory.TempFolder + "\\" + tempfilename))
            {
                return FileDownloadNotFound(new BO.o27Attachment() { o27OriginalFileName = tempfilename, o27ArchiveFolder = "TEMP" });
            }
            BO.o27Attachment recO27 = Factory.o27AttachmentBL.InhaleFileByInfox(Factory.TempFolder + "\\" + tempfilename + ".infox");
            if (recO27 == null)
            {
                //neexistuje infox soubor, ale temp file existuje
                recO27 = new BO.o27Attachment() { o27OriginalFileName = tempfilename };

            }
            

            if (string.IsNullOrEmpty(recO27.o27ContentType) || directdownload)
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Factory.TempFolder + "\\" + tempfilename);
                Response.Headers["Content-Type"] = "application/octet-stream";
                Response.Headers["Content-Length"] = fileBytes.Length.ToString();
                if (recO27.o27OriginalFileName != null)
                {
                    return File(fileBytes, "application/octet-stream", recO27.o27OriginalFileName);
                }
                else
                {
                    return File(fileBytes, "application/octet-stream", tempfilename);
                }
                
            }
            else
            {
                Response.Headers["Content-Disposition"] = string.Format("inline; filename={0}",BO.Code.File.ConvertToSafeFileName( recO27.o27OriginalFileName));
                var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(Factory.TempFolder + "\\" + tempfilename), recO27.o27ContentType);
                return fileContentResult;
            }

        }

        public ActionResult FileDownloadTempFileNDB(string tempfilename, string contenttype, string downloadfilename,string subfolder)
        {
            if (string.IsNullOrEmpty(downloadfilename)) downloadfilename = tempfilename;
            string strFullPath = $"{Factory.TempFolder}\\{tempfilename}";
            if (string.IsNullOrEmpty(contenttype)) contenttype = "application/octet-stream";

            if (!string.IsNullOrEmpty(subfolder))
            {
                strFullPath = $"{Factory.TempFolder}\\{subfolder}\\{tempfilename}";
            }

            if (!System.IO.File.Exists(strFullPath))
            {
                return FileDownloadNotFound(new BO.o27Attachment() { o27OriginalFileName = tempfilename, o27ArchiveFolder = "TEMP" });
            }
            Response.Headers["Content-Disposition"] = string.Format("inline; filename={0}", downloadfilename);
            var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(strFullPath), contenttype);
            return fileContentResult;
        }

        




        public ActionResult FileDownloadNotFound(BO.o27Attachment c)
        {
            var fullPath = Factory.TempFolder + "\\notfound.txt";
            System.IO.File.WriteAllText(fullPath, string.Format("Soubor [{0}] na serveru [??????\\{1}] neexistuje!", c.o27OriginalFileName, c.o27ArchiveFolder));
            Response.Headers["Content-Disposition"] = string.Format("inline; filename={0}", "notfound.txt");
            var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(fullPath), "text/plain");
            return fileContentResult;
        }
        public ActionResult FileDownloadNotFound()
        {
            var fullPath = Factory.TempFolder + "\\notfound.txt";
            System.IO.File.WriteAllText(fullPath, "File not exists!");
            Response.Headers["Content-Disposition"] = string.Format("inline; filename={0}", "notfound.txt");
            var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(fullPath), "text/plain");
            return fileContentResult;
        }

        public BO.Result ChangeTempFileLabel(string fileguid,string tempfilename, string newlabel)
        {
            if (string.IsNullOrEmpty(newlabel))
            {
                newlabel = "";
            }
            newlabel = newlabel.Replace("|", "");
            string strInfoxFullPath = Factory.TempFolder + "\\" + tempfilename + ".infox";
            var rec = Factory.o27AttachmentBL.InhaleFileByInfox(strInfoxFullPath);
            if (rec == null)
            {
                return new BO.Result(true, "infox is null");
            }
            
            //System.IO.File.WriteAllText(Factory.App.TempFolder + "\\" + tempfilename + ".popis", newlabel);
            System.IO.File.WriteAllText(strInfoxFullPath, rec.o27ContentType + "|" + rec.o27FileSize.ToString() + "|" + rec.o27OriginalFileName + "|" + fileguid + "_" + rec.o27OriginalFileName + "|" + fileguid + "|" + rec.o27Entity + "|" + newlabel);
           
            return new BO.Result(false);
        }
        public BO.Result DeleteTempFile(string filename,string guid)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                var rec = Factory.o27AttachmentBL.LoadByGuid(filename);
                if (rec != null)
                {
                    Factory.o27AttachmentBL.Move2Deleted(rec);
                    Factory.CBL.DeleteRecord("o27Attachment", rec.pid);
                    return new BO.Result(false,"Souborová příloha nenávratně odstraněna.");
                }
            }
            

            var files4delete = BO.Code.File.GetFileListFromDir(Factory.TempFolder, $"{guid}*.*", SearchOption.TopDirectoryOnly, true);
            foreach (var file in files4delete)
            {
                if (file ==$"{Factory.TempFolder}\\{filename}")
                {
                    System.IO.File.Delete(file);
                    return new BO.Result(false);
                }
                
            }

            return new BO.Result(true, "file not found");
        }
        public BO.Result ChangeFileLabel(string fileguid, string newlabel)
        {
            if (string.IsNullOrEmpty(newlabel) == true)
            {
                newlabel = "";
            }
            newlabel = newlabel.Replace("|", "");

            var rec = Factory.o27AttachmentBL.LoadByGuid(fileguid);
            if (rec == null)
            {
                return new BO.Result(true, "file not found");
            }
            rec.o27Name = newlabel;
            if (Factory.o27AttachmentBL.Save(rec) > 0)
            {
                return new BO.Result(false);
            }
            else
            {
                return new BO.Result(true, "Chyba");
            }

        }
        public bool ClearTempFiles(string guid)
        {
            var files4delete = BO.Code.File.GetFileListFromDir(Factory.TempFolder, $"{guid}*.*",SearchOption.TopDirectoryOnly,true);
            foreach (var file in files4delete)
            {
                System.IO.File.Delete(file);
            }

            //var rec = Factory.o27AttachmentBL.LoadByGuid(fileguid);
            //var recTemp = new BO.p85Tempbox() { p85GUID = guid, p85DataPID = rec.pid, p85FreeText01 = rec.o27Entity };
            //Factory.p85TempboxBL.Save(recTemp);
            //this.AddMessage("Odstranění přílohy je třeba potvrdit tlačítkem [Uložit změny]", "info");
            return true;

        }



    }
}