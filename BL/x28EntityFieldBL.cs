using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ix28EntityFieldBL
    {
        public BO.x28EntityField Load(int pid);
        public IEnumerable<BO.x28EntityField> GetList(BO.myQuery mq);
        public IEnumerable<BO.x28EntityField> GetList_ApplicableInForm(string prefix, int intEntityTypeID, bool bolTestUserAccess);
        public int Save(BO.x28EntityField rec,List<BO.x26EntityField_Binding> lisX26);
        public IEnumerable<BO.x26EntityField_Binding> GetList_x26(int x28id);
        public System.Data.DataTable GetFieldsValues(int pid, IEnumerable<BO.x28EntityField> fields);   //vrací hodnoty polí v odpovídající tabulce entity

    }
    class x28EntityFieldBL : BaseBL, Ix28EntityFieldBL
    {
        public x28EntityFieldBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null,bool istestcloud=false)
        {
            sb("select a.*,x27.x27Name,lower(x24.x24Name) as TypeName,");
            sb(_db.GetSQL1_Ocas("x28"));
            sb(" FROM x28EntityField a inner join x24DataType x24 on a.x24id=x24.x24id LEFT OUTER JOIN x27EntityFieldGroup x27 ON a.x27ID=x27.x27ID");

            if (istestcloud)
            {
                sb(this.AppendCloudQuery(strAppend));
            }
            else
            {
                sb(strAppend);
            }
            return sbret();
        }
        public BO.x28EntityField Load(int pid)
        {
            return _db.Load<BO.x28EntityField>(GetSQL1(" WHERE a.x28ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.x28EntityField> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x28EntityField>(fq.FinalSql, fq.Parameters);
        }
        public IEnumerable<BO.x28EntityField> GetList_ApplicableInForm(string prefix, int intEntityTypeID,bool bolTestUserAccess)   //vrátí seznam polí aplikovatelných pro záznam entity prefix
        {
            //v intEntityTypeID je hodnota j07ID/p42ID/p92ID...

            string s = GetSQL1($" WHERE a.x28Flag=1 AND GETDATE() BETWEEN a.x28ValidFrom AND a.x28ValidUntil AND a.x28Entity='{prefix}'",_mother.CurrentUser.IsHostingModeTotalCloud);
            

            string strPrefix4Type = null;
            switch (prefix)
            {
                case "p41":
                    strPrefix4Type="p42"; break;               
                case "j02":
                    strPrefix4Type = "j07"; break;
                case "p28":
                    strPrefix4Type = "p29"; break;
                case "p31":
                    strPrefix4Type="p34"; break;
                case "p91":
                    strPrefix4Type="p92"; break;
                case "p90":
                    strPrefix4Type="p89"; break;
                case "p56":
                    strPrefix4Type = "p57"; break;
                case "o22":
                    strPrefix4Type = "o21"; break;
            }
            if (strPrefix4Type is null)
            {
                s += " AND a.x28IsAllEntityTypes=1";
            }
            else
            {
                s += $" AND (a.x28IsAllEntityTypes=1 OR a.x28ID IN (select x28ID FROM x26EntityField_Binding WHERE x26RecTypeEntity='{strPrefix4Type}' AND x26RecTypePid={intEntityTypeID}))";
               
            }
            if (bolTestUserAccess)
            {
                s += " AND (a.x28IsPublic=1 OR ','+a.x28NotPublic_j04IDs+',' LIKE '%," + _mother.CurrentUser.j04ID.ToString() + ",%'";
                if (_mother.CurrentUser.j07ID > 0)
                {
                    s += " OR ','+a.x28NotPublic_j07IDs+',' LIKE '%," + _mother.CurrentUser.j07ID.ToString() + ",%'";
                }
                s += ")";
            }
            return _db.GetList<BO.x28EntityField>(s);
        }
        public IEnumerable<BO.x26EntityField_Binding> GetList_x26(int x28id)
        {
            return _db.GetList<BO.x26EntityField_Binding>("select *,convert(bit,1) as IsChecked from x26EntityField_Binding where x28ID=@pid", new { pid = x28id });
        }

        public System.Data.DataTable GetFieldsValues(int pid, IEnumerable<BO.x28EntityField> fields)
        {
            if (fields.Count() == 0)
            {
                return null;
            }

            //string strSQL = "SELECT "+pid.ToString()+" AS pid," + string.Join(",", fields.Select(p => p.x28Field)) + " FROM " + fields.First().SourceTableName + " WHERE " + fields.First().x28Entity + "ID = "+pid.ToString();
            string strSQL = $"SELECT {pid} AS pid,{string.Join(",", fields.Select(p => p.x28Field))} FROM {fields.First().SourceTableName} WHERE {fields.First().x28Entity}ID = {pid}";


            return _db.GetDataTable(strSQL);
        }

        public int Save(BO.x28EntityField rec, List<BO.x26EntityField_Binding> lisX26)
        {
            if (string.IsNullOrEmpty(rec.x28Field) && rec.x28Flag == BO.x28FlagENUM.UserField)
            {
                rec.x28Field = FindFirstUsableField(rec.x28Entity, rec.x24ID);
            }
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddEnumInt("x28Flag", rec.x28Flag);
            p.AddString("x28Name", rec.x28Name);
            p.AddInt("x28Ordinary", rec.x28Ordinary);
            p.AddString("x28Field", rec.x28Field);
            p.AddEnumInt("x24id", rec.x24ID);
            p.AddString("x28Entity", rec.x28Entity);
            p.AddInt("x27ID", rec.x27ID,true);
            
            p.AddString("x28datasource", rec.x28DataSource);
            p.AddBool("x28IsFixedDataSource", rec.x28IsFixedDataSource);
            p.AddBool("x28IsRequired", rec.x28IsRequired);            
            p.AddBool("x28IsPublic", rec.x28IsPublic);
            if (rec.x28IsPublic)
            {
                rec.x28NotPublic_j04IDs = null; rec.x28NotPublic_j07IDs = null;
            }
            p.AddString("x28NotPublic_j04IDs", rec.x28NotPublic_j04IDs);
            p.AddString("x28NotPublic_j07IDs", rec.x28NotPublic_j07IDs);


            p.AddString("x28Grid_Field", rec.x28Grid_Field);
            p.AddString("x28Grid_SqlSyntax", rec.x28Grid_SqlSyntax);
            p.AddString("x28Grid_SqlFrom", rec.x28Grid_SqlFrom);
            p.AddString("x28Pivot_SelectSql", rec.x28Pivot_SelectSql);
            p.AddString("x28Pivot_GroupBySql", rec.x28Pivot_GroupBySql);

            p.AddString("x28Query_SqlSyntax", rec.x28Query_SqlSyntax);
            p.AddString("x28Query_Field", rec.x28Query_Field);
            p.AddString("x28Query_sqlComboSource", rec.x28Query_sqlComboSource);

            p.AddInt("x28TextboxHeight", rec.x28TextboxHeight);
            p.AddInt("x28TextboxWidth", rec.x28TextboxWidth);

            p.AddString("x28HelpText", rec.x28HelpText);
            p.AddBool("x28IsAllEntityTypes", rec.x28IsAllEntityTypes);
            p.AddInt("x28ReminderNotifyBefore", rec.x28ReminderNotifyBefore);
            

            int intPID = _db.SaveRecord("x28EntityField", p, rec);
            if (lisX26 != null)
            {
                rec.x28IsAllEntityTypes = true;
                if (rec.pid > 0 && intPID>0)
                {
                    _db.RunSql("DELETE FROM x26EntityField_Binding WHERE x28ID=@pid", new { pid = intPID });
                }
                if (intPID > 0)
                {
                    foreach (var c in lisX26.Where(p => p.IsChecked))
                    {
                        _db.RunSql("INSERT INTO x26EntityField_Binding(x28ID,x26RecTypePid,x26RecTypeEntity,x26IsEntryRequired) VALUES(@pid,@typepid,@entity,@b)", new { pid = intPID, typepid = c.x26RecTypePid, entity = c.x26RecTypeEntity, b = c.x26IsEntryRequired });
                        rec.x28IsAllEntityTypes = false;
                    }
                }
                _db.RunSql("UPDATE x28EntityField set x28IsAllEntityTypes=@b WHERE x28ID=@pid", new { pid = intPID, b = rec.x28IsAllEntityTypes });
            }

            return intPID;
        }
        private bool ValidateBeforeSave(BO.x28EntityField rec)
        {
            switch (rec.x28Flag)
            {
                case BO.x28FlagENUM.UserField:
                    if (rec.x28Entity == "o23")
                    {
                        this.AddMessage("Pro entitu [Dokument] je povoleno zakládat pouze GRID sloupec na míru.");return false;
                    }
                   
                    if (string.IsNullOrEmpty(rec.x28Field))
                    {
                        this.AddMessage("Systém nedokázal najít volné fyzické pole pro toto uživatelské pole."); return false;
                    }
                    if (rec.x24ID !=BO.x24IdENUM.tDate && rec.x24ID != BO.x24IdENUM.tDateTime && rec.x28ReminderNotifyBefore>0)
                    {
                        this.AddMessage("Automatická notikace se vyplňuje pouze u datumového pole."); return false;
                    }
                    if (rec.x28TextboxHeight>0 && rec.x28DataSource != null)
                    {
                        this.AddMessage("Poznámkové pole nemůže mít definiční obor hodnot."); return false;
                    }
                    break;
                case BO.x28FlagENUM.GridField:
                    if (string.IsNullOrEmpty(rec.x28Grid_Field))
                    {
                        Random rnd = new Random();
                        rec.x28Grid_Field = "gridfield_" + rnd.Next(1, 10000).ToString();
                    }
                    if (string.IsNullOrEmpty(rec.x28Grid_Field) && string.IsNullOrEmpty(rec.x28Query_Field))
                    {
                        this.AddMessage("Chybná specifikace pole.");return false;
                    }
                    if (rec.x28Grid_Field.Contains(".") || rec.x28Grid_Field.Contains("[") || rec.x28Grid_Field.Contains("]"))
                    {
                        this.AddMessage("Pole obsahuje zakázané znaky.");return false;
                    }
                    break;

            }
            if (string.IsNullOrEmpty(rec.x28Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (!rec.x28IsPublic)
            {
                if (string.IsNullOrEmpty(rec.x28NotPublic_j04IDs) && string.IsNullOrEmpty(rec.x28NotPublic_j07IDs))
                {
                    this.AddMessage("Pokud pole není dostupné všem uživatelům, zaškrtněte okruh rolí nebo pozic.");return false;
                }
            }
            

            return true;
        }


        public string FindFirstUsableField(string strPrefix, BO.x24IdENUM x24id)
        {

            string stype = x24id.ToString().ToLower();
            stype = stype.Substring(1, stype.Length-1);

            return _db.Load<BO.GetString>("select dbo.x28_getFirstUsableField('" +BO.Code.Entity.GetEntity(strPrefix) + "','" + stype + "') as Value").Value;
        }


    }
}
