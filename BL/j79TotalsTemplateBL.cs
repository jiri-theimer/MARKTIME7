
namespace BL
{
    public interface Ij79TotalsTemplateBL
    {
        public BO.j79TotalsTemplate Load(int pid);
        public IEnumerable<BO.j79TotalsTemplate> GetList(int j02id,string prefix);
        public int Save(BO.j79TotalsTemplate rec, List<int> j04ids, List<int> j11ids);
        public int CreateDefaultSysRecord(int j02id,string prefix);

    }
    class j79TotalsTemplateBL : BaseBL, Ij79TotalsTemplateBL
    {
        public j79TotalsTemplateBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("j79"));
            sb(" FROM j79TotalsTemplate a INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID AND x67x.x01ID="+_mother.CurrentUser.x01ID.ToString());
            sb(strAppend);
            return sbret();
        }
        public BO.j79TotalsTemplate Load(int pid)
        {
            return _db.Load<BO.j79TotalsTemplate>(GetSQL1(" WHERE a.j79ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.j79TotalsTemplate> GetList(int j02id,string prefix)
        {
            var mq = new BO.myQuery("j79") { explicit_orderby = "a.j79IsSystem DESC,a.j79Ordinary,a.j79Name" };
            string s = $" WHERE a.j79Entity IS NULL AND (a.j02ID={j02id} OR a.j79IsPublic=1 OR a.j79ID IN (select j79ID FROM j80TotalsReceiver WHERE j04ID={_mother.CurrentUser.j04ID}))";
            if (!string.IsNullOrEmpty(prefix))
            {
                s = $" WHERE a.j79Entity = '{prefix}' AND (a.j02ID={j02id} OR a.j79IsPublic=1 OR a.j79ID IN (select j79ID FROM j80TotalsReceiver WHERE j04ID={_mother.CurrentUser.j04ID}))";
            }
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(s), mq, _mother.CurrentUser);
            return _db.GetList<BO.j79TotalsTemplate>(fq.FinalSql, fq.Parameters);
        }

        public int CreateDefaultSysRecord(int j02id,string prefix)
        {
            var rec = new BO.j79TotalsTemplate() { j02ID = j02id, j79IsSystem = true,j79Entity=prefix,j79Name="Výchozí šablona statistiky" };
            switch (rec.j79Entity)
            {
                case "j02":
                    rec.j79Columns = "p31_p41__p41Project__KlientProjektu,a__p31Worksheet__p31Hours_Orig,a__p31Worksheet__p31Amount_WithoutVat_Orig";
                    break;
                case "p28":
                    rec.j79Columns = "p31_j02__j02User__fullname_desc,a__p31Worksheet__p31Hours_Orig,a__p31Worksheet__p31Amount_WithoutVat_Orig";
                    break;
                case "p56":
                    rec.j79Columns = "p31_j02__j02User__fullname_desc,a__p31Worksheet__p31Hours_Orig,a__p31Worksheet__p31Amount_WithoutVat_Orig";
                    break;
                case "p91":
                    rec.j79Columns = "p31_p41__p41Project__NazevProjektu,p31_j02__j02User__fullname_desc,p32_p34__p34ActivityGroup__p34Name,a__p31Worksheet__p31Hours_Orig,a__p31Worksheet__p31Hours_Invoiced,a__p31Worksheet__p31Amount_WithoutVat_Invoiced,a__p31Worksheet__Vyfakturovano_Hodiny_Pausal,a__p31Worksheet__Vyfakturovano_Hodiny_Odpis";
                    break;
                case "p41":
                case "le5":
                case "le4":
                    rec.j79Columns = "p31_j02__j02User__fullname_desc,p31_p32__p32Activity__p32Name,a__p31Worksheet__p31Hours_Orig,a__p31Worksheet__p31Amount_WithoutVat_Orig";
                    break;
                default:
                    rec.j79Columns = "p31_p41__p41Project__KlientProjektu,p31_j02__j02User__fullname_desc,a__p31Worksheet__p31Hours_Orig,a__p31Worksheet__p31Amount_WithoutVat_Orig";
                    rec.j79GroupField1 = "p31_p41__p41Project__KlientProjektu";
                    break;
            }
            
            return Save(rec,null,null);

        }

        public int Save(BO.j79TotalsTemplate rec, List<int> j04ids, List<int> j11ids)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddBool("j79IsSystem", rec.j79IsSystem);
            if (rec.j02ID == 0)
            {
                rec.j02ID = _mother.CurrentUser.pid;
            }
            p.AddInt("j02ID", rec.j02ID,true);
            p.AddString("j79Entity", rec.j79Entity);
            p.AddString("j79Name", rec.j79Name);
            p.AddString("j79Columns", rec.j79Columns);
            p.AddString("j79GroupField1", rec.j79GroupField1);
            p.AddString("j79GroupField2", rec.j79GroupField2);
            p.AddString("j79GroupField3", rec.j79GroupField3);
            p.AddString("j79PivotField", rec.j79PivotField);
            p.AddString("j79PivotValue", rec.j79PivotValue);
            p.AddInt("j79StateQuery", rec.j79StateQuery);
            p.AddString("j79TabQuery", rec.j79TabQuery);
            p.AddBool("j79IsPublic", rec.j79IsPublic);
            p.AddString("j79Query_j02IDs", rec.j79Query_j02IDs);
            p.AddString("j79Query_j11IDs", rec.j79Query_j11IDs);
            p.AddString("j79Query_j07IDs", rec.j79Query_j07IDs);
            p.AddInt("j79Ordinary", rec.j79Ordinary);
            p.AddString("j79AddQuery", rec.j79AddQuery);

            int intJ79ID= _db.SaveRecord("j79TotalsTemplate", p, rec);
            if (intJ79ID > 0 && j04ids != null && j11ids != null)
            {
                if (rec.pid > 0)
                {
                    _db.RunSql("if EXISTS(select j80ID FROM j80TotalsReceiver WHERE j79ID=@pid) DELETE FROM j80TotalsReceiver WHERE j79ID=@pid", new { pid = intJ79ID });
                }
                if (j04ids.Count > 0)
                {
                    _db.RunSql("INSERT INTO j80TotalsReceiver(j79ID,j04ID) SELECT @pid,j04ID FROM j04UserRole WHERE j04ID IN (" + string.Join(",", j04ids) + ")", new { pid = intJ79ID });
                }
                if (j11ids.Count > 0)
                {
                    _db.RunSql("INSERT INTO j80TotalsReceiver(j79ID,j04ID) SELECT @pid,j11ID FROM j11Team WHERE j11ID IN (" + string.Join(",", j11ids) + ")", new { pid = intJ79ID });
                }
            }

            return intJ79ID;

        }
        private bool ValidateBeforeSave(BO.j79TotalsTemplate rec)
        {
            if (string.IsNullOrEmpty(rec.j79Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.j79Columns))
            {
                this.AddMessage("Chybí vyplnit [Sloupce]."); return false;
            }

            return true;
        }

    }
}
