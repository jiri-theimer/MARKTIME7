using BL;
using System.Data;
using System.Drawing;

namespace UI.Models.Guru
{
    public class MigrationViewModel: BaseViewModel
    {
        public string DestDbName { get; set; }
        public string SourceDbName { get; set; }
        public string CountryCode { get; set; }
        public List<CreateLicenseKey> lisKeys { get; set; }
        public string SourceConnectString { get; set; } = "server=SQL2017.mycore.cloud\\MARKTIME;database=tomaierlegal;uid=MARKTIME;pwd=58PMapN2jhBvdblxqnIB;";
        public string DestConnectString { get; set; } = "server=LAPTOP-4H8SPDMA\\SQLEXPRESS;database=a7MARKTIME_EMPTY;uid=sa;pwd=a;";
        public int DestX01ID { get; set; } = 1;
        public string DestPassword { get; set; }
        public string x01LoginDomain { get; set; } = "mt7.cz";
        public string Message { get; set; }

        public string Vysledek { get; set; }

        public List<string> Errors { get; set; }

        public string SVN(object c)
        {
            if (Convert.IsDBNull(c) || c==null)
            {
                return "NULL";
            }
            return BO.Code.Bas.GS(c.ToString());

        }
        public string BVN(object c)
        {
            if (Convert.IsDBNull(c) || c == null)
            {
                return "0";
            }
            if (Convert.ToBoolean(c) == true)
            {
                return "1";
            }
            return "0";

        }
        public string GVN(object c,string field)
        {
            switch (field)
            {
                case "p31HoursEntryFlag":
                    if (c == System.DBNull.Value) return "0";
                    break;
            }

            if (Convert.IsDBNull(c))
            {
                if (field.Contains("UserInsert") || field.Contains("UserUpdate"))
                {
                    return "'guru'";
                }
                if (field.Contains("DateInsert") || field.Contains("DateUpdate"))
                {
                    return "GETDATE()";
                }
                return "NULL";
            }

            

            switch (c.GetType().ToString())
            {

                case "System.DateTime":
                    return BO.Code.Bas.GD(Convert.ToDateTime(c));
                case "System.Double":
                case "System.Decimal":
                    return BO.Code.Bas.GN(Convert.ToDouble(c));
                case "System.Int32":
                    if (Convert.ToInt32(c) == 0 && field.ToUpper().Contains("ID"))
                    {
                        return "NULL";
                    }
                    return Convert.ToInt32(c).ToString();
                case "System.Boolean":
                    return BO.Code.Bas.GB(Convert.ToBoolean(c));
                default:
                  
                    return BO.Code.Bas.GS(c.ToString());
            }
        }

        public string GetPrefix(object x29id,DataTable dtX29)
        {
            if (x29id == null || x29id==System.DBNull.Value)
            {
                return null;
            }
            foreach (System.Data.DataRow dbRow in dtX29.Rows)
            {
                if (Convert.ToInt32(dbRow["x29ID"]) == Convert.ToInt32(x29id))
                {
                    return dbRow["x29TableName"].ToString().Substring(0, 3);
                }
            }

            return null;
        }

        public string GetSql_ZalozitRoli(string strDB,string strEntity, string strX67Name, string strRoleValue)
        {
            string s = $"INSERT INTO {strDB}.dbo.x67EntityRole(x67Entity,x67Name,x67RoleValue,x01ID,x67DateUpdate,x67UserUpdate,x67UserInsert) VALUES('{strEntity}','{strX67Name}','{strRoleValue}',{this.DestX01ID},GETDATE(),'guru','guru')";
            return s;


        }

        public bool ZalozitRoli(BL.Factory f,string strEntity, string strX67Name,string strRoleValue)
        {
            string s = $"INSERT INTO x67EntityRole(x67Entity,x67Name,x67RoleValue,x01ID,x67DateUpdate,x67UserUpdate,x67UserInsert) VALUES('{strEntity}','{strX67Name}','{strRoleValue}',{this.DestX01ID},GETDATE(),'guru','guru')";
            return f.FBL.RunSql(s,null, this.DestConnectString);

            
        }
        public bool ZalozitTypKontaktu(BL.Factory f, string strName, int intScopeFlag,int ordinary,int billingtab,int mediatab,int contactpersonstab)
        {
            string s = $"INSERT INTO p29ContactType(p29Name,p29ScopeFlag,x01ID,p29DateUpdate,p29UserUpdate,p29UserInsert,p29Ordinary,p29RolesTab,p29BillingTab,p29ContactMediaTab,p29ContactPersonsTab) VALUES('{strName}',{intScopeFlag},{this.DestX01ID},GETDATE(),'guru','guru',{ordinary},1,{billingtab},{mediatab},{contactpersonstab})";
            return f.FBL.RunSql(s, null, this.DestConnectString);


        }

        public int GetUserJ04ID(BL.Factory f,int j02id)
        {
            if (j02id == 0) return 1;

            var dt = f.FBL.GetDataTable($"select j04ID FROM j03User WHERE j02ID={j02id}", this.SourceConnectString);
            if (dt.Rows.Count == 0) return 1;

            //return this.GVN(dt.Rows[0]["j04ID"], "j04ID");
            return Convert.ToInt32(dt.Rows[0]["j04ID"]);
        }

        public string GetUserLogin(BL.Factory f, int j02id)
        {
            var dt = f.FBL.GetDataTable($"select j03Login FROM j03User WHERE j02ID={j02id}", this.SourceConnectString);
            if (dt.Rows.Count == 0)
            {
                return "NULL";
            }
            return this.SVN(dt.Rows[0]["j03Login"]);
        }

        public int GetJ25ID(BL.Factory f, string strCode)
        {            
            var dt = f.FBL.GetDataTable($"select j25ID FROM j25ReportCategory WHERE j25Code LIKE '{strCode}'", this.DestConnectString);
            if (dt.Rows.Count == 0) return 0;
            
            return Convert.ToInt32(dt.Rows[0]["j25ID"]);
        }
        public int GetX31ID(BL.Factory f, string strX31Code)
        {
            var dt = f.FBL.GetDataTable($"select x31ID FROM x31Report WHERE x31Code LIKE '{strX31Code}'", this.DestConnectString);
            if (dt.Rows.Count == 0) return 0;

            return Convert.ToInt32(dt.Rows[0]["x31ID"]);
        }
    }
}
