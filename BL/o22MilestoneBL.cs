

using BO;

namespace BL
{
    public interface Io22MilestoneBL
    {
        public BO.o22Milestone Load(int pid);

        public IEnumerable<BO.o22Milestone> GetList(BO.myQueryO22 mq);
        public int Save(BO.o22Milestone rec, List<BO.x69EntityRole_Assign> lisX69);
        public BO.o22RecDisposition InhaleRecDisposition(int pid, BO.o22Milestone rec = null);
        public DateTime GetKonecLhuty(DateTime d0, int delka, string jednotka);
        public IEnumerable<BO.o22MilestoneDayline> GetList_Dayline(BO.myQueryO22 mq, int intX67ID);
        public BO.o22MilestoneSum LoadSumRow(int pid);

    }
    class o22MilestoneBL : BaseBL, Io22MilestoneBL
    {
        public o22MilestoneBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null, string strAppendColumns = null)
        {
            sb("SELECT a.p41ID,a.o21ID,a.j02ID_Owner,a.o22Name,a.o22ExternalCode,a.o22PlanFrom,a.o22PlanUntil,a.o22ExternalCode");
            sb(",a.o22IsAllDay,a.o22Location");
            sb(",p28client.p28Name as ProjectClient,o21x.o21Name,o21x.o21TypeFlag,o21x.o21Color,isnull(p41.p41NameShort,p41.p41Name) as p41Name,p41.p41Code,j02owner.j02LastName+' '+j02owner.j02FirstName as Owner");
            sb("," + _db.GetSQL1_Ocas("o22"));
            
            sb(",a.o22Notepad,a.x04ID,b05.b05RecordPid,b05.b05RecordEntity,a.o22DurationCount,a.o22DurationUnit,a.o22DurationCalcFlag");
            if (strAppendColumns != null)
            {
                sb(strAppendColumns);
            }
            sb(" FROM o22Milestone a");
            sb(" INNER JOIN o21MilestoneType o21x ON a.o21ID=o21x.o21ID LEFT OUTER JOIN p41Project p41 ON a.p41ID=p41.p41ID");
            sb(" LEFT OUTER JOIN p28Contact p28client ON p41.p28ID_Client=p28client.p28ID");            
            sb(" LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");
            
            sb(" LEFT OUTER JOIN b05Workflow_History b05 ON a.o22ID=b05.o22ID");
            sb(strAppend);
            return sbret();
        }
        public BO.o22Milestone Load(int pid)
        {
            return _db.Load<BO.o22Milestone>(GetSQL1(" WHERE a.o22ID=@pid"), new { pid = pid });
        }


        public IEnumerable<BO.o22Milestone> GetList(BO.myQueryO22 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o22Milestone>(fq.FinalSql, fq.Parameters);
        }

