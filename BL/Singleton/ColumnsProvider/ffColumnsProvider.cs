

namespace BL
{
    public class ffColumnsProvider:ColumnsProviderBase
    {
        public ffColumnsProvider(BL.Factory f,string prefix)
        {
            if (prefix == "o23" || prefix==null)
            {
                Handle_o23UserFields(f);    //dokumenty přes o16DocType_FieldSetting
            }
           

            Handle_x67Roles(f,prefix);         //role v záznamech

            var lisX28 = f.x28EntityFieldBL.GetList(new BO.myQuery("x28")).OrderBy(p=>p.x28Entity);
            string strField = "";string strPrefix = "";
            foreach(var rec in lisX28)
            {
                strPrefix = rec.x28Entity.Substring(0, 3);
                if (rec.x28Flag == BO.x28FlagENUM.UserField)
                {
                    strField = rec.x28Field;
                    
                }
                else
                {
                    strField = "gridFreefield_" + rec.pid.ToString();           //musí tam být výraz 'Free', který se explicitně testuje v j72Columns           
                    rec.x28Name += "+";
                }


                this.EntityName = BO.Code.Entity.GetEntity(rec.x28Entity);

                this.CurrentFieldGroup = "Uživatelská pole";
                
                
                switch (rec.x24ID)
                {
                    case BO.x24IdENUM.tInteger:
                        oc=AF(strField, rec.x28Name, null, "num0", rec.x28IsGridTotals);
                       
                        break;
                    case BO.x24IdENUM.tDecimal:
                        oc=AF(strField, rec.x28Name, null, "num", rec.x28IsGridTotals);
                        break;
                    case BO.x24IdENUM.tDate:
                        oc=AFDATE(strField, rec.x28Name);
                        break;
                    case BO.x24IdENUM.tDateTime:
                        oc=AF(strField, rec.x28Name, null, "datetime");
                        break;
                    case BO.x24IdENUM.tBoolean:
                        oc=AFBOOL(strField, rec.x28Name);
                        break;
                    default:
                        oc=AF(strField, rec.x28Name);
                        break;
                }
                
                if (rec.x28Flag == BO.x28FlagENUM.GridField)        //čistě grid uživatelské pole na míru
                {
                    oc.SqlSyntax = rec.x28Grid_SqlSyntax;
                    oc.RelSqlInCol = rec.x28Grid_SqlFrom;
                }
                else
                {                    
                    oc.SqlSyntax = "ff_relname_" + strPrefix + "." + rec.x28Field;                   
                    oc.RelSqlInCol = "LEFT OUTER JOIN " + rec.SourceTableName + " ff_relname_" + strPrefix + " ON a." + strPrefix + "ID = ff_relname_" + strPrefix + "." + strPrefix + "ID";
                    
                }
                oc.SqlExplicitGroupBy = oc.SqlSyntax;



                if (strPrefix == "p41" && f.p07LevelsCount>1 && (prefix==null || prefix.Substring(0,2)=="le"))
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        if (f.getP07Level(i, true) != null)
                        {
                            this.EntityName = "le"+i.ToString();
                            var ocx = AF($"gridFreefield_le{i}_" + rec.pid.ToString(), oc.Header, oc.SqlSyntax, oc.FieldType, oc.IsShowTotals);
                            ocx.SqlSyntax = ocx.SqlSyntax.Replace("_p41", $"_le{i}");
                            ocx.RelSqlInCol = $"LEFT OUTER JOIN p41Project_FreeField ff_relname_le{i} ON a.p41ID = ff_relname_le{i}.p41ID";
                            ocx.SqlExplicitGroupBy = "a.p41ID";
                        }
                    }                    
                }

            }


