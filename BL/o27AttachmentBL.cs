
namespace BL
{
    public interface Io27AttachmentBL
    {
        public BO.o27Attachment Load(int pid);
        public BO.o27Attachment LoadByGuid(string guid);
        
        public IEnumerable<BO.o27Attachment> GetList(BO.myQueryO27 mq);
        
        public BO.o27Attachment InhaleFileByInfox(string strInfoxFullPath);
        public List<BO.o27Attachment> GetTempFiles( string strTempGUID);
        public int Save(BO.o27Attachment rec);
        public bool SaveChangesAndUpload(string guid, string entity, int recpid);
        public bool SaveSingleUpload(string guid, string entity, int recpid);
        public int UploadAndSaveOneFile(BO.o27Attachment rec, string strOrigFileName, string strSourceFullPath, string strExplicitArchiveFileName = null);
        public List<BO.o27Attachment> CopyTempFiles2Upload(string strTempGUID);
        public string GetUploadFolder(string entity);
        public bool CopyOneTempFile2Upload(string strTempFileName, string strDestFolderName, string strDestFileName);
        public void Move2Deleted(BO.o27Attachment rec);
        public string CreateTempInfoxFile(string strGuid, string strEntity, string strTempFileName, string strOrigFileName, string strContentType);
        public bool CommitNotepdChanges(string strTempNotepadGuid,string strPrefix, int intRecordPid);

    }
    class o27AttachmentBL : BaseBL, Io27AttachmentBL
    {
        public o27AttachmentBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");            
            sb(_db.GetSQL1_Ocas("o27"));
            sb(" FROM o27Attachment a");
            sb(strAppend);
            return sbret();
        }
        public BO.o27Attachment Load(int pid)
        {
            return _db.Load<BO.o27Attachment>(GetSQL1(" WHERE a.o27ID=@pid"), new { pid = pid });
        }
        public BO.o27Attachment LoadByGuid(string guid)
        {
            return _db.Load<BO.o27Attachment>(GetSQL1(" WHERE a.o27Guid=@guid"), new { guid = guid });
        }


        public IEnumerable<BO.o27Attachment> GetList(BO.myQueryO27 mq)
        {
            if (mq.explicit_orderby == null) { mq.explicit_orderby = "a.o27ID DESC"; };
            //if (mq.tempguid != null)
            //{
            //    mq.explicit_orderby = null;
            //}
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            
            return _db.GetList<BO.o27Attachment>(fq.FinalSql, fq.Parameters);
        }
        


        public int Save(BO.o27Attachment rec)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("o27Entity", rec.o27Entity);
            p.AddInt("o27RecordPid", rec.o27RecordPid, true);
           
            p.AddString("o27Name", rec.o27Name);
           
            p.AddString("o27OriginalFileName", rec.o27OriginalFileName);
            p.AddString("o27FileExtension", rec.o27FileExtension);
            p.AddString("o27ArchiveFileName", rec.o27ArchiveFileName);
            p.AddString("o27ArchiveFolder", rec.o27ArchiveFolder);
            p.AddString("o27WwwRootFolder", rec.o27WwwRootFolder);
            if (rec.o27FileExtension == null && rec.o27ArchiveFileName != null && rec.o27ArchiveFileName.Contains("."))
            {
                var arr = rec.o27ArchiveFileName.Split(".");
                rec.o27FileExtension = arr[arr.Length-1];
            }
            
            p.AddInt("o27FileSize", rec.o27FileSize);
            p.AddString("o27ContentType", rec.o27ContentType);
            p.AddString("o27FullText", rec.o27FullText);
            if (rec.o27Guid == Guid.Empty)
            {
                rec.o27Guid = Guid.NewGuid();
            }
            p.AddString("o27Guid", rec.o27Guid.ToString());
            p.AddString("o27NotepadTempGuid", rec.o27NotepadTempGuid);
            p.AddEnumInt("o27CallerFlag", rec.o27CallerFlag);
          

            int intPID = _db.SaveRecord("o27Attachment", p, rec);



