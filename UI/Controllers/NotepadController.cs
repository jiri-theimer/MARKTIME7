using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace UI.Controllers
{
    public class NotepadController : BaseController
    {
        private readonly IWebHostEnvironment _env;
        public NotepadController(IWebHostEnvironment env)
        {
            _env = env;
        }


        [HttpPost("/Notepad/UploadImages")]
        [Produces("application/json")]
        public async Task<IActionResult> PostImage(List<IFormFile> files)
        {
            var theFile = HttpContext.Request.Form.Files.GetFile("file");       //název souboru obrázku je v default parameteru [file]
            string strWhat = HttpContext.Request.Form["what"];            //předává editor
            int intX04ID = BO.Code.Bas.InInt(HttpContext.Request.Form["x04id"]);            //předává editor
            string strPrefix = HttpContext.Request.Form["prefix"];            //předává editor
            string strTempGuid = HttpContext.Request.Form["tempguid"];            //předává editor
            if (string.IsNullOrEmpty(strPrefix)) strPrefix = "---";

            string strUploadRootAbsolute = Factory.NotepadFolder + "\\" + strPrefix + "\\" + GetFolderYYMM(false);
            string strUploadRootRelative = "/_users/" + Factory.Lic.x01Guid + "/NOTEPAD/" + strPrefix + "/" + GetFolderYYMM(true);


            if (!System.IO.Directory.Exists(strUploadRootAbsolute))
            {
                System.IO.Directory.CreateDirectory(strUploadRootAbsolute); //založit notepad složku na serveru
            }
            // Get the mime type
            
            // Get File Extension
            string extension = System.IO.Path.GetExtension(theFile.FileName);

            // Generate Random name.
            string strArchiveFileName = BO.Code.Bas.GetGuid().Substring(0, 20) + extension;


            // Build the full path including the file name
            string strFullPath = Path.Combine(strUploadRootAbsolute, strArchiveFileName);
            
            var valid = ValidateFile(theFile, strWhat, intX04ID);
            if (valid.Flag == BO.ResultEnum.Failed)
            {
                throw new ArgumentException("####" + valid.Message + "####");
            }

            try
            {
                // Copy contents to memory stream.
                Stream stream;
                stream = new MemoryStream();
                theFile.CopyTo(stream);
                stream.Position = 0;


                // Save the file
                using (FileStream writerFileStream = System.IO.File.Create(strFullPath))
                {
                    await stream.CopyToAsync(writerFileStream);
                    writerFileStream.Dispose();
                }

                string strFileGuid = SaveO27Rec(theFile, strTempGuid, strPrefix, strArchiveFileName, null, "NOTEPAD\\" + strPrefix + "\\" + GetFolderYYMM(false));


                // Return the file path as json
                Hashtable fileUrl = new Hashtable();
                fileUrl.Add("link", strUploadRootRelative + "/" + strArchiveFileName);
                //fileUrl.Add("link", "/FileUpload/FileDownloadTempFile?tempfilename=" + strFileName);
                return Json(fileUrl);


            }

            catch (ArgumentException ex)
            {
                return Json(ex.Message);
            }



        }



        [HttpPost("/Notepad/UploadFiles")]
        [Produces("application/json")]
        public async Task<IActionResult> PostFile(List<IFormFile> files)
        {

            var theFile = HttpContext.Request.Form.Files.GetFile("file");       //název souboru je v default parameteru [file]

            string strWhat = HttpContext.Request.Form["what"];            //předává editor
            int intX04ID = BO.Code.Bas.InInt(HttpContext.Request.Form["x04id"]);            //předává editor
            string strPrefix = HttpContext.Request.Form["prefix"];            //předává editor
            string strTempGuid = HttpContext.Request.Form["tempguid"];            //předává editor

            //System.IO.File.WriteAllText("c:\\temp\\hovado2.txt", "what: " + strWhat + ", x04id: " + strX04ID+", tempguid: "+ strTempGuid);
            if (string.IsNullOrEmpty(strPrefix)) strPrefix = "---";


            string strUploadRootAbsolute = Factory.UploadFolder + "\\NOTEPAD\\" + strPrefix + "\\" + GetFolderYYMM(false);


            if (!System.IO.Directory.Exists(strUploadRootAbsolute))
            {
                System.IO.Directory.CreateDirectory(strUploadRootAbsolute); //založit notepad složku na serveru
            }

            // Get File Extension
            string extension = System.IO.Path.GetExtension(theFile.FileName);

            // Generate Random name.
            string strArchiveFileName = BO.Code.Bas.GetGuid().Substring(0, 20) + extension;




            // Build the full path including the file name
            string strFullPath = Path.Combine(strUploadRootAbsolute, strArchiveFileName);


            var valid = ValidateFile(theFile, strWhat, intX04ID);
            if (valid.Flag == BO.ResultEnum.Failed)
            {
                throw new ArgumentException("####"+valid.Message+"####");
            }

            // Copy contents to memory stream.
            Stream stream;
            stream = new MemoryStream();
            theFile.CopyTo(stream);
            stream.Position = 0;


            // Save the file
            using (FileStream writerFileStream = System.IO.File.Create(strFullPath))
            {
                await stream.CopyToAsync(writerFileStream);
                writerFileStream.Dispose();
            }

            string strFileGuid = SaveO27Rec(theFile, strTempGuid, strPrefix, strArchiveFileName, "NOTEPAD\\" + strPrefix + "\\" + GetFolderYYMM(false), null);


            // Return the file path as json
            Hashtable fileUrl = new Hashtable();
            fileUrl.Add("link", "/FileUpload/FileDownloadInline?guid=" + strFileGuid);
            return Json(fileUrl);



        }

        

        private string SaveO27Rec(IFormFile theFile, string strTempGuid, string strPrefix, string strArchiveFileName, string strArchiveFolder, string strWwwRootFolder)  //vrací o27Guid
        {
            var rec = new BO.o27Attachment() { o27NotepadTempGuid = strTempGuid, o27Entity = strPrefix, o27ContentType = theFile.ContentType, o27FileSize = Convert.ToInt32(theFile.Length) };
            rec.o27FileExtension = System.IO.Path.GetExtension(theFile.FileName);
            rec.o27OriginalFileName = theFile.FileName;
            rec.o27ArchiveFileName = strArchiveFileName;
            rec.o27ArchiveFolder = strArchiveFolder;
            rec.o27WwwRootFolder = strWwwRootFolder;
            rec.o27CallerFlag = BO.o27CallerFlagENUM.Notepad;
            rec.o27Guid = Guid.NewGuid();
            if (Factory.o27AttachmentBL.Save(rec) > 0)
            {
                return rec.o27Guid.ToString();
            }

            return null;

        }

        private BO.Result ValidateFile(IFormFile theFile, string strWhat, int intX04ID)
        {            
            if (intX04ID == 0) return new BO.Result(false);
            var recX04 = Factory.x04NotepadConfigBL.Load(intX04ID);

            if (strWhat == "image" && recX04.x04ImageMaxSize > 0 && theFile.Length > recX04.x04ImageMaxSize)
            {
                return new BO.Result(true, $"Velikost obrázku ({BO.Code.Bas.FormatFileSize(Convert.ToInt32(theFile.Length))}) je větší než povolená velikost ({BO.Code.Bas.FormatFileSize(recX04.x04ImageMaxSize)}).");
            }
            if (strWhat == "file" && recX04.x04FileMaxSize > 0 && theFile.Length > recX04.x04FileMaxSize)
            {
                return new BO.Result(true, $"Velikost souboru ({BO.Code.Bas.FormatFileSize(Convert.ToInt32(theFile.Length))}) je větší než povolená velikost ({BO.Code.Bas.FormatFileSize(recX04.x04ImageMaxSize)}).");
            }

            string ext = System.IO.Path.GetExtension(theFile.FileName);
            if (strWhat == "file" && !string.IsNullOrEmpty(recX04.x04FileAllowedTypes+","+ recX04.x04ImageAllowedTypes))
            {
                var lis = BO.Code.Bas.ConvertString2List((recX04.x04FileAllowedTypes+ ","+recX04.x04ImageAllowedTypes).Replace(";", ","));
                if (!lis.Any(p => "." + p.ToUpper() == ext.ToUpper()))
                {
                    return new BO.Result(true, $"Nepovolená přípona souboru ({ext}). Povolené přípony/formáty: {recX04.x04FileAllowedTypes+","+recX04.x04ImageAllowedTypes}");
                }
            }
            if (strWhat == "image" && !string.IsNullOrEmpty(recX04.x04ImageAllowedTypes))
            {
                var lis = BO.Code.Bas.ConvertString2List((recX04.x04ImageAllowedTypes).Replace(";", ","));
                if (!lis.Any(p => "." + p.ToUpper() == ext.ToUpper()))
                {
                    return new BO.Result(true, $"Nepovolená přípona obrázku ({ext}). Povolené přípony/formáty: {recX04.x04FileAllowedTypes + "," + recX04.x04ImageAllowedTypes}");
                }
            }


            return new BO.Result(false);
        }

        private string GetFolderYYMM(bool bolRelative)
        {
            if (bolRelative)
            {
                return DateTime.Now.Year.ToString() + "/" + BO.Code.Bas.RightString("0" + DateTime.Now.Month.ToString(), 2);
            }
            else
            {
                return DateTime.Now.Year.ToString() + "\\" + BO.Code.Bas.RightString("0" + DateTime.Now.Month.ToString(), 2);
            }

        }
    }
}
