

namespace BO
{
    public class myQueryO23:baseQuery
    {
        private string entity { get; set; }
        private int recpid { get; set; }  
        public int o17id { get; set; }
        public int j02id { get; set; }
        public int p41id { get; set; }
        public int p28id { get; set; }
        public int o51id { get; set; }
        public int leindex { get; set; }   //nadřízená vertikální úrověň projektu #1 - #4
        public int lepid { get; set; }     //nadřízená vertikální úrověň projektu, hodnota p41id
        
        public bool? isremindernotifybefore { get; set; }

        public myQueryO23()
        {
            this.Prefix = "o23";
        }

        public override List<QRow> GetRows()
        {
            if (this.p31statequery > 0) this.Handle_p31StateQuery();
            if (!string.IsNullOrEmpty(this.p31tabquery)) this.Handle_p31TabQuery();
            if (this.o17id > 0)
            {
                AQ("o18x.o17ID=@o17id", "o17id", this.o17id);
            }
            if (this.j02id > 0)
            {
                this.entity = "j02";this.recpid = this.j02id;
            }
            if (this.p28id > 0)
            {
                this.entity = "p28"; this.recpid = this.p28id;
            }
            if (this.p41id > 0)
            {
                this.entity = "p41"; this.recpid = this.p41id;
            }

           

            if (this.o51id > 0)
            {
                AQ("a.o23ID IN (select o52RecordPid FROM o52TagBinding where o52RecordEntity='o23' AND o51ID=@o51id)", "o51id", this.o51id);
            }

            if (this.entity !=null && this.recpid>0)
            {
                AQ("a.o23ID IN (select xa.o23ID FROM o19DocTypeEntity_Binding xa INNER JOIN o20DocTypeEntity xb ON xa.o20ID=xb.o20ID WHERE xb.o20Entity=@entity AND xa.o19RecordPid=@recpid)", "entity", this.entity, "AND", null, null, "recpid", this.recpid);

            }


            if (this.leindex > 0 && this.lepid > 0)
            {
                AQ($"a.o23ID IN (select xa.o23ID FROM o19DocTypeEntity_Binding xa INNER JOIN o20DocTypeEntity xb ON xa.o20ID=xb.o20ID AND xb.o20Entity='{this.entity}' INNER JOIN p41Project xc ON xa.o19RecordPid=xc.p41ID WHERE xc.p41ID=@lepid OR xc.p41ID_P07Level{this.leindex}=@lepid)", "lepid", this.lepid);
            }

            if (!string.IsNullOrEmpty(_searchstring))
            {
                string s = "";
                if (_searchstring.Length == 1)
                {
                    //hledat pouze podle počátečních písmen
                    s = "a.o23Name Like @expr+'%' OR a.o23Code LIKE @expr+'%' OR a.o23FreeText01 LIKE @expr+'%'";

                }
                else
                {
                    //něco jako fulltext
                    s = "a.o23Name LIKE '%'+@expr+'%' COLLATE Latin1_General_CI_AI OR a.o23Code LIKE '%'+@expr+'%' OR a.o23FreeText01 LIKE '%'+@expr+'%' OR a.o23FreeText02 LIKE '%'+@expr+'%' OR a.o23FreeText04 LIKE '%'+@expr+'%'";
                   

                }
                AQ(s, "expr", _searchstring);

            }

            if (isremindernotifybefore == true)
            {
                string s = "a.o18ID IN (select o18ID FROM o16DocType_FieldSetting WHERE o16ReminderNotifyBefore=1) AND (a.o23FreeDate01 IS NOT NULL OR a.o23FreeDate02 IS NOT NULL OR a.o23FreeDate03 IS NOT NULL OR a.o23FreeDate04 IS NOT NULL OR a.o23FreeDate05 IS NOT NULL)";
                AQ(s, null, null);
            }

            if (this.MyRecordsDisponible)
            {
                Handle_MyDisponible();
            }

            return this.InhaleRows();

        }

        private void Handle_MyDisponible()
        {
            if (this.CurrentUser.IsAdmin || this.CurrentUser.TestPermission(PermValEnum.GR_o23_Reader) || this.CurrentUser.TestPermission(PermValEnum.GR_o23_Owner))
            {
                return; //přístup ke všem dokumentům
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("a.j02ID_Owner=@j02id_query");
            sb.Append(" OR EXISTS (SELECT 1 FROM x74DocRole_Permission");
            sb.Append(" WHERE (j02ID=@j02id_query OR x74IsAllUsers=1");

            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                sb.Append($" OR j11ID IN ({this.CurrentUser.j11IDs})");   //v portálu se nehraje na oprávnění pro všechny uživatele
            }
            sb.Append(")");
            sb.Append(" AND o23ID=a.o23ID");
            sb.Append(")");


            sb.Append(" OR EXISTS (SELECT 1 FROM x69EntityRole_Assign xa inner join x67EntityRole xb ON xa.x67ID=xb.x67ID");
            sb.Append(" WHERE xb.x67Entity='o23' and (xa.j02ID=@j02id_query OR xa.x69IsAllUsers=1");
            if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            {
                sb.Append(" OR xa.j11ID IN (" + this.CurrentUser.j11IDs + ")");
            }
            sb.Append(")");
            sb.Append(" AND ((xa.x69RecordEntity='o23' AND xa.x69RecordPid=a.o23ID) OR (xa.x69RecordEntity='o18' AND xa.x69RecordPid=a.o18ID))");
            sb.Append(")");

            AQ(sb.ToString(), "j02id_query", get_real_j02id_query());
            
            
            //string s = "a.j02ID_Owner=@j02id_query";
            //s += " OR EXISTS (SELECT 1 FROM x69EntityRole_Assign xa inner join x67EntityRole xb ON xa.x67ID=xb.x67ID";
            //s += " WHERE xb.x67Entity='o23' and (xa.j02ID=@j02id_query";


            //if (!string.IsNullOrEmpty(this.CurrentUser.j11IDs))
            //{
            //    s += " OR xa.j11ID IN (" + this.CurrentUser.j11IDs + ")";
            //}
            //s += ")";
            //s += " AND ((xa.x69RecordEntity='o23' AND xa.x69RecordPid=a.o23ID) OR (xa.x69RecordEntity='o18' AND xa.x69RecordPid=a.o18ID))";
            //s += ")";

            //AQ(s, "j02id_query", this.CurrentUser.pid);
        }
    }
}