        public IEnumerable<BO.o22MilestoneDayline> GetList_Dayline(BO.myQueryO22 mq,int intX67ID)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(" INNER JOIN x69EntityRole_Assign m1 ON a.o22ID=m1.x69RecordPid AND m1.x69RecordEntity='o22' AND m1.x67ID="+ intX67ID.ToString()+" LEFT OUTER JOIN j02User prijemce ON m1.j02ID=prijemce.j02ID", ",m1.j02ID,m1.j11ID,m1.x69IsAllUsers,prijemce.j02LastName,prijemce.j02FirstName"), mq, _mother.CurrentUser);
            return _db.GetList<BO.o22MilestoneDayline>(fq.FinalSql, fq.Parameters);
        }

        public int Save(BO.o22Milestone rec, List<BO.x69EntityRole_Assign> lisX69)
        {
            if (rec.o21ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Typ]."); return 0;
            }
            var recO21 = _mother.o21MilestoneTypeBL.Load(rec.o21ID);

            if (ValidateBeforeSave(rec, recO21) == false)
            {
                return 0;
            }
            int intPID = 0;
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("o21ID", rec.o21ID, true);
                p.AddInt("p41ID", rec.p41ID, true);
                if (rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;
                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);

                p.AddString("o22Name", rec.o22Name);
                p.AddString("o22Location", rec.o22Location);
                p.AddBool("o22IsAllDay", rec.o22IsAllDay);
                p.AddString("o22ExternalCode", rec.o22ExternalCode);
                p.AddString("o22Notepad", rec.o22Notepad);
                p.AddInt("x04ID", rec.x04ID, true);

                p.AddDateTime("o22PlanFrom", rec.o22PlanFrom);
                p.AddDateTime("o22PlanUntil", rec.o22PlanUntil);
                p.AddInt("o22DurationCount", rec.o22DurationCount);
                p.AddString("o22DurationUnit", rec.o22DurationUnit);
                p.AddEnumInt("o22DurationCalcFlag", rec.o22DurationCalcFlag);

                intPID = _db.SaveRecord("o22Milestone", p, rec);
                if (intPID > 0)
                {
                    if (lisX69 != null && !DL.BAS.SaveX69(_db, "o22", intPID, lisX69))
                    {
                        this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                        return 0;
                    }

                    sc.Complete();
                }
            }





            return intPID;
        }

        public bool ValidateBeforeSave(BO.o22Milestone rec,BO.o21MilestoneType recO21)
        {
            if (string.IsNullOrEmpty(rec.o22Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (recO21.o21TypeFlag == BO.o21TypeFlagEnum.Lhuta || recO21.o21TypeFlag == BO.o21TypeFlagEnum.Udalost)
            {
                if (rec.o22PlanFrom==null || rec.o22PlanUntil == null)
                {
                    this.AddMessage("Neúplné období od-do."); return false;
                }
            }
            if (recO21.o21TypeFlag == BO.o21TypeFlagEnum.Milnik && rec.o22PlanUntil == null)
            {
                this.AddMessage("Neúplné [Datum]."); return false;
            }
            if (recO21.o21TypeFlag == BO.o21TypeFlagEnum.Udalost && rec.o22PlanUntil != null)
            {
                if (rec.o22PlanUntil.Value.Hour==0 && rec.o22PlanUntil.Value.Minute == 0)
                {
                    rec.o22PlanUntil = BO.Code.Bas.ConvertDateTo235959(rec.o22PlanUntil.Value);
                }
            }

            if (rec.o22PlanFrom !=null && rec.o22PlanUntil !=null && rec.o22PlanFrom.Value > rec.o22PlanUntil.Value)
            {
                this.AddMessage("Začátek je větší než konec."); return false;
            }


            return true;
        }


        public BO.o22RecDisposition InhaleRecDisposition(int pid, BO.o22Milestone rec = null)
        {
            var c = new BO.o22RecDisposition();
            if (rec == null)
            {
                rec = Load(pid);
            }
            if (rec.j02ID_Owner == _mother.CurrentUser.j02ID || _mother.CurrentUser.IsAdmin || _mother.CurrentUser.TestPermission(PermValEnum.GR_o22_Owner))
            {
                c.OwnerAccess = true; c.ReadAccess = true;
                return c;
            }

            if (_mother.CurrentUser.TestPermission(PermValEnum.GR_o22_Reader))
            {
                c.ReadAccess = true;
            }


            var lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneMilestone(rec, true);
            foreach (var role in lisX69)
            {
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.o22_Owner))
                {
                    c.OwnerAccess = true; c.ReadAccess = true;  //vlastník
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                    return c;
                }
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.o22_Reader))
                {
                    c.ReadAccess = true;
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                }
            }


            return c;
        }

        public BO.o22MilestoneSum LoadSumRow(int pid)
        {
            return _db.Load<BO.o22MilestoneSum>("EXEC dbo.o22_inhale_sumrow @j02id_sys,@pid", new { j02id_sys = _mother.CurrentUser.pid, pid = pid });
        }

        public DateTime GetKonecLhuty(DateTime d0, int delka,string jednotka)
        {
            DateTime d = d0;

            switch (jednotka)
            {
                case "d":
                    d = d.AddDays(delka);
                    break;
                case "e":   //pracovní dny
                    d = GetKonecLhuty_PracovniDny(d0, delka);
                    break;
                case "w":
                    d = d.AddDays(delka * 7);
                    break;
                case "m":
                    d = d.AddMonths(delka);
                    break;
                case "y":
                    d = d.AddYears(delka);
                    break;

            }

            return d;

            
        }



        private DateTime GetKonecLhuty_PracovniDny(DateTime d0,int pocetdni)
        {
            var lisSvatky = _mother.c26HolidayBL.GetList(new BO.myQueryC26() { global_d1 = d0, global_d2 = d0.AddDays(pocetdni*2) });

            DateTime d = d0;
            DateTime d2 = d;
            int x = 0;

            while (x<= pocetdni)
            {
                if (!(d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday))
                {
                    if (!lisSvatky.Any(p => p.c26Date == d))
                    {
                        x += 1;
                    }

                }

                d = d.AddDays(1);
            }


            return d;
        }

    }
}
