

namespace BL
{
    public interface Ij04UserRoleBL
    {
        public BO.j04UserRole Load(int pid);
        public IEnumerable<BO.j04UserRole> GetList(BO.myQueryJ04 mq);
        public int Save(BO.j04UserRole rec, List<int> perms, List<int> x54ids);
        public IEnumerable<BO.Permission> GetList_BoundX53(int j04id);
        public IEnumerable<BO.x54WidgetGroup> GetList_x54(int j04id);

    }
    class j04UserRoleBL : BaseBL, Ij04UserRoleBL
    {

        public j04UserRoleBL(BL.Factory mother) : base(mother)
        {

        }

        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*," + _db.GetSQL1_Ocas("j04") + ",x67x.x01ID,x67x.x67RoleValue FROM j04UserRole a INNER JOIN x67EntityRole x67x ON a.x67ID=x67x.x67ID");

            sb(strAppend);
            return sbret();
        }

        public BO.j04UserRole Load(int pid)
        {
            return _db.Load<BO.j04UserRole>(GetSQL1(" WHERE a.j04ID=@pid"), new { pid = pid });
        }
        public IEnumerable<BO.j04UserRole> GetList(BO.myQueryJ04 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.j04UserRole>(fq.FinalSql, fq.Parameters);
        }
        public IEnumerable<BO.Permission> GetList_BoundX53(int j04id)
        {
            var allperms = BO.Code.Entity.GetAllPermissions("j04");
            var ret = new List<BO.Permission>();

            string s = "SELECT a.x68PermValue as Value FROM x68EntityRole_Permission a INNER JOIN x67EntityRole b ON a.x67ID=b.x67ID INNER JOIN j04UserRole c ON b.x67ID=c.x67ID WHERE c.j04ID=@j04id";
            var saveperms = _db.GetList<BO.GetInteger>(s, new { j04id = j04id });
            foreach (var perm in saveperms)
            {
                if (allperms.Any(p => (int)p.Value == perm.Value))
                {
                    ret.Add(allperms.First(p => (int)p.Value == perm.Value));
                }
            }

            return ret;

        }
        public IEnumerable<BO.x54WidgetGroup> GetList_x54(int j04id)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("x54"));
            sb(" FROM x54WidgetGroup a");
            sb(" WHERE a.x54ID IN (SELECT x54ID FROM x59WidgetToUser WHERE j04ID=@j04id)");
            sb(" ORDER BY a.x54Ordinary");
            return _db.GetList<BO.x54WidgetGroup>(sbret(), new { j04id = j04id });
        }

        public int Save(BO.j04UserRole rec, List<int> perms, List<int> x54ids)
        {
            if (!ValidateBeforeSave(rec, perms))
            {
                return 0;
            }
            var recX67 = new BO.x67EntityRole() { x67Entity = "j04", x67Name = rec.j04Name, x01ID = rec.x01ID };
            if (rec.pid > 0)
            {
                recX67 = _mother.x67EntityRoleBL.Load(rec.x67ID);
            }
            rec.x67ID = _mother.x67EntityRoleBL.Save(recX67, perms);
            if (rec.x67ID == 0)
            {
                return 0;
            }

            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x67ID", rec.x67ID, true);
            p.AddString("j04Name", rec.j04Name);
            p.AddString("j04HomePageUrl", rec.j04HomePageUrl);
            p.AddString("j04HomePageUrl_Mobile", rec.j04HomePageUrl_Mobile);

            p.AddBool("j04IsAllowLoginByGoogle", rec.j04IsAllowLoginByGoogle);
            p.AddBool("j04IsModule_o23", rec.j04IsModule_o23);
            p.AddBool("j04IsModule_p41", rec.j04IsModule_p41);
            p.AddBool("j04IsModule_j02", rec.j04IsModule_j02);
            p.AddBool("j04IsModule_p31", rec.j04IsModule_p31);
            p.AddBool("j04IsModule_p91", rec.j04IsModule_p91);
            p.AddBool("j04IsModule_p90", rec.j04IsModule_p90);
            p.AddBool("j04IsModule_p28", rec.j04IsModule_p28);
            p.AddBool("j04IsModule_p56", rec.j04IsModule_p56);
            p.AddBool("j04IsModule_Widgets", rec.j04IsModule_Widgets);
            p.AddBool("j04IsModule_x31", rec.j04IsModule_x31);
            p.AddBool("j04IsModule_p11", rec.j04IsModule_p11);
            p.AddBool("j04IsModule_o43", rec.j04IsModule_o43);
            p.AddBool("j04IsModule_r01", rec.j04IsModule_r01);
            p.AddBool("j04IsModule_p49", rec.j04IsModule_p49);

            p.AddByte("j04FilesTab", rec.j04FilesTab);

            p.AddInt("a55ID_o23", rec.a55ID_o23, true);
            p.AddInt("a55ID_p28", rec.a55ID_p28, true);
            p.AddInt("a55ID_le5", rec.a55ID_le5, true);
            p.AddInt("a55ID_le4", rec.a55ID_le4, true);
            p.AddInt("a55ID_p91", rec.a55ID_p91, true);
            p.AddInt("a55ID_j02", rec.a55ID_j02, true);

            p.AddString("j04GridColumnsExclude", rec.j04GridColumnsExclude);

            int intPID = _db.SaveRecord("j04UserRole", p, rec);


            if (intPID > 0)    //vyčistit uživatelskou cache pro účty s vazbou na tuto roli
            {
                if (x54ids != null)
                {
                    if (rec.pid > 0)
                    {
                        _db.RunSql("DELETE FROM x59WidgetToUser WHERE j04ID=@pid", new { pid = intPID });
                    }
                    if (x54ids.Count() > 0)
                    {
                        _db.RunSql("INSERT INTO x59WidgetToUser(j04ID,x54ID) SELECT @pid,x54ID FROM x54WidgetGroup WHERE x54ID IN (" + string.Join(",", x54ids) + ")", new { pid = intPID });
                    }
                }

                _db.RunSql("UPDATE j02User set j02Cache_TimeStamp=null WHERE j04ID IN (select a.j04ID FROM j04UserRole a INNER JOIN x67EntityRole b ON a.x67ID=b.x67ID WHERE b.x67ID=@pid)", new { pid = intPID });

            }

            return intPID;
        }


        private bool ValidateBeforeSave(BO.j04UserRole rec, List<int> perms)
        {
            if (perms != null && perms.Count == 0)
            {
                this.AddMessage("K roli musí být přiřazeno minimálně jedno oprávnění!"); return false;
            }
            if (string.IsNullOrEmpty(rec.j04Name) == true)
            {
                this.AddMessage("Název aplikační role je povinné pole."); return false;
            }



            return true;
        }
    }
}