            //Handle_Stitky(f);

            


        }

        
        private void Handle_Stitky_All(string entity,string prefix,string prefixdb)
        {            
            oc = AF($"AllFreeTags_{prefix}", "Štítky", $"{prefix}_o54.o54InlineHtml");
            oc.RelSqlInCol = $"LEFT OUTER JOIN o54TagBindingInline {prefix}_o54 ON a.{prefixdb}ID = {prefix}_o54.o54RecordPid AND {prefix}_o54.o54RecordEntity='{prefix}'";
            oc.Entity = entity;
            oc = AF($"AllFreeTags_{prefix}text", "Štítky text", $"{prefix}_o54.o54InlineText");
            oc.RelSqlInCol = $"LEFT OUTER JOIN o54TagBindingInline {prefix}_o54 ON a.{prefixdb}ID = {prefix}_o54.o54RecordPid AND {prefix}_o54.o54RecordEntity='{prefix}'";
            oc.Entity = entity;
        }
        private void Handle_Stitky(BL.Factory f)
        {
            //štítky
            this.CurrentFieldGroup = "Štítky";

            
            Handle_Stitky_All("o23Doc", "o23", "o23");
            Handle_Stitky_All("p31Worksheet", "p31", "p31");
            Handle_Stitky_All("p28Contact", "p28", "p28");
            Handle_Stitky_All("j02User", "j02", "j02");
            Handle_Stitky_All("p91Invoce", "p91", "p91");
            Handle_Stitky_All("p90Proforma", "p90", "p90");
           
            if (f.p07LevelsCount > 1)
            {
                for (int i = 1; i <= 5; i++)
                {
                    if (f.getP07Level(i, true) != null)
                    {
                        Handle_Stitky_All("le" + i.ToString(), "le" + i.ToString(), "p41");
                    }

                }
            }
            else
            {
                //Handle_Stitky_All("p41Project", "p41", "p41");
                //Handle_Stitky_All("le5", "le5", "p41");
            }
            
            
            var lisO53 = f.o53TagGroupBL.GetList(new BO.myQuery("o53")).Where(p => p.o53Field != null && p.o53Entities != null);
            foreach (var c in lisO53)
            {
                var entities = BO.Code.Bas.ConvertString2List(c.o53Entities);
                
                foreach (string oneentity in entities)
                {
                    
                    this.EntityName = BO.Code.Entity.GetEntity(oneentity);
                    string strTagPrefix = this.EntityName.Substring(0, 3);
                    
                    oc = AF("FreeTag"+c.o53Field, c.o53Name, strTagPrefix+"_o54."+c.o53Field);  //důležité je, aby obsahoval výraz "Free"
                    oc.RelSqlInCol = $"LEFT OUTER JOIN o54TagBindingInline {strTagPrefix}_o54 ON a.{strTagPrefix}ID = {strTagPrefix}_o54.o54RecordPid AND {strTagPrefix}_o54.o54RecordEntity='{oneentity}'";
                    if (oneentity == "p41" && f.p07LevelsCount>1)
                    {
                        for(int i = 1; i <= 5; i++)
                        {
                            if (f.getP07Level(i, true) != null)
                            {
                                strTagPrefix = "le" + i.ToString();
                                string strDbPrefix = "p41";
                                oc = AF("FreeTag" + c.o53Field, c.o53Name, strTagPrefix + "_o54." + c.o53Field);  //důležité je, aby obsahoval výraz "Free"
                                oc.Entity = "le" + i.ToString();
                                oc.RelSqlInCol = $"LEFT OUTER JOIN o54TagBindingInline {strTagPrefix}_o54 ON a.{strDbPrefix}ID = {strTagPrefix}_o54.o54RecordPid AND {strTagPrefix}_o54.o54RecordEntity='{oneentity}'";

                            }

                        }
                    }
                }



            }
        }

        private void Handle_x67Roles(BL.Factory f,string gridprefix)
        {            
            this.CurrentFieldGroup = "Role";

            var lisX67 = f.x67EntityRoleBL.GetList(new BO.myQuery("x67")).Where(p=>p.x67Entity !="j04" && p.x67Entity != "o17").OrderBy(p => p.x67Entity);
            foreach(var c in lisX67)
            {
                if (c.x67Entity == "p41")
                {
                    this.EntityName = "p41Project";

                }
                else
                {
                    this.EntityName = BO.Code.Entity.GetEntity(c.x67Entity);
                }

                //if (c.x67Entity == "p41" && (gridprefix == "p41" || gridprefix=="le5"))
                //{
                //    this.EntityName = "le5";

                //}
                //else
                //{
                //    this.EntityName = BO.Code.Entity.GetEntity(c.x67Entity);
                //}


                if (string.IsNullOrEmpty(this.EntityName))
                {
                    BO.Code.File.LogInfo("BO.Code.Entity missing: "+c.x67Entity);
                }
               
                
                
                oc = AF($"x71Free_{c.pid}", c.x67Name, $"x71Free{c.pid}.x71Value");
                oc.NotShowRelInHeader = true;
                oc.RelSqlInCol = $"LEFT OUTER JOIN x71EntityRole_Inline x71Free{c.pid} ON a.{c.x67Entity}ID=x71Free{c.pid}.x71RecordPid AND x71Free{c.pid}.x67ID={c.pid} AND x71Free{c.pid}.x71RecordEntity='{c.x67Entity}'";
                oc.SqlExplicitGroupBy = $"x71Free{c.pid}.x71Value";
            }
        }
        private void Handle_o23UserFields(BL.Factory f)
        {
            this.EntityName = "o23Doc";
            var lisO18 = f.o18DocTypeBL.GetList(new BO.myQueryO18());
            var lisO16 = f.o18DocTypeBL.GetList_o16();
            foreach (var recO18 in lisO18)
            {
                this.CurrentFieldGroup = recO18.o18Name;
                var qry = lisO16.Where(p => p.o18ID == recO18.pid && p.o16IsGridField==true).OrderBy(p => p.o16Ordinary);
                foreach(var c in qry)
                {
                    string strSQL = null;string strType = "string";
                    switch (c.FieldType)
                    {
                        case BO.x24IdENUM.tDate:
                            strSQL = "dbo.my_iif2_date(a.o18ID," + c.o18ID.ToString() + "," + c.o16Field + ",null)";
                            strType = "date";
                            break;
                        case BO.x24IdENUM.tDateTime:
                            strSQL = "dbo.my_iif2_date(a.o18ID," + c.o18ID.ToString() + "," + c.o16Field + ",null)";
                            strType = "datetime";                            
                            break;
                        case BO.x24IdENUM.tDecimal:
                            strSQL = "dbo.my_iif2_number(a.o18ID," + c.o18ID.ToString() + "," + c.o16Field + ",null)";
                            strType = "num";
                            break;
                        case BO.x24IdENUM.tInteger:
                            strSQL = "dbo.my_iif2_number(a.o18ID," + c.o18ID.ToString() + "," + c.o16Field + ",null)";
                            strType = "num0";
                            break;
                        case BO.x24IdENUM.tBoolean:
                            strSQL = "dbo.my_iif2_bit(a.o18ID," + c.o18ID.ToString() + "," + c.o16Field + ",null)";
                            strType = "bool";
                            break;
                        default:                            
                            strSQL = "dbo.my_iif2_string(a.o18ID," + c.o18ID.ToString() + "," + c.o16Field + ",null)";
                            strType = "string";
                            break;
                    }

                    //oc = AF("o16_Freefield" + c.o16ID.ToString(), c.o16Name, strSQL, strType);
                    oc = AF($"o16_Freefield_{c.o18ID}_{c.o16Field}", c.o16Name, strSQL, strType);
                    if (c.o16NameGrid != null)
                    {
                        oc.Header = c.o16NameGrid;
                    }
                }
            }

        }


        private void AA(BO.x28EntityField rec)
        {
            
        }
    }
}
