using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace DL
{
    public static class BAS
    {
        public static string ParseMergeSQL(string strSQL, string strPidValue, string par1 = null, string par2 = null)
        {
            strSQL = strSQL.Replace("#pid#", strPidValue, StringComparison.OrdinalIgnoreCase);
            strSQL = strSQL.Replace("[%pid%]", strPidValue, StringComparison.OrdinalIgnoreCase);
            strSQL = strSQL.Replace("@pid", strPidValue, StringComparison.OrdinalIgnoreCase);
            par1 = BO.Code.Bas.OcistitSQL(par1);
            par2 = BO.Code.Bas.OcistitSQL(par2);

            strSQL = strSQL.Replace("#par1#", par1, StringComparison.OrdinalIgnoreCase);
            strSQL = strSQL.Replace("@par1", par1, StringComparison.OrdinalIgnoreCase);
            strSQL = strSQL.Replace("#par2#", par1, StringComparison.OrdinalIgnoreCase);

            strSQL = strSQL.Replace("@par2", par2, StringComparison.OrdinalIgnoreCase);

            strSQL = strSQL.Replace("delete ", "", StringComparison.OrdinalIgnoreCase);

            return BO.Code.Bas.OcistitSQL(strSQL);
        }

        public static bool SaveX69(DL.DbHandler db, string recprefix, int recpid, List<BO.x69EntityRole_Assign> lisX69)
        {
            string guid = BO.Code.Bas.GetGuid();
            foreach (var c in lisX69.Where(p=>p.j02ID>0 || p.j11ID>0 || p.x69IsAllUsers))
            {
                db.RunSql("INSERT INTO p85TempBox(p85GUID,p85DataPID,p85OtherKey1,p85OtherKey2,p85prefix,p85FreeBoolean01) VALUES(@guid,@x67id,@j02id,@j11id,@prefix,@isall)", new { guid = guid, x67id = c.x67ID, j02id = BO.Code.Bas.TestIntAsDbKey(c.j02ID), j11id = BO.Code.Bas.TestIntAsDbKey(c.j11ID), prefix = recprefix, isall = c.x69IsAllUsers });
            }

            var pars = new Dapper.DynamicParameters();

            pars.Add("recordpid", recpid, DbType.Int32);
            pars.Add("entity", recprefix, DbType.String);
            pars.Add("guid", guid, DbType.String);
            pars.Add("x67id_only_onerole_update", 0, DbType.Int32);


            if (db.RunSp("x69_save_one_record", ref pars, false) == "1")
            {
                return true;
            }

            return false;

        }

        public static bool SaveJ08(DL.DbHandler db, string recprefix, int recpid, List<BO.j08CreatePermission> lisJ08)
        {
            db.RunSql("DELETE FROM j08CreatePermission where j08RecordEntity=@prefix AND j08RecordPid=@pid", new { prefix = recprefix, pid = recpid });

            foreach (var c in lisJ08)
            {
                db.RunSql("INSERT INTO j08CreatePermission(j08RecordEntity,j08RecordPid,j02ID,j11ID,j04ID,j08IsAllUsers) VALUES(@prefix,@pid,@j02id,@j11id,@j04id,@isall)", new { prefix = recprefix, pid = recpid, j02id = BO.Code.Bas.TestIntAsDbKey(c.j02ID), j11id = BO.Code.Bas.TestIntAsDbKey(c.j11ID), j04id = BO.Code.Bas.TestIntAsDbKey(c.j04ID), isall = c.j08IsAllUsers });
            }


            return true;

        }

        public static bool SaveFreeFields(DL.DbHandler db, int intSourcePID, List<BO.FreeFieldInput> lisFFI)
        {
            if (lisFFI == null || lisFFI.Where(p => p.IsVisible).Count() == 0)
            {
                return true;
            }
            string strTable = lisFFI[0].SourceTableName;
            string strPrefix = strTable.Substring(0, 3);
            string strFieldPID = $"{strPrefix}ID";

            int intSavedPID = db.GetIntegerFromSql($"select {strFieldPID} FROM {strTable} WHERE {strFieldPID}={intSourcePID}");

            if (lisFFI.Where(p => !p.IsVisible).Count() > 0)
            {
                lisFFI = lisFFI.Where(p => p.IsVisible == true).ToList();   //ukládat pouze viditelná pole na formuláři
            }
            var p = new Params4Dapper();
            p.AddInt("pid", intSourcePID);

            var ss = new System.Text.StringBuilder();

            foreach (var c in lisFFI)
            {

                switch (c.TypeName)
                {
                    case "date":
                    case "datetime":
                        if (c.x28IsRequired && c.DateInput == null)
                        {
                            db.CurrentUser.AddMessage($"Pole ** {c.x28Name} ** je povinné k vyplnění. "); return false;
                        }
                        p.AddDateTime(c.x28Field, c.DateInput);
                        if (c.x28ReminderNotifyBefore > 0)
                        {
                            //datum (uživatelské pole) se má připomínat
                            
                            int intO24ID = db.GetIntegerFromSql($"select o24ID FROM o24Reminder WHERE o24RecordEntity='{c.x28Entity}' AND o24RecordPid={intSavedPID} AND o24Flag='{c.x28Field}'");
                            
                            if (c.DateInput != null)
                            {
                                var platnostdo = db.Load<BO.GetDate>($"SELECT {strPrefix}ValidUntil as Value FROM {BO.Code.Entity.GetEntity(strPrefix)} WHERE {strFieldPID}={intSavedPID}").Value;

                                DateTime dd = c.DateInput.Value.AddHours(-1 * (double)c.x28ReminderNotifyBefore);
                                int intJ02ID = db.GetIntegerFromSql($"select j02ID_Owner FROM {BO.Code.Entity.GetEntity(c.x28Entity)} WHERE {c.x28Entity}ID={intSavedPID}");
                                if (intO24ID == 0)
                                {
                                    ss.AppendLine($"INSERT INTO o24Reminder(o24Count,o24Unit,o24Flag,j02ID,o24StaticDate,o24RecordDate,o24RecordPid,o24RecordEntity,o24MediumFlag,x01ID,o24Memo,o24DateUpdate,o24UserUpdate,o24UserInsert,o24ValidUntil) VALUES({-1 * c.x28ReminderNotifyBefore},'h','{c.x28Field}',{intJ02ID},{BO.Code.Bas.GD(dd)},{BO.Code.Bas.GD(c.DateInput)},{intSavedPID},'{c.x28Entity}',1,{c.x01ID},'{c.x28Name}',GETDATE(),'basDL','basDL',{BO.Code.Bas.GD(platnostdo.Value)});");
                                }
                                else
                                {
                                    ss.AppendLine($"UPDATE o24Reminder set o24Count={-1 * c.x28ReminderNotifyBefore},o24Unit='h', o24StaticDate={BO.Code.Bas.GD(dd)},o24RecordDate={BO.Code.Bas.GD(c.DateInput)},o24ValidUntil={BO.Code.Bas.GD(platnostdo)} WHERE o24ID={intO24ID};");
                                }
                            }
                            else
                            {
                                if (intO24ID > 0)
                                {
                                    ss.AppendLine($"DELETE FROM o24Reminder WHERE o24ID={intO24ID};");
                                }
                            }                                                        
                            
                        }
                        break;
                    case "decimal":
                    case "integer":
                        if (c.x28IsRequired && c.NumInput == 0)
                        {
                            db.CurrentUser.AddMessage($"Pole ** {c.x28Name} ** je povinné k vyplnění. "); return false;
                        }
                        p.AddDouble(c.x28Field, c.NumInput);
                        break;
                    case "boolean":
                        p.AddBool(c.x28Field, c.CheckInput);
                        break;
                    default:
                        if (c.x28IsRequired && string.IsNullOrEmpty(c.StringInput))
                        {
                            db.CurrentUser.AddMessage($"Pole ** {c.x28Name} ** je povinné k vyplnění. "); return false;
                        }
                        if (c.StringInput != null && c.StringInput.Length > 20)
                        {
                            //kontrola velikosti obsahu string polí
                            int intMaxSize = db.Load<BO.GetInteger>("select dbo.getfieldsize(@fld, @tbl) as Value", new { fld = c.x28Field, tbl = strTable }).Value;
                            if (intMaxSize > 0 && c.StringInput.Length > intMaxSize)
                            {
                                db.CurrentUser.AddMessage($"Délka pole ** {c.x28Name} ** může být maximálně {intMaxSize} znaků. Vy posíláte {c.StringInput.Length} znaků."); return false;
                            }
                        }

                        p.AddString(c.x28Field, c.StringInput);
                        break;
                }

            }


            string strSQL = "";

            if (intSavedPID == 0)
            {

                strSQL = "INSERT INTO " + strTable + " (" + strFieldPID + "," + string.Join(",", lisFFI.Select(p => p.x28Field)) + ") VALUES (@pid," + string.Join(",", lisFFI.Select(p => "@" + p.x28Field)) + ")";
            }
            else
            {
                strSQL = "UPDATE " + strTable + " SET " + string.Join(",", lisFFI.Select(p => p.x28Field + " = @" + p.x28Field)) + " WHERE " + strFieldPID + " = @pid";
            }


            if ( db.RunSql(strSQL, p.getDynamicDapperPars()))
            {
                if (ss.Length > 0)
                {
                    db.RunSql(ss.ToString());
                }
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}