            return intPID;
        }

        public int UploadAndSaveOneFile(BO.o27Attachment rec,string strOrigFileName,string strSourceFullPath,string strExplicitArchiveFileName = null)
        {
            
            if (string.IsNullOrEmpty(strOrigFileName))
            {
                rec.o27OriginalFileName = BO.Code.File.GetFileInfo(strSourceFullPath).Name;
            }
            else
            {
                rec.o27OriginalFileName = strOrigFileName;
            }            
            rec.o27FileExtension = BO.Code.File.GetFileInfo(strSourceFullPath).Extension;
            rec.o27FileSize = Convert.ToInt32(BO.Code.File.GetFileInfo(strSourceFullPath).Length);            
            
            if (string.IsNullOrEmpty(strExplicitArchiveFileName) == false)
            {
                rec.o27ArchiveFileName = strExplicitArchiveFileName;
            }
            if (string.IsNullOrEmpty(rec.o27ArchiveFileName))
            {
                rec.o27ArchiveFileName = rec.o27OriginalFileName ;
            }
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            rec.o27ArchiveFolder = GetUploadFolder(rec.o27Entity);
            if (!System.IO.Directory.Exists(_mother.UploadFolder + "\\" + rec.o27ArchiveFolder))
            {
                System.IO.Directory.CreateDirectory(_mother.UploadFolder + "\\" + rec.o27ArchiveFolder);
            }
            var intO27ID = Save(rec);
            if (intO27ID > 0)
            {
                System.IO.File.Copy(strSourceFullPath, _mother.UploadFolder + "\\" + rec.o27ArchiveFolder+"\\"+rec.o27ArchiveFileName, true);
            }

            return intO27ID;

        }

        public bool SaveSingleUpload(string guid, string entity, int recpid)
        {
            using (var sc = new System.Transactions.TransactionScope()) //podléhá jedné transakci
            {
                var recs4upload = new List<BO.o27Attachment>();

                var mq = new BO.myQueryO27() { entity = entity, recpid = recpid };
                
                var lisO27Saved = GetList(mq);
                              
                if (GetTempFiles(guid).Count > 0)
                {
                    foreach (var recO27 in GetTempFiles(guid))
                    {

                        recO27.o27RecordPid = recpid;
                        recO27.o27Entity = entity;
                        recO27.o27ArchiveFolder = GetUploadFolder(recO27.o27Entity);
                        recO27.o27ArchiveFileName = recO27.o27OriginalFileName;

                        if (lisO27Saved.Count() > 0)
                        {
                            recO27.pid = lisO27Saved.First().pid;   //natvrdo přepsat stávající záznam dokumentu                            
                        }
                        var intO27ID = Save(recO27);
                        if (intO27ID > 0)
                        {
                            recs4upload.Add(recO27);
                            
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                
                sc.Complete();

                foreach (var rec in recs4upload)
                {
                    //soubor na serveru se ukládá pod jeho originálním fyzickým názvem
                    CopyOneTempFile2Upload(guid+"_"+rec.o27OriginalFileName, rec.o27ArchiveFolder, rec.o27OriginalFileName);
                }
            }


            return true;
        }


        public bool SaveChangesAndUpload(string guid,string entity,int recpid)
        {
            var recs4upload = new List<BO.o27Attachment>();
            var lisTempO27 = GetTempFiles(guid);
            if (lisTempO27.Count > 0)
            {
                foreach (var recO27 in lisTempO27)
                {
                    recO27.o27Entity = entity;
                    recO27.o27RecordPid = recpid;
                    recO27.o27ArchiveFolder = GetUploadFolder(entity);
                    recO27.o27Guid = Guid.NewGuid();
                    var intO27ID = Save(recO27);
                    if (intO27ID > 0)
                    {
                        recs4upload.Add(recO27);
                        switch (entity)
                        {
                            case "o23":
                                _mother.FBL.RunSql("UPDATE o23Doc SET o27ID_Last=@o27id WHERE o23ID=@pid", new { o27id = intO27ID, pid = recpid });
                                break;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            var tempList = _mother.p85TempboxBL.GetList(guid);
            foreach (var recTemp in tempList)  //test na odstraněné záznamy příloh
            {
                if (_mother.CBL.DeleteRecord("o27Attachment", recTemp.p85DataPID) != "1")
                {
                    this.AddMessageTranslated("Error: DELETE.");
                    return false;
                }
                else
                {
                    switch (entity)
                    {
                        case "o23":
                            _mother.FBL.RunSql("if exists(select o23ID FROM o23Doc WHERE o27ID_Last=@pid) UPDATE o23Doc SET o27ID_Last=NULL WHERE o27ID_Last=@pid", new { pid = recTemp.p85DataPID });
                            break;
                    }
                }
            }
           
            foreach (var rec in recs4upload)
            {
                CopyOneTempFile2Upload(rec.o27ArchiveFileName, rec.o27ArchiveFolder, rec.o27ArchiveFileName);
            }


            return true;
        }

        public bool ValidateBeforeSave(BO.o27Attachment rec)
        {
            
            if (string.IsNullOrEmpty(rec.o27ArchiveFileName))
            {
                this.AddMessage("Chybí vyplnit [o27ArchiveFileName]."); return false;
            }
            if (string.IsNullOrEmpty(rec.o27OriginalFileName))
            {
                this.AddMessage("Chybí vyplnit [o27OriginalFileName]."); return false;
            }


            return true;
        }


        public BO.o27Attachment InhaleFileByInfox(string strInfoxFullPath)
        {
            //příklad infox: image/png|99330|2019-11-13_104250.png|8ab14290cd8d4a929d518d2f9e663ecd_2019-11-13_104250.png|8ab14290cd8d4a929d518d2f9e663ecd|o23|Poznámka
            if (!System.IO.File.Exists(strInfoxFullPath))
            {
                return null;
            }

            string s = System.IO.File.ReadAllText(strInfoxFullPath);
            List<string> arr = BO.Code.Bas.ConvertString2List(s, "|");
            var rec = new BO.o27Attachment()
            {
                o27ContentType = arr[0],
                o27FileSize = BO.Code.Bas.InInt(arr[1]),
                o27OriginalFileName = arr[2],
                o27ArchiveFileName = arr[3],
                o27Guid = new Guid(arr[4]),
                o27Entity = arr[5],
                o27Name = arr[6]
            };
            
            if (rec.o27OriginalFileName.Contains("."))
            {
                arr = BO.Code.Bas.ConvertString2List(rec.o27OriginalFileName, ".");
                rec.o27FileExtension = "."+arr[arr.Count - 1];
            }
            else
            {
                rec.o27FileExtension = ".";
            }
            
            rec.FullPath = _mother.TempFolder + "\\" + rec.o27ArchiveFileName;
            if (rec.o27FileSize == 0 && System.IO.File.Exists(rec.FullPath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(rec.FullPath);
                rec.o27FileSize = fileBytes.Length;
            }
            return rec;

        }

        public List<BO.o27Attachment> GetTempFiles(string strTempGUID)
        {
            var lisO27 = new List<BO.o27Attachment>();
            foreach (string file in System.IO.Directory.EnumerateFiles(_mother.TempFolder, strTempGUID + "*.infox", System.IO.SearchOption.TopDirectoryOnly))
            {
                var rec = InhaleFileByInfox(file);
                if (System.IO.File.Exists(_mother.TempFolder + "\\" + rec.o27ArchiveFileName))
                {
                    rec.FullPath = _mother.TempFolder + "\\" + rec.o27ArchiveFileName;
                    lisO27.Add(rec);
                }

            }

            return lisO27;
        }

        public void Move2Deleted(BO.o27Attachment rec)
        {
            string strDir = _mother.UploadFolder + "\\DELETED\\" + DateTime.Now.Year.ToString() + "\\" + BO.Code.Bas.RightString("0" + DateTime.Now.Month.ToString(), 2);

            if (!System.IO.Directory.Exists(strDir))
            {
                System.IO.Directory.CreateDirectory(strDir);
            }
            if (System.IO.File.Exists(_mother.UploadFolder + "\\" + rec.o27ArchiveFolder + "\\" + rec.o27ArchiveFileName))
            {
                System.IO.File.Move(_mother.UploadFolder + "\\" + rec.o27ArchiveFolder + "\\" + rec.o27ArchiveFileName, strDir+"\\"+ rec.o27ArchiveFileName, true);

            }


        }

        public bool CopyOneTempFile2Upload(string strTempFileName,string strDestFolderName,string strDestFileName)
        {
            if (!System.IO.Directory.Exists(_mother.UploadFolder + "\\" + strDestFolderName))
            {
                System.IO.Directory.CreateDirectory(_mother.UploadFolder + "\\" + strDestFolderName);
            }
            string strDestFullPath = _mother.UploadFolder;
            if (!string.IsNullOrEmpty(strDestFolderName))
            {
                strDestFullPath += "\\" + strDestFolderName;
            }
            strDestFullPath+="\\"+strDestFileName;

            System.IO.File.Copy(_mother.TempFolder + "\\" + strTempFileName, strDestFullPath, true);

            return true;
        }
        public List<BO.o27Attachment> CopyTempFiles2Upload(string strTempGUID)
        {
            var lisO27 = new List<BO.o27Attachment>();
            foreach (string file in System.IO.Directory.EnumerateFiles(_mother.TempFolder, strTempGUID + "*.infox", System.IO.SearchOption.TopDirectoryOnly))
            {
                BO.o27Attachment rec = InhaleFileByInfox(file);
                rec.o27ArchiveFolder = GetUploadFolder(rec.o27Entity);

                if (!System.IO.Directory.Exists(_mother.UploadFolder + "\\" + rec.o27ArchiveFolder))
                {
                    System.IO.Directory.CreateDirectory(_mother.UploadFolder + "\\" + rec.o27ArchiveFolder);
                }
                rec.FullPath = _mother.UploadFolder + "\\" + rec.o27ArchiveFolder + "\\" + rec.o27ArchiveFileName;
                System.IO.File.Copy(_mother.TempFolder + "\\" + rec.o27ArchiveFileName, rec.FullPath, true);

                lisO27.Add(rec);
            }
            return lisO27;
        }


        public string GetUploadFolder(string entity)
        {
            if (entity !=null && entity.ToLower() == "x31")
            {
                return entity;  //tiskové sestavy jsou všechny na spolu v jedné složce
            }
            string s = DateTime.Now.Year.ToString() + "\\" + BO.Code.Bas.RightString("0" + DateTime.Now.Month.ToString(), 2);

            if (string.IsNullOrEmpty(entity))
            {
                return s;
            }
            else
            {
                return entity + "\\" + s;
            }

          
        }

       
        public string CreateTempInfoxFile(string strGuid,string strEntity,string strTempFileName,string strOrigFileName,string strContentType)
        {
            
            string strInfoxFileName = strGuid + "_"+strOrigFileName+".infox";
            int intFileSize= Convert.ToInt32(BO.Code.File.GetFileInfo(_mother.TempFolder+"\\"+strTempFileName).Length);

            System.IO.File.AppendAllText(_mother.TempFolder + "\\" + strInfoxFileName, strContentType +"| " + intFileSize.ToString() + "|"+strOrigFileName+"|" + strTempFileName+"|" + strGuid + "|" + strEntity + "|");

            return _mother.TempFolder + "\\" + strInfoxFileName;
        }

        public bool CommitNotepdChanges(string strTempNotepadGuid,string strPrefix,int intRecordPid)
        {
            return _db.RunSql("UPDATE o27Attachment set o27RecordPid=@recpid,o27NotepadTempGuid=null WHERE o27NotepadTempGuid=@guid", new { recpid = intRecordPid, guid = strTempNotepadGuid });



        }

    }
}
