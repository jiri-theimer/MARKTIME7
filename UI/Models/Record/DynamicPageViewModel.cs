using UI.Models.a55;

namespace UI.Models.Record
{
    public class DynamicPageViewModel:BaseViewModel
    {
        public int a55ID { get; set; }
        public int b02ID { get; set; }
        public int rec_pid { get; set; }
        public string rec_prefix { get; set; }

        public BO.a55RecPage RecA55 { get; set; }
        public BO.a59RecPageLayer RecA59 { get; set; }

        public string portlet_cssclass { get; set; }

        public string BoxColCss { get; set; } = "col-lg-6";
        public WebpageLayerEnvironment DockStructure { get; set; }
        public int ColumnsPerPage { get; set; }
        public List<BO.a58RecPageBox> lisUserWidgets { get; set; }
        public List<BO.a58RecPageBox> lisAllWidgets { get; set; }


        public int SearchedPid { get; set; }
        public string SearchedText { get; set; }

        public string MergeStaticHtmlText(BL.Factory f, string strHtmlText,int rec_pid)       //V HTML textu mohou být SQL dotazy
        {
            if (string.IsNullOrEmpty(strHtmlText) || !strHtmlText.Contains("{") || !strHtmlText.Contains("}"))
            {
                return strHtmlText;
            }

           
            var keys = new List<BO.StringPair>();
            keys.Add(new BO.StringPair() { Key = "@pid", Value = rec_pid.ToString() });
            keys.Add(new BO.StringPair() { Key = "@j02login", Value = f.CurrentUser.j02Login });
            keys.Add(new BO.StringPair() { Key = "@j02id", Value = f.CurrentUser.pid.ToString() });
           

            var sqls = BO.Code.MergeContent.GetAllMergeExternalSqlsInContent(strHtmlText, keys);  //vrátí všechny SQL externí dotazy
            foreach (BO.MergeExternalSql sql in sqls)
            {               
                var dbExternal = new DL.DbHandler(f.App.ConnectString, f.CurrentUser, f.App.LogFolder); //db connection na databázi                    
                string ret = null;
                if (sql.MergedSql.Contains("select ", StringComparison.OrdinalIgnoreCase))
                {

                    var dt = dbExternal.GetDataTable(sql.MergedSql);  //výsledek pro select syntaxy se natahuje přes datatable
                    try
                    {
                        if (dt.Columns.Count > 1)
                        {
                            //tabulka s více sloupci -> TBODY
                            var sb = new System.Text.StringBuilder(); int xx = 0; int intCols = dt.Columns.Count;
                            //sb.AppendLine("<tbody>");
                            foreach (System.Data.DataRow dbRow in dt.Rows)
                            {
                                sb.AppendLine("<tr>");
                                for (int i = 0; i < intCols; i++)
                                {
                                    sb.AppendLine($"<td>{dbRow[i]}</td>");
                                }
                                sb.AppendLine("</tr>");
                                xx += 1;
                                if (xx > 1000) break;   //omezovač na 1000 záznamů
                            }
                            //sb.AppendLine("</tbody>");
                            ret = sb.ToString();
                        }
                        else
                        {
                            //skalár hodnota
                            System.Data.DataRow dbRow = dt.Rows[0];
                            if (dbRow[0] != DBNull.Value)
                            {
                                ret = dbRow[0].ToString();
                            }
                        }

                    }
                    catch
                    {
                        ret = "?";
                    }


                }
                else
                {
                    sql.MergedSql = "SELECT " + sql.MergedSql + " as Value";
                    try
                    {
                        ret = dbExternal.Load<BO.GetString>(sql.MergedSql).Value;
                    }
                    catch (Exception e)
                    {
                        ret = e.Message;
                    }
                }

                strHtmlText = strHtmlText.Replace(sql.OrigExpr, ret);
            }

            return strHtmlText;
        }


    }
}
