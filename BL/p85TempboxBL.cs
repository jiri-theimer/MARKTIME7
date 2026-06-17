

namespace BL
{
    public interface Ip85TempboxBL
    {
        public BO.p85Tempbox Load(int pid);
        public BO.p85Tempbox LoadByGuid(string strGUID);
        public IEnumerable<BO.p85Tempbox> GetList(string strGUID, bool bolIncludeDeleted = false, string strPrefix = null);
        public int Save(BO.p85Tempbox rec);
        public BO.p85Tempbox VirtualDelete(int intPID);
        public bool VirtualDeleteByGuid(string guid);
        public bool PermanentDeleteByGuid(string guid);
        public List<int> GetPidsFromTemp(string guid,string prefix=null);
        public int SetRemember(string s, int pid);
        public BO.p85Tempbox LoadRemember(int pid);
    }
    class p85TempboxBL : BaseBL, Ip85TempboxBL
    {
        public p85TempboxBL(BL.Factory mother) : base(mother)
        {

        }

        private string GetSQL1(string strAppend = null)
        {            
            sb("SELECT a.*," + _db.GetSQL1_Ocas("p85") + " FROM p85Tempbox a");
            sb(strAppend);
            return sbret();
        }

        public BO.p85Tempbox Load(int pid)
        {
            return _db.Load<BO.p85Tempbox>(GetSQL1(" WHERE a.p85ID=@pid"), new { pid = pid });
        }
        public BO.p85Tempbox LoadByGuid(string strGUID)
        {
            return _db.Load<BO.p85Tempbox>(GetSQL1(" WHERE a.p85GUID = @guid"), new { guid = strGUID });
        }
        public IEnumerable<BO.p85Tempbox> GetList(string strGUID, bool bolIncludeDeleted = false, string strPrefix = null)
        {
            var p = new Dapper.DynamicParameters();
            p.Add("guid", strGUID);
            sb(GetSQL1(" WHERE a.p85GUID=@guid"));
            if (bolIncludeDeleted == false)
            {
                sb(" AND a.p85IsDeleted=0");                
            }
            if (strPrefix != null)
            {
                sb(" AND a.p85Prefix=@prefix");
                p.Add("prefix", strPrefix);
            }
            return _db.GetList<BO.p85Tempbox>(sbret(), p);
        }

        public BO.p85Tempbox VirtualDelete(int intPID)
        {
            sb("UPDATE p85Tempbox set p85Isdeleted=1 WHERE p85ID=@pid;");
            sb(GetSQL1(" WHERE a.p85ID=@pid"));
            return _db.Load<BO.p85Tempbox>(sbret(), new { pid = intPID });
        }

        public bool VirtualDeleteByGuid(string guid)
        {
           
            return _db.RunSql("UPDATE p85Tempbox set p85Isdeleted=1 WHERE p85Guid=@guid", new { guid = guid });
        }
        public bool PermanentDeleteByGuid(string guid)
        {

            return _db.RunSql("DELETE FROM p85Tempbox WHERE p85Guid=@guid", new { guid = guid });
        }
        public int SetRemember(string s,int pid)
        {
            var c = new BO.p85Tempbox() {p85Prefix="rem", p85GUID=BO.Code.Bas.GetGuid(), p85DataPID = pid, p85FreeText01 = s,p85OtherKey1=_mother.CurrentUser.pid,p85FreeDate01=DateTime.Now };
            return Save(c);
        }
        public BO.p85Tempbox LoadRemember(int pid)
        {
            var c= _db.Load<BO.p85Tempbox>(GetSQL1(" WHERE a.p85Prefix='rem' AND a.p85DataPID=@pid AND p85OtherKey1=@j02id"), new { pid = pid,j02id=_mother.CurrentUser.pid });
            if (c == null)
            {
                return null;
            }
            if (c.p85FreeDate01.Value.AddHours(24)< DateTime.Now)
            {
                return null;
            }

            return c;
        }

        public int Save(BO.p85Tempbox rec)
        {
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.p85ID);
            p.AddString("p85GUID", rec.p85GUID);
            p.AddString("p85Prefix", rec.p85Prefix);
            p.AddInt("p85DataPID", rec.p85DataPID, true);
            p.AddInt("p85ClonePid", rec.p85ClonePid);
            p.AddBool("p85IsDeleted", rec.p85IsDeleted);
            p.AddBool("p85IsFinished", rec.p85IsFinished);

            p.AddInt("p85OtherKey1", rec.p85OtherKey1, true);
            p.AddInt("p85OtherKey2", rec.p85OtherKey2, true);
            p.AddInt("p85OtherKey3", rec.p85OtherKey3, true);
            p.AddInt("p85OtherKey4", rec.p85OtherKey4, true);
            p.AddInt("p85OtherKey5", rec.p85OtherKey5, true);

            p.AddString("p85Message", rec.p85Message);

            p.AddString("p85FreeText01", rec.p85FreeText01);
            p.AddString("p85FreeText02", rec.p85FreeText02);
            p.AddString("p85FreeText03", rec.p85FreeText03);
            p.AddString("p85FreeText04", rec.p85FreeText04);
            p.AddString("p85FreeText05", rec.p85FreeText05);
            p.AddString("p85FreeText06", rec.p85FreeText06);
            p.AddString("p85FreeText07", rec.p85FreeText07);
            p.AddString("p85FreeText08", rec.p85FreeText08);
            p.AddString("p85FreeText09", rec.p85FreeText09);


            p.AddBool("p85FreeBoolean01", rec.p85FreeBoolean01);
            p.AddBool("p85FreeBoolean02", rec.p85FreeBoolean02);
            p.AddBool("p85FreeBoolean03", rec.p85FreeBoolean03);
            p.AddBool("p85FreeBoolean04", rec.p85FreeBoolean04);

            p.AddDouble("p85FreeNumber01", rec.p85FreeNumber01);
            p.AddDouble("p85FreeNumber02", rec.p85FreeNumber02);
            p.AddDouble("p85FreeNumber03", rec.p85FreeNumber03);
            p.AddDouble("p85FreeNumber04", rec.p85FreeNumber04);
            p.AddDouble("p85FreeFloat01", rec.p85FreeFloat01);
            p.AddDouble("p85FreeFloat02", rec.p85FreeFloat02);
            p.AddDouble("p85FreeFloat03", rec.p85FreeFloat03);

            p.AddDateTime("p85FreeDate01", rec.p85FreeDate01);
            p.AddDateTime("p85FreeDate02", rec.p85FreeDate02);
            p.AddDateTime("p85FreeDate03", rec.p85FreeDate03);
            p.AddDateTime("p85FreeDate04", rec.p85FreeDate04);

            return _db.SaveRecord("p85Tempbox", p, rec);
        }

        public List<int> GetPidsFromTemp(string guid, string prefix = null)
        {
            var lisTemp = GetList(guid, true, prefix);
            return lisTemp.Select(p => p.p85DataPID).ToList();
        }
    }
}
