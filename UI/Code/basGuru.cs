namespace UI.Code
{
    public static class basGuru
    {
        public static System.Data.DataTable Load_DataTable_From_File(BL.Factory f,string strTable)
        {
            var strFile = $"{f.App.RootUploadFolder}\\_distribution\\{strTable}.dt";
            return Json_To_DataTable(BO.Code.File.GetFileContent(strFile));
        }
        public static System.Data.DataTable Json_To_DataTable(string strJson)
        {
            System.Data.DataTable dt = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(strJson);
            return dt;
        }

        public static List<BO.StringPair> GetColumns(System.Data.DataTable dt)
        {
            var lis = new List<BO.StringPair>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                lis.Add(new BO.StringPair() { Key = dt.Columns[i].ColumnName, Value = dt.Columns[i].GetType().ToString() });
            }
            return lis;
        }

        public static string GVN(object c, string field, int intX01ID)
        {
            if (field.ToLower().Contains("x01id"))
            {
                return intX01ID.ToString();
            }
            if (field.Contains("DateInsert") || field.Contains("DateUpdate") || field.Contains("ValidFrom"))
            {
                return "GETDATE()";
            }
            if (field.Contains("UserInsert") || field.Contains("UserUpdate"))
            {
                return "'setup'";
            }

            if (Convert.IsDBNull(c) || field.ToLower().Contains("j11id") || field.ToLower().Contains("b01id"))
            {
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
                case "System.Int64":
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
    }
}
