using BO;
using System;
using System.Collections.Generic;

namespace BL
{
    public interface Ix67EntityRoleBL
    {
        public BO.x67EntityRole Load(int pid);
        public IEnumerable<BO.x67EntityRole> GetList(BO.myQuery mq);
        public int Save(BO.x67EntityRole rec, List<int> perms);
        public IEnumerable<BO.Permission> GetList_BoundPerms(int x67id);
        

        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69(string record_entity, int record_pid);        
        //public IEnumerable<BO.x67EntityRole> GetList_BoundSlaves(int x67id);
        public bool IamReceiverOfList(IEnumerable<BO.x69EntityRole_Assign> lis);
        public void SaveO28(int x67id, List<BO.o28ProjectRole_Workload> lisO28);
        public IEnumerable<BO.o28ProjectRole_Workload> GetListO28(int x67id);
        public IEnumerable<BO.RoleAssignedToRecord> GetRolesAssignedToRecord(int record_pid, string record_prefix);

        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneDoc(BO.o23Doc rec, bool bolOnly4CurUser);
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneProject(BO.p41Project rec,bool bolOnly4CurUser);
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneContact(BO.p28Contact rec, bool bolOnly4CurUser);
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneInvoice(BO.p91Invoice rec, bool bolOnly4CurUser);
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneProforma(BO.p90Proforma rec, bool bolOnly4CurUser);
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneTask(BO.p56Task rec, bool bolOnly4CurUser);
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneMilestone(BO.o22Milestone rec, bool bolOnly4CurUser);
        public void Recovery_x68(BO.x67EntityRole rec);
        public void UpdateOneRoleAssign(int recpid, string recprefix, int x67id, List<int> j02ids, List<int> j11ids, bool bolIsAll);    //využití v Hromadné úpravy
        public void ClearOneRoleAssign(int recpid, string recprefix, int x67id);    //využití v Hromadné úpravy
        public bool Validate_lisX69_BeforeAssign(List<BO.x69EntityRole_Assign> lisX69);
        public IEnumerable<BO.x67EntityRole> GetList_One_Invoice(int p91id, int j02id,string j11ids);
    }
    class x67EntityRoleBL:BaseBL, Ix67EntityRoleBL
    {
        public x67EntityRoleBL(BL.Factory mother) : base(mother)
        {
            
        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("x67"));
            sb(" FROM x67EntityRole a");
            sb(strAppend);
            return sbret();
        }
        public BO.x67EntityRole Load(int pid)
        {
            return _db.Load<BO.x67EntityRole>(GetSQL1(" WHERE a.x67ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.x67EntityRole> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.x67Ordinary";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x67EntityRole>(fq.FinalSql, fq.Parameters);
        }

        public IEnumerable<BO.x67EntityRole> GetList_One_Invoice(int p91id, int j02id,string j11ids)
        {
            string s = GetSQL1(" INNER JOIN x73InvoiceRole_Permission x73 ON x73.x67ID=a.x67ID WHERE x73.p91ID=@p91id AND (x73.j02ID=@j02id OR x73.x73IsAllUsers=1)");
            if (j11ids != null)
            {
                s = GetSQL1($" INNER JOIN x73InvoiceRole_Permission x73 ON x73.x67ID=a.x67ID WHERE x73.p91ID=@p91id AND (x73.j02ID=@j02id OR x73.j11ID IN ({j11ids}) OR x73.x73IsAllUsers=1)");
            }
            return _db.GetList<BO.x67EntityRole>(s, new { p91id = p91id, j02id = j02id });

            
        }

        public bool Validate_lisX69_BeforeAssign(List<BO.x69EntityRole_Assign> lisX69)
        {
            if (lisX69 != null && lisX69.Count() > 0)
            {
                foreach (var c in lisX69)
                {
                    if (c.x67IsRequired && (c.j02ID == 0 && c.j11ID == 0 && !c.x69IsAllUsers))
                    {
                        this.AddMessageTranslated($"Roli [{c.x67Name}] je povinné obsadit.");
                        return false;
                    }
                }
            }

            return true;
        }

        public int Save(BO.x67EntityRole rec, List<int> perms)
        {
            if (!ValidateBeforeSave(rec, perms))
            {
                return 0;
            }
            var strRoleValue = new String('0', 50);
            if (perms != null && perms.Count > 0)
            {
                foreach (var perm in perms.Where(p=>p>0))
                {
                    strRoleValue = strRoleValue.Substring(0, perm - 1) + "1" + strRoleValue.Substring(perm, strRoleValue.Length - perm);
                }

                
            }
            

            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
                p.AddInt("a55ID", rec.a55ID, true);
                p.AddString("x67Entity", rec.x67Entity);
                p.AddString("x67Name", rec.x67Name);
                p.AddInt("x67Ordinary", rec.x67Ordinary);
                p.AddString("x67RoleValue", strRoleValue);
                p.AddBool("x67IsRequired", rec.x67IsRequired);

                int intPID = _db.SaveRecord("x67EntityRole", p, rec);
                if (intPID > 0)
                {
                    if (rec.pid > 0)
                    {
                        _db.RunSql("DELETE FROM x68EntityRole_Permission WHERE x67ID=@pid", new { pid = intPID });
                    }
                    if (perms != null && perms.Count > 0)
                    {
                        foreach(var perm in perms.Where(p=>p>0))
                        {
                            _db.RunSql("INSERT INTO x68EntityRole_Permission(x67ID,x68PermValue) VALUES (@pid,@oneperm)", new { pid = intPID,oneperm=perm });
                        }
                        
                    }
                    

                   

                    sc.Complete();
                    //_db.RunSql("exec dbo.x73_recovery_all");

                    return intPID;
                }
            }


            return 0;
            
        }
        public bool ValidateBeforeSave(BO.x67EntityRole rec, List<int> perms)
        {
            
            if (string.IsNullOrEmpty(rec.x67Name))
            {
                this.AddMessage("Chybí vyplnit [Název role]."); return false;
            }
            //if (perms==null || !perms.Any(p => p > 0))
            //{
            //    this.AddMessage("V nastavení role musí být zaškrtnuto minimálně jedno oprávnění.");return false;
            //}
            return true;
        }

        public IEnumerable<BO.Permission> GetList_BoundPerms(int x67id)
        {
            var recX67 = Load(x67id);
            var allperms = BO.Code.Entity.GetAllPermissions(recX67.x67Entity);
            
            var lis = _db.GetList<BO.GetInteger>("SELECT x68PermValue as Value FROM x68EntityRole_Permission WHERE x67ID=@x67id", new { x67id = x67id });
            var ret = new List<BO.Permission>();
            foreach(var oneperm in lis)
            {
                try
                {
                    ret.Add(allperms.First(p => (int)p.Value == oneperm.Value));
                }catch
                {
                    //oprávnění neexistuje
                }
                
            }

            return ret;
        }

           

        private void Init_SQL_GetList_X69()
        {
            sb("SELECT a.*,j02.j02Name as Person,x67.x67Name,j11.j11Name,x67.x67Entity,x67.x67RoleValue,x67.x67IsRequired");
            sb(" FROM x69EntityRole_Assign a INNER JOIN x67EntityRole x67 ON a.x67ID=x67.x67ID");
            sb(" LEFT OUTER JOIN j02User j02 ON a.j02ID=j02.j02ID LEFT OUTER JOIN j11Team j11 ON a.j11ID=j11.j11ID");
        }
        private void SQL_OcasQueryByUser()
        {
            if (_mother.CurrentUser.j11IDs != null)
            {
                sb($" AND (a.j02ID={_mother.CurrentUser.pid} OR a.x69IsAllUsers=1 OR a.j11ID IN ({_mother.CurrentUser.j11IDs}))");
            }
            else
            {
                sb($" AND (a.j02ID={_mother.CurrentUser.pid} OR a.x69IsAllUsers=1)");
            }
        }

        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69(string record_entity, int record_pid)        //metodu používá shared view: _RoleAssign
        {
            Init_SQL_GetList_X69();
            sb(" WHERE a.x69RecordPid=@recpid AND a.x69RecordEntity=@entity");
            sb(" ORDER BY x67.x67Ordinary,a.x67ID");

            return _db.GetList<BO.x69EntityRole_Assign>(sbret(), new { recpid = record_pid, entity = record_entity });
        }

        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneDoc(BO.o23Doc rec, bool bolOnly4CurUser)
        {
            Init_SQL_GetList_X69();
            sb(" WHERE (a.x69RecordPid=@o23id AND a.x69RecordEntity='o23') OR (a.x69RecordPid=@o18id AND a.x69RecordEntity='o18')");
            if (bolOnly4CurUser)
            {
                SQL_OcasQueryByUser();
            }
            return _db.GetList<BO.x69EntityRole_Assign>(sbret(), new { o23id = rec.pid, o18id = rec.o18ID });
        }
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneContact(BO.p28Contact rec,bool bolOnly4CurUser)
        {
            Init_SQL_GetList_X69();
            sb(" WHERE (a.x69RecordPid=@p28id AND a.x69RecordEntity='p28') OR (a.x69RecordPid=@p29id AND a.x69RecordEntity='p29')");
            if (bolOnly4CurUser)
            {
                SQL_OcasQueryByUser();
            }
            return _db.GetList<BO.x69EntityRole_Assign>(sbret(), new { p28id = rec.pid, p29id = rec.p29ID });
        }
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneProject(BO.p41Project rec, bool bolOnly4CurUser)
        {
            Init_SQL_GetList_X69();
            sb(" WHERE ((a.x69RecordPid=@p41id AND a.x69RecordEntity='p41') OR (a.x69RecordPid=@p42id AND a.x69RecordEntity='p42')");
            if (rec.j18ID > 0)
            {
                sb($" OR (a.x69RecordPid={rec.j18ID} AND a.x69RecordEntity='j18')");
            }
            if (rec.p28ID_Client > 0)
            {
                sb($" OR (a.x69RecordPid={rec.p28ID_Client} AND a.x69RecordEntity='p28' AND x67.x67Entity='p41')");   // a.x67ID IN (select x67ID FROM x67EntityRole WHERE x67Entity='p41'))");
            }
            sb(")");
            if (bolOnly4CurUser)
            {
                SQL_OcasQueryByUser();
            }

            return _db.GetList<BO.x69EntityRole_Assign>(sbret(), new { p41id = rec.pid, p42id = rec.p42ID });
        }
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneInvoice(BO.p91Invoice rec, bool bolOnly4CurUser)
        {
            Init_SQL_GetList_X69();
            
            sb(" WHERE ((a.x69RecordPid=@p91id AND a.x69RecordEntity='p91') OR (a.x69RecordPid=@p92id AND a.x69RecordEntity='p92'))");
            
            if (bolOnly4CurUser)
            {
                SQL_OcasQueryByUser();
            }
            //sb($" OR a.x69RecordPid IN (select p91ID FROM x73InvoiceRole_Permission WHERE p91ID=@p91id AND (j02ID={_mother.CurrentUser.pid} OR x73IsAllUsers=1))");

            return _db.GetList<BO.x69EntityRole_Assign>(sbret(), new { p91id = rec.pid, p92id = rec.p92ID });
        }
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneProforma(BO.p90Proforma rec,bool bolOnly4CurUser)
        {
            Init_SQL_GetList_X69();
            sb(" WHERE (a.x69RecordPid=@p90id AND a.x69RecordEntity='p90') OR (a.x69RecordPid=@p89id AND a.x69RecordEntity='p89')");
            if (bolOnly4CurUser)
            {
                SQL_OcasQueryByUser();
            }
            return _db.GetList<BO.x69EntityRole_Assign>(sbret(), new { p90id = rec.pid, p89id = rec.p89ID });
        }
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneTask(BO.p56Task rec, bool bolOnly4CurUser)
        {
            Init_SQL_GetList_X69();
            sb(" WHERE (a.x69RecordPid=@p56id AND a.x69RecordEntity='p56')");
            if (bolOnly4CurUser)
            {
                SQL_OcasQueryByUser();
            }
            return _db.GetList<BO.x69EntityRole_Assign>(sbret(), new { p56id = rec.pid });
        }
        public IEnumerable<BO.x69EntityRole_Assign> GetList_X69_OneMilestone(BO.o22Milestone rec, bool bolOnly4CurUser)
        {
            Init_SQL_GetList_X69();
            sb(" WHERE (a.x69RecordPid=@o22id AND a.x69RecordEntity='o22')");
            if (bolOnly4CurUser)
            {
                SQL_OcasQueryByUser();
            }
            return _db.GetList<BO.x69EntityRole_Assign>(sbret(), new { o22id = rec.pid });
        }
        public void SaveO28(int x67id, List<BO.o28ProjectRole_Workload> lisO28)
        {
            _db.RunSql("DELETE FROM o28ProjectRole_Workload WHERE x67ID=@x67id", new { x67id = x67id });
            foreach (var c in lisO28)
            {
                _db.RunSql("INSERT INTO o28ProjectRole_Workload(x67ID,p34ID,o28EntryFlag,o28PermFlag) VALUES(@x67id,@p34id,@entryflag,@permflag)", new { x67id = x67id, p34id = c.p34ID, entryflag = (int)c.o28EntryFlag, permflag = (int)c.o28PermFlag });
            }
        }

        public IEnumerable<BO.o28ProjectRole_Workload> GetListO28(int x67id)
        {
            return _db.GetList<BO.o28ProjectRole_Workload>("select a.*,p34.p34Name from o28ProjectRole_Workload a INNER JOIN p34ActivityGroup p34 ON a.p34ID=p34.p34ID WHERE a.x67ID=@pid", new { pid = x67id });
        }


        public bool IamReceiverOfList(IEnumerable<BO.x69EntityRole_Assign> lis)
        {
            foreach(var c in lis)
            {
                if (c.j02ID == _mother.CurrentUser.pid) return true;
                if (c.j11ID>0 && _mother.CurrentUser.j11IDs != null)
                {
                    if ((","+ _mother.CurrentUser.j11IDs + ",").Contains(c.j11ID.ToString()))
                    {
                        return true;
                    }
                        
                }
            }

            return false;
        }

        public IEnumerable<BO.RoleAssignedToRecord> GetRolesAssignedToRecord(int record_pid,string record_prefix)
        {
            return _db.GetList<BO.RoleAssignedToRecord>("select a.x67ID,a.x67Name,b.x71Value as Receivers,b.x71RecordPid as RecordPid,a.x67Entity as RecordEntity from x67EntityRole a INNER JOIN x71EntityRole_Inline b ON a.x67ID=b.x67ID WHERE a.x67Entity=@prefix AND b.x71RecordPid=@pid AND b.x71Value IS NOT NULL ORDER BY a.x67Ordinary", new { prefix = record_prefix,pid=record_pid });
        }
        public void ClearOneRoleAssign(int recpid, string recprefix, int x67id)
        {
            _db.RunSql("if exists(select x69ID FROM x69EntityRole_Assign WHERE x67ID=@x67id AND x69RecordEntity=@prefix AND x69RecordPid=@pid) DELETE FROM x69EntityRole_Assign WHERE x67ID=@x67id AND x69RecordPid=@pid", new { x67id = x67id,prefix=recprefix, pid = recpid });

            _db.RunSql("exec dbo.x71_recovery_one_record_one_role @pid,@prefix,@x67id", new { pid = recpid, prefix = recprefix, x67id = x67id });
        }
        public void UpdateOneRoleAssign(int recpid,string recprefix,int x67id,List<int> j02ids,List<int> j11ids,bool bolIsAll)
        {
            _db.RunSql("if exists(select x69ID FROM x69EntityRole_Assign WHERE x67ID=@x67id AND x69RecordEntity=@prefix AND x69RecordPid=@pid) DELETE FROM x69EntityRole_Assign WHERE x67ID=@x67id AND x69RecordPid=@pid", new { x67id = x67id, prefix = recprefix, pid = recpid });

            if (bolIsAll)
            {
                _db.RunSql("INSERT INTO x69EntityRole_Assign(x67ID,x69RecordEntity,x69RecordPid,x69IsAllUsers) VALUES(@x67id,@prefix,@pid,1)", new { x67id = x67id, pid = recpid,prefix=recprefix });
            }
            else
            {
                foreach(int j02id in j02ids)
                {
                    _db.RunSql("INSERT INTO x69EntityRole_Assign(x67ID,x69RecordEntity,x69RecordPid,j02ID) VALUES(@x67id,@prefix,@pid,@j02id)", new { x67id = x67id, pid = recpid, prefix = recprefix,j02id=j02id });
                }
                foreach (int j11id in j11ids)
                {
                    _db.RunSql("INSERT INTO x69EntityRole_Assign(x67ID,x69RecordEntity,x69RecordPid,j11ID) VALUES(@x67id,@prefix,@pid,@j11id)", new { x67id = x67id, pid = recpid, prefix = recprefix, j11id = j11id });
                }
            }

            _db.RunSql("exec dbo.x71_recovery_one_record_one_role @pid,@prefix,@x67id", new { pid = recpid, prefix = recprefix,x67id=x67id });
        }
        public void Recovery_x68(BO.x67EntityRole rec)
        {
            //rekonstrukce tabulky x68EntityRole_Permission

            var lis = GetList_BoundPerms(rec.pid);
            if (lis.Count() == 0 && rec.x67RoleValue.Contains("1"))
            {
                
                var perms = new List<int>();
                for (int i = 0; i < 50; i++)
                {
                    if (rec.x67RoleValue.Substring(i, 1) == "1")
                    {
                        perms.Add(i + 1);
                    }
                }
                Save(rec, perms);                
            }
        }

    }
}
