using BO;
using Dapper;


namespace BL
{
    public interface Ij72TheGridTemplateBL
    {
        public BO.j72TheGridTemplate Load(int j72id);
        public string LoadName(int j72id);
        public BO.TheGridState LoadState(int j72id, int j02id);
        public BO.TheGridState LoadState(string strEntity, int j02id, string strMasterEntity,string strRez);
        public int Save(BO.j72TheGridTemplate rec, List<BO.j73TheGridQuery> lisJ73, List<int> j04ids, List<int> j11ids);
        public int SaveState(BO.TheGridState rec, int j02id);
        public IEnumerable<BO.j72TheGridTemplate> GetList(string strEntity, int intJ02ID, string strMasterEntity);
        public IEnumerable<BO.j73TheGridQuery> GetList_j73(int j72id,string prefix,int j72id_druhy);
        public string getFiltrAlias(string prefix, BO.baseQuery mq);
        public string getDefaultPalletePreSaved(string entity, string master_entity, BO.baseQuery mq);  //vrací seznam výchozí palety sloupců pro grid: pouze pro významné entity
        public string getDefaultPalleteSearchbox(string entity);
        public void SetAsDefaultGlobalColumns(string prefix, string j72Columns, string master_entity = null);    //zapíše výchozí paletu sloupců do o58GlobalParam
        public void UpdateColumns2AllUsers(string entity, string j72Columns);   //přepíše nastavení sloupců všem uživatelům



    }

    class j72TheGridTemplateBL : BaseBL, Ij72TheGridTemplateBL
    {

        public j72TheGridTemplateBL(BL.Factory mother) : base(mother)
        {

        }

        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("j72"));
            sb(" FROM j72TheGridTemplate a LEFT OUTER JOIN j75TheGridState j75 ON a.j72ID=j75.j72ID");
            sb(strAppend);
            return sbret();
        }
        private string GetSQL2(int j72id,string strAppend = null)
        {
            sb("SELECT a.*,j75.j75ID,");
            sb("j75.j75SortDataField,j75.j75SortOrder,j75.j75PageSize,j75.j75CurrentPagerIndex,j75.j75Filter,j75.j75HeightPanel1,j75.j75ColumnsGridWidth,j75.j75ColumnsReportWidth,j75.j75OperatorFlag,j75.j75GroupDataField,j75GroupLastValue,j75IsGroupFirst,j75GroupFC,");

            sb(_db.GetSQL1_Ocas("j72"));
            if (j72id > 0)
            {
                sb(" FROM j72TheGridTemplate a LEFT OUTER JOIN (select * from j75TheGridState WHERE j72ID=@j72id and j02ID=@j02id) j75 ON a.j72ID=j75.j72ID");
                
            }
            else
            {
                sb(" FROM j72TheGridTemplate a LEFT OUTER JOIN (select * from j75TheGridState WHERE j02ID=@j02id) j75 ON a.j72ID=j75.j72ID");                
            }
            
            sb(strAppend);
            return sbret();
        }


        public BO.j72TheGridTemplate Load(int j72id)
        {
            return _db.Load<BO.j72TheGridTemplate>(GetSQL1(" WHERE a.j72ID=@j72id"), new { j72id = j72id });
        }
        public string LoadName(int j72id)
        {
            try
            {
                return _db.Load<BO.GetString>("select j72Name as Value from j72TheGridTemplate WHERE j72ID=@pid", new { pid = j72id }).Value;
            }
            catch
            {
                return null;
            }
            
        }
     
        
        public BO.TheGridState LoadState(int j72id,int j02id)
        {
            return _db.Load<BO.TheGridState>(GetSQL2(j72id," WHERE a.j72ID=@j72id"), new { j72id = j72id,j02id=j02id });
        }
        public BO.TheGridState LoadState(string strEntity, int j02id, string strMasterEntity,string strRez)
        {   //načtení systémového gridu: j72SystemFlag=1
            string strW = " WHERE a.j72SystemFlag=1 AND a.j72Entity=@entity AND a.j02ID=@j02id";
            var pars = new DynamicParameters();
            pars.Add("j02id", j02id,System.Data.DbType.Int32);
            pars.Add("entity", strEntity, System.Data.DbType.String);
            if (!String.IsNullOrEmpty(strMasterEntity))
            {
                pars.Add("masterentity", strMasterEntity, System.Data.DbType.String);
                strW += " AND a.j72MasterEntity=@masterentity";
            }
            else
            {
                strW += " AND a.j72MasterEntity IS NULL";
            }
            if (!String.IsNullOrEmpty(strRez))
            {
                pars.Add("rez", strRez, System.Data.DbType.String);
                strW += " AND a.j72Rez=@rez";
            }
            else
            {
                strW += " AND a.j72Rez IS NULL";                
            }

            return _db.Load<BO.TheGridState>(GetSQL2(0, strW), pars);

           

        }

        public int SaveState(BO.TheGridState rec,int j02id)
        {
            rec.pid = rec.j75ID;
            if (rec.j75PageSize < 0) rec.j75PageSize = 100;

            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.j75ID);
            p.AddInt("j72ID", rec.j72ID, true);
            p.AddInt("j02ID",j02id , true);
            p.AddInt("j75PageSize", rec.j75PageSize);
            p.AddInt("j75CurrentPagerIndex", rec.j75CurrentPagerIndex);
            p.AddInt("j75CurrentRecordPid", rec.j75CurrentRecordPid);
            p.AddString("j75SortDataField", rec.j75SortDataField);
            p.AddString("j75SortOrder", rec.j75SortOrder);
            p.AddString("j75Filter", rec.j75Filter);
            p.AddInt("j75HeightPanel1", rec.j75HeightPanel1);
            p.AddString("j75ColumnsReportWidth", rec.j75ColumnsReportWidth);
            p.AddString("j75ColumnsGridWidth", rec.j75ColumnsGridWidth);
            p.AddInt("j75OperatorFlag", rec.j75OperatorFlag);
            p.AddString("j75GroupDataField", rec.j75GroupDataField);
            p.AddString("j75GroupLastValue", rec.j75GroupLastValue);
            p.AddBool("j75IsGroupFirst", rec.j75IsGroupFirst);
            p.AddString("j75GroupFC", rec.j75GroupFC);

            int intJ75ID = _db.SaveRecord("j75TheGridState", p, rec,false,true);

            return intJ75ID;
        }


        public int Save(BO.j72TheGridTemplate rec, List<BO.j73TheGridQuery> lisJ73, List<int> j04ids, List<int> j11ids)
        {
            if (ValidateBeforeSave(rec, lisJ73) == false)
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.j72ID);
            p.AddString("j72Name", rec.j72Name);
            p.AddEnumInt("j72SystemFlag", rec.j72SystemFlag);
            p.AddInt("j02ID", rec.j02ID, true);

            p.AddString("j72Entity", rec.j72Entity);
            p.AddString("j72MasterEntity", rec.j72MasterEntity);
            p.AddString("j72Rez", rec.j72Rez);
            p.AddString("j72Columns", rec.j72Columns);

            p.AddBool("j72IsPublic", rec.j72IsPublic);
            p.AddBool("j72IsNoWrap", rec.j72IsNoWrap);
            
            p.AddInt("j72SelectableFlag", rec.j72SelectableFlag);
            p.AddBool("j72IsQueryNegation", rec.j72IsQueryNegation);

            if (lisJ73 != null)
            {
                p.AddBool("j72HashJ73Query", false);
            }

            int intJ72ID = _db.SaveRecord("j72TheGridTemplate", p, rec);

            if (j04ids != null && j11ids != null)
            {
                if (rec.pid > 0)
                {
                    _db.RunSql("if EXISTS(select j74ID FROM j74TheGridReceiver WHERE j72ID=@pid) DELETE FROM j74TheGridReceiver WHERE j72ID=@pid", new { pid = intJ72ID });
                }
                if (j04ids.Count > 0)
                {
                    _db.RunSql("INSERT INTO j74TheGridReceiver(j72ID,j04ID) SELECT @pid,j04ID FROM j04UserRole WHERE j04ID IN (" + string.Join(",", j04ids) + ")", new { pid = intJ72ID });
                }
                if (j11ids.Count > 0)
                {
                    _db.RunSql("INSERT INTO j74TheGridReceiver(j72ID,j04ID) SELECT @pid,j11ID FROM j11Team WHERE j11ID IN (" + string.Join(",", j11ids) + ")", new { pid = intJ72ID });
                }
            }
            if (lisJ73 != null)
            {
                if (rec.pid > 0)
                {
                    _db.RunSql("if EXISTS(select j73ID FROM j73TheGridQuery WHERE j72ID=@pid) DELETE FROM j73TheGridQuery WHERE j72ID=@pid", new { pid = intJ72ID });
                }
                foreach (var c in lisJ73)
                {
                    if (c.IsTempDeleted == true && c.j73ID > 0)
                    {
                        _db.RunSql("DELETE FROM j73TheGridQuery WHERE j73ID=@pid", new { pid = c.j73ID });
                    }
                    else
                    {
                        p = new DL.Params4Dapper();
                        p.AddInt("pid", c.j73ID, true);
                        p.AddInt("j72ID", intJ72ID, true);
                        p.AddString("j73Column", c.j73Column);
                        p.AddString("j73Operator", c.j73Operator);
                        p.AddInt("j73ComboValue", c.j73ComboValue);
                        p.AddInt("j73DatePeriodFlag", c.j73DatePeriodFlag);
                        if (c.j73DatePeriodFlag > 0)
                        {
                            c.j73Date1 = null; c.j73Date2 = null;
                        }
                        p.AddDateTime("j73Date1", c.j73Date1);
                        p.AddDateTime("j73Date2", c.j73Date2);
                        p.AddDouble("j73Num1", c.j73Num1);
                        p.AddDouble("j73Num2", c.j73Num2);
                        p.AddString("j73Value", c.j73Value);
                        p.AddString("j73ValueAlias", c.j73ValueAlias);
                        p.AddInt("j73Ordinal", c.j73Ordinal);
                        p.AddString("j73Op", c.j73Op);
                        p.AddString("j73BracketLeft", c.j73BracketLeft);
                        p.AddString("j73BracketRight", c.j73BracketRight);

                        p.AddString("j02AddValue", c.j02AddValue);
                        p.AddString("j02AddValueAlias", c.j02AddValueAlias);
                        p.AddString("j11AddValue", c.j11AddValue);
                        p.AddString("j11AddValueAlias", c.j11AddValueAlias);
                        p.AddBool("j73IsOfferPersonsAdd", c.j73IsOfferPersonsAdd);

                        _db.SaveRecord("j73TheGridQuery", p, c, false, true);
                    }

                }
                if (GetList_j73(intJ72ID,rec.j72Entity.Substring(0,3),0).Count() > 0)
                {
                    _db.RunSql("UPDATE j72TheGridTemplate set j72HashJ73Query=1 WHERE j72ID=@pid", new { pid = intJ72ID });
                }
            }

            return intJ72ID;
        }
        private bool ValidateBeforeSave(BO.j72TheGridTemplate rec, List<BO.j73TheGridQuery> lisJ73)
        {
            if (rec.j72SystemFlag !=BO.j72SystemFlagEnum.QueryOnly && string.IsNullOrEmpty(rec.j72Columns) == true)
            {
                this.AddMessage("Tabulka musí obsahovat minimálně jeden sloupec."); return false;
            }
           
            if (rec.j72SystemFlag == BO.j72SystemFlagEnum.QueryOnly && (lisJ73==null || lisJ73.Where(p=>!p.IsTempDeleted).Count()==0))
            {
                this.AddMessage("Filtr musí obsahovat minimálně jednu filtrovací podmínku."); return false;
            }
            if (rec.j72SystemFlag == BO.j72SystemFlagEnum.QueryOnly && string.IsNullOrEmpty(rec.j72Name))
            {
                this.AddMessage("Chybí název filtru."); return false;
            }

            if (lisJ73 != null)
            {
                int x = 0; string lb = ""; string rb = "";
                foreach (var c in lisJ73.Where(p => p.IsTempDeleted == false))
                {
                    x += 1;
                    if (c.j73BracketLeft != null)
                    {
                        lb += c.j73BracketLeft;
                    }
                    if (c.j73BracketRight != null)
                    {
                        rb += c.j73BracketRight;
                    }

                    switch (c.FieldType)
                    {
                        case "date":
                            if (c.j73Operator == "INTERVAL" && c.j73Date1 == null && c.j73Date2 == null && c.j73DatePeriodFlag == 0)
                            {
                                this.AddMessageTranslated(string.Format(_mother.tra("Filtr řádek [{0}] musí mít alespoň jedno vyplněné datum nebo pojmenované období."), x)); return false;
                            }
                            break;
                        case "string":
                            if (string.IsNullOrEmpty(c.j73Value) == true && (c.j73Operator == "CONTAINS" || c.j73Operator == "STARTS" || c.j73Operator == "EQUAL" || c.j73Operator == "NOT-EQUAL"))
                            {
                                this.AddMessageTranslated(string.Format(_mother.tra("Filtr řádek [{0}] obsahuje nevyplněnou hodnotu."), x)); return false;
                            }
                            break;
                        case "combo":
                            if (c.j73ComboValue == 0 && (c.j73Operator == "EQUAL" || c.j73Operator == "NOT-EQUAL"))
                            {
                                this.AddMessageTranslated(string.Format(_mother.tra("Filtr řádek [{0}] obsahuje nevyplněnou hodnotu."), x)); return false;
                            }
                            break;
                        case "multi":
                            if (string.IsNullOrEmpty(c.j73Value) == true && (c.j73Operator == "EQUAL" || c.j73Operator == "NOT-EQUAL"))
                            {
                                this.AddMessageTranslated(string.Format(_mother.tra("Filtr řádek [{0}] obsahuje nevyplněnou hodnotu."), x)); return false;
                            }
                            break;
                    }
                }
                if (lb.Length != rb.Length)
                {
                    this.AddMessage(string.Format("Ve filtrovací podmínce nejsou správně závorky.", x)); return false;
                }
            }


            return true;
        }


        public IEnumerable<BO.j72TheGridTemplate> GetList(string strEntity, int intJ02ID, string strMasterEntity)
        {
            string s = $"SELECT a.*,{_db.GetSQL1_Ocas("j72")} FROM j72TheGridTemplate a";
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                s = $"{s} INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID";
            }
            var p = new Dapper.DynamicParameters();
            p.Add("j02id", intJ02ID);
            p.Add("entity", strEntity);
            p.Add("j04id", _mother.CurrentUser.j04ID);

            if (string.IsNullOrEmpty(strMasterEntity))
            {
                s = $"{s} WHERE a.j72Entity=@entity AND a.j72MasterEntity IS NULL";
            }
            else
            {
                if (strMasterEntity == "recpage")
                {
                    s = $"{s} WHERE a.j72Entity=@entity AND (a.j72MasterEntity IS NULL OR a.j72MasterEntity='recpage')";   //recpage nabízí úzký i široký přehled
                }
                else
                {
                    s = $"{s} WHERE a.j72Entity=@entity AND a.j72MasterEntity = @masterentity";
                    p.Add("masterentity", strMasterEntity);
                }
                
                
            }
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                s = $"{s} AND x67x.x01ID=@x01id";
                p.Add("x01id", _mother.CurrentUser.x01ID);
            }
            s = $"{s} AND (a.j02ID=@j02id OR a.j72IsPublic=1 OR a.j72ID IN (select j72ID FROM j74TheGridReceiver WHERE j04ID=@j04id))";
            


            return _db.GetList<BO.j72TheGridTemplate>($"{s} ORDER BY a.j72SystemFlag DESC", p);
        }



        public IEnumerable<BO.j73TheGridQuery> GetList_j73(int j72id,string prefix,int j72id_druhy)
        {
            string s = "SELECT a.*,b.j72IsQueryNegation FROM j73TheGridQuery a INNER JOIN j72TheGridTemplate b ON a.j72ID=b.j72ID WHERE a.j72ID=@j72id ORDER BY a.j73Ordinal";
            if (j72id_druhy > 0)
            {
                s = "SELECT a.*,b.j72IsQueryNegation FROM j73TheGridQuery a INNER JOIN j72TheGridTemplate b ON a.j72ID=b.j72ID WHERE a.j72ID IN (@j72id,@j72id_druhy) ORDER BY a.j72ID,a.j73Ordinal";
            }

            var lis = _db.GetList<BO.j73TheGridQuery>(s, new { j72id =j72id, j72id_druhy= j72id_druhy });
            if (lis.Count() > 0)
            {
                var lisQueryFields = new BL.TheQueryFieldProvider(prefix).getPallete();
                foreach (var c in lis.Where(p => p.j73Column != null))
                {
                    if (lisQueryFields.Where(p => p.Field == c.j73Column).Count() > 0)
                    {
                        var cc = lisQueryFields.Where(p => p.Field == c.j73Column).First();
                        c.FieldType = cc.FieldType;
                        c.FieldEntity = cc.SourceEntity;
                        c.FieldSqlSyntax = cc.FieldSqlSyntax;
                        c.SqlWrapper = cc.SqlWrapper;
                        c.MasterPrefix = cc.MasterPrefix;
                        c.MasterPid = cc.MasterPid;
                        c.MyQueryInline = cc.MyQueryInline;
                        c.j73IsOfferPersonsAdd = cc.IsOfferPersonsAdd;
                    }
                }
            }
            return lis;
        }

        public string getFiltrAlias(string prefix, BO.baseQuery mq)
        {
            if (mq.lisJ73.Count() == 0) return "";
            var lisFields = new BL.TheQueryFieldProvider(prefix).getPallete();

            var lis = new List<string>();

            foreach (var c in mq.lisJ73)
            {
                string ss = "";
                BO.TheQueryField cField = null;
                if (c.j73BracketLeft != null)
                {
                    ss += "(";
                }
                if (c.j73Op == "OR")
                {
                    ss += " OR ";
                }
                if (lisFields.Where(p => p.Field == c.j73Column).Count() > 0)
                {
                    cField = lisFields.Where(p => p.Field == c.j73Column).First();
                    string s = cField.Header;
                    if (_mother.CurrentUser.j02LangIndex > 0)
                    {
                        s = _mother.tra(s);
                    }
                    ss = "[" + s + "] ";
                }
                switch (c.j73Operator)
                {
                    case "EQUAL":
                        ss += "=";
                        break;
                    case "NOT-ISNULL":
                        ss += _mother.tra("Není prázdné");
                        break;
                    case "ISNULL":
                        ss += _mother.tra("Je prázdné");
                        break;
                    case "INTERVAL":
                        ss += _mother.tra("Je interval");
                        break;
                    case "GREATERZERO":
                        ss += _mother.tra("Je větší než nula");
                        break;
                    case "ISNULLORZERO":
                        ss += _mother.tra("Je nula nebo prázdné");
                        break;
                    case "NOT-EQUAL":
                        ss += _mother.tra("Není rovno");
                        break;
                    case "CONTAINS":
                        lis.Add(_mother.tra("Obsahuje"));
                        break;
                    case "STARTS":
                        ss += _mother.tra("Začíná na");
                        break;
                    default:
                        break;
                }
                if (c.j73ValueAlias != null)
                {
                    ss += c.j73ValueAlias;
                }
                else
                {
                    ss += c.j73Value;
                }
                if (c.j02AddValueAlias != null)
                {
                    ss += $" [{c.j02AddValueAlias}]";
                }
                if (c.j11AddValueAlias != null)
                {
                    ss += $" [{c.j11AddValueAlias}]";
                }
                if (c.j73DatePeriodFlag > 0)
                {
                    var cPeriods = new BO.Code.Cls.ThePeriodProviderSupport();
                    var lisPeriods = cPeriods.GetPallete();

                    var d1 = lisPeriods.Where(p => p.pid == c.j73DatePeriodFlag).First().d1;
                    var d2 = Convert.ToDateTime(lisPeriods.Where(p => p.pid == c.j73DatePeriodFlag).First().d2).AddDays(1).AddMinutes(-1);
                    ss += ": " + BO.Code.Bas.ObjectDate2String(d1, "dd.MM.yyyy") + " - " + BO.Code.Bas.ObjectDate2String(d2, "dd.MM.yyyy");
                }

                if (c.j73BracketRight != null)
                {
                    ss += ")";
                }
                lis.Add(ss);
            }

            return string.Join("; ", lis);
        }

        public void SetAsDefaultGlobalColumns(string prefix,string j72Columns, string master_entity=null)
        {
            if (string.IsNullOrEmpty(master_entity))
            {
                _db.RunSql("DELETE FROM o58GlobalParam WHERE o58Entity=@prefix AND o58MasterEntity IS NULL AND o58Key='j72Columns'", new { prefix = prefix });
            }
            else
            {
                _db.RunSql("DELETE FROM o58GlobalParam WHERE o58Entity=@prefix AND o58MasterEntity=@me AND o58Key='j72Columns'", new { prefix = prefix,me= master_entity });
            }

            if (string.IsNullOrEmpty(j72Columns))
            {                
                return;
            }
            
            _db.RunSql("declare @pid int; select @pid=max(o58ID)+1 FROM o58GlobalParam; INSERT INTO o58GlobalParam(o58ID,o58Name,o58Key,o58Entity,o58UserInsert,o58UserUpdate,o58DefaultValue,o58DateUpdate,o58MasterEntity) VALUES(case when isnull(@pid,0)<1000 then 1000 else @pid end,'Sloupce','j72Columns',@prefix,@login,@login,@cols,GETDATE(),@me)", new { prefix = prefix,login=_mother.CurrentUser.j02Login,cols=j72Columns,me= master_entity });

        }
        public void UpdateColumns2AllUsers(string entity, string j72Columns)
        {            
            if (string.IsNullOrEmpty(j72Columns) || string.IsNullOrEmpty(entity))
            {
                return;
            }
            
            _db.RunSql("UPDATE j72TheGridTemplate SET j72Columns=@cols WHERE j72Entity=@entity AND j72SystemFlag=1", new { cols = j72Columns, entity = entity });

        }
        public string getDefaultPalleteSearchbox(string entity)
        {            
            string s = null;
           
            switch (entity.Substring(0,3))
            {
                case "j02":
                    s = "a__j02User__fullname_desc,a__j02User__PoziceUzivatele";
                    break;
                case "p31":
                    s = "a__p31Worksheet__p31Date,p31_j02__j02User__fullname_desc,p31_p41__p41Project__KlientProjektu,p31_p41__p41Project__NazevProjektu,p31_p32__p32Activity__p32Name,a__p31Worksheet__p31Hours_Orig,a__p31Worksheet__p31Text";
                    break;
                case "p28":
                    s = "a__p28Contact__p28Name,a__p28Contact__p28RegID,a__p28Contact__Adresa1";
                    break;
                case "p32":
                    s = "a__p32Activity__p32Name,p32_p34__p34ActivityGroup__p34Name,p32_p95__p95InvoiceRow__p95Name,p32_p38__p38ActivityTag__p38Name";
                    break;
                case "le1":
                    s = "a__le1__NazevProjektu,a__le1__TypProjektu,a__le1__KlientProjektu";
                    break;
                case "le2":
                    s = "a__le2__NazevProjektu,a__le2__TypProjektu,a__le2__KlientProjektu";
                    break;
                case "le3":
                    s = "a__le3__NazevProjektu,a__le3__TypProjektu,a__le3__KlientProjektu";
                    break;
                case "le4":
                    s = "a__le4__NazevProjektu,a__le4__TypProjektu,a__le4__KlientProjektu";
                    break;
                case "le5":                    
                    s = "a__le5__KlientProjektu,a__le5__NazevNeboZkraceny,a__le5__p41Code,a__le5__TypProjektu";
                    break;
                case "p41":
                    s = "a__p41Project__KlientProjektu,a__p41Project__NazevNeboZkraceny,a__p41Project__p41Code,a__p41Project__TypProjektu";
                    break;

                case "p90":
                    s = "a__p90Proforma__p90Code,a__p90Proforma__p90Date,p90_p28__p28Contact__p28Name,a__p90Proforma__p90Amount,p90_j27__j27Currency__j27Code,a__p90Proforma__p90Amount_Billed,a__p90Proforma__p90DateMaturity,a__p90Proforma__p91codes,a__p90Proforma__ChybiSparovat,a__p90Proforma__p90Text1";
                    break;
                case "p91":
                    s = "a__p91Invoice__p91Code,a__p91Invoice__p91Client,a__p91Invoice__p91DateSupply,a__p91Invoice__p91Amount_WithoutVat,a__p91Invoice__p91Amount_Debt,a__p91Invoice__p91DateMaturity,a__p91Invoice__p91Text1";
                    break;
                case "o23":
                    s = "a__o23Doc__o23Name,a__o23Doc__DocType,o23_p28__p28Contact__p28Name,o23_p41__p41Project__p41Name,a__o23Doc__AllFreeTags_o23,a__o23Doc__DateInsert_o23Doc,a__o23Doc__UserInsert_o23Doc";
                    break;
                case "p56":
                    s = "a__p56Task__p56Name,a__p56Task__AktualniStav,a__p56Task__p56PlanUntil,a__p56Task__p56Code";
                    break;
                
                default:
                    return null;
            }

       

            List<string> lis = new List<string>();
            var arr = s.Split(",");
            for (int i = 0; i < arr.Count(); i++)   //pokud v definici sloupce chybí určení entity, pak doplnit:
            {
                if (arr[i].Contains("__"))
                {
                    lis.Add(arr[i]);
                }
                else
                {
                    lis.Add("a__" + entity + "__" + arr[i]);
                }
            }



            s = string.Join(",", lis);

            return s;

        }
        public string getDefaultPalletePreSaved(string entity, string master_entity, BO.baseQuery mq)  //vrací seznam výchozí palety sloupců pro grid: pouze pro významné entity
        {
            string s = null;
            
            BO.GetString ss = null;
            if (master_entity == null)
            {
                ss= _db.Load<GetString>("SELECT o58DefaultValue as Value FROM o58GlobalParam WHERE o58Entity=@prefix AND o58MasterEntity IS NULL AND o58Key='j72Columns'", new { prefix = mq.Prefix });
            }
            else
            {
                ss = _db.Load<GetString>("SELECT o58DefaultValue as Value FROM o58GlobalParam WHERE o58Entity=@prefix AND o58MasterEntity=@me AND o58Key='j72Columns'", new { prefix = mq.Prefix, me= master_entity });
            }
            if (ss != null)
            {
                return ss.Value;    //globální nastavení pro výchozí grid sloupce
            }

            string strMasterPrefixDb = null;
            if (master_entity != null)
            {
                strMasterPrefixDb = master_entity.Substring(0, 3);
            }

            switch (mq.Prefix)
            {
                case "j02":
                    s = "a__j02User__fullname_desc,a__j02User__PoziceUzivatele,a__j02User__AplikacniRoleUzivatele,j02_nevyuctovano__com_nevyuctovano__Hodiny,j02_nevyuctovano__com_nevyuctovano__Hodiny_Fa,a__j02User__j02Ping_TimeStamp";
                    if (master_entity == "recpage")
                    {
                        s = "a__j02User__fullname_desc,a__j02User__PoziceUzivatele";
                    }
                    if (master_entity == "approve_aio")
                    {
                        s = "a__j02User__fullname_desc,a__j02User__PoziceUzivatele,j02_nevyuctovano__com_nevyuctovano__Hodiny_Rozpr,j02_nevyuctovano__com_nevyuctovano__Hodiny,j02_nevyuctovano__com_nevyuctovano__Honorar,j02_nevyuctovano__com_nevyuctovano__Hodiny_Fa,j02_vykazano__com_vykazano__Hodiny,j02_vykazano__com_vykazano__PosledniUkon_Kdy";
                    }
                    break;
                case "p31":
                    bool bolAllowRates = _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_AllowRates);
                    s = "a__p31Worksheet__p31Date,p31_j02__j02User__fullname_desc,p31_p41__p41Project__KlientProjektu,p31_p41__p41Project__NazevProjektu,p31_p32__p32Activity__p32Name,a__p31Worksheet__p31Hours_Orig";
                    
                    bool bolResitOcas = true;
                    
                    switch (strMasterPrefixDb)
                    {
                        case "p91":
                            s = "p31Date,p31_j02__j02User__fullname_desc,p31_p41__p41Project__NazevProjektu,p31_p32__p32Activity__p32Name,p31Hours_Invoiced";
                            if (bolAllowRates)
                            {
                                s += ",p31Rate_Billing_Invoiced,p31Amount_WithoutVat_Invoiced,p31VatRate_Invoiced";
                            }
                            s += ",p31Text";
                            bolResitOcas = false;
                            break;
                        case "app": //schvalování
                            s = "a__p31Worksheet__p31Date,p31_j02__j02User__fullname_desc,p31_p41_client__p28Contact__p28Name,p31_p41__p41Project__NazevProjektu,p31_p32__p32Activity__p32Name,a__p31Worksheet__p31Hours_Approved_Billing,a__p31Worksheet__p31Rate_Billing_Approved,a__p31Worksheet__p31Amount_WithoutVat_Approved,a__p31Worksheet__p31Hours_Orig";
                            if (bolAllowRates)
                            {
                                s += ",a__p31Worksheet__p31Rate_Billing_Orig,a__p31Worksheet__p31Amount_WithoutVat_Orig";
                            }
                            s += ",a__p31Worksheet__p31Text,a__p31Worksheet__SchvaleneHodinyVPausalu";
                            bolResitOcas = false;
                            break;
                        case "p28": //nezobrazovat v přehledu duplicitně název klienta
                            s = "a__p31Worksheet__p31Date,p31_j02__j02User__fullname_desc,p31_p41__p41Project__NazevProjektu,p31_p32__p32Activity__p32Name,a__p31Worksheet__p31Hours_Orig";                           
                            break;
                        case "le5":                        
                        case "p41":    //nezobrazovat v přehledu duplicitně název klienta a projekt
                        case "p56":    //nezobrazovat v přehledu duplicitně název klienta a projekt
                        case "le4":
                        case "le3":
                            s = "a__p31Worksheet__p31Date,p31_j02__j02User__fullname_desc,p31_p32__p32Activity__p32Name,a__p31Worksheet__p31Hours_Orig";                            
                            break;
                        case "j02":  //nezobrazovat v přehledu duplicitně název uživatele
                            s = "a__p31Worksheet__p31Date,p31_p41__p41Project__KlientProjektu,p31_p41__p41Project__p41Name,p31_p32__p32Activity__p32Name,a__p31Worksheet__p31Hours_Orig";                            
                            break;
                        case "j06":
                            s = "j06_j02__j02User__fullname_desc,a__j06UserHistory__Pozice,a__j06UserHistory__Fond,a__j06UserHistory__j06ValidFrom,a__j06UserHistory__j06ValidUntil";
                            break;
                        case "j92":
                            s = "j92_j02__j02User__fullname_desc,a__j92PingLog__j92Date,a__j92PingLog__j92BrowserFamily,a__j92PingLog__j92BrowserOS,a__j92PingLog__j92BrowserDeviceType,a__j92PingLog__j92BrowserAvailWidth,a__j92PingLog__j92BrowserAvailHeight,a__j92PingLog__j92RequestURL";
                            break;

                    }

                    if (bolResitOcas && bolAllowRates)
                    {
                        s += ",a__p31Worksheet__p31Rate_Billing_Orig,a__p31Worksheet__p31Amount_WithoutVat_Orig,a__p31Worksheet__j27Code_Billing_Orig";
                    }
                    if (bolResitOcas)
                    {
                        s += ",a__p31Worksheet__p31Text";
                    }

                    
                    break;
                case "p32":
                    s = "a__p32Activity__p32Name,a__p32Activity__p32IsBillable,a__p32Activity__p32Ordinary,p32_p34__p34ActivityGroup__p34Name,p32_p95__p95InvoiceRow__p95Name";
                    break;
                
                case "p28":
                    s = "a__p28Contact__p28Name,a__p28Contact__TypKontaktu,a__p28Contact__Adresa1,a__p28Contact__p28RegID,a__p28Contact__p28VatID,p28_nevyuctovano__com_nevyuctovano__Hodiny,p28_nevyuctovano__com_nevyuctovano__BezDph,p28_nevyuctovano__com_nevyuctovano__PosledniUkon_Kdy";
                    if (master_entity == "recpage")
                    {
                        s = "a__p28Contact__p28Name,a__p28Contact__Adresa1";
                    }
                    if (master_entity == "approve_aio")
                    {
                        s = "a__p28Contact__p28Name,p28_nevyuctovano__com_nevyuctovano__Hodiny_Rozpr,p28_nevyuctovano__com_nevyuctovano__Hodiny,p28_nevyuctovano__com_nevyuctovano__Honorar,p28_nevyuctovano__com_nevyuctovano__Honorar_EUR,p28_nevyuctovano__com_nevyuctovano__Vydaje,p28_nevyuctovano__com_nevyuctovano__Pausaly,p28_nevyuctovano__com_nevyuctovano__BezDph,p28_nevyuctovano__com_nevyuctovano__PosledniUkon_Kdy,p28_invoice__com_vyuctovano__PosledniFaktura_Kdy";
                    }
                    break;
                case "p41":
                    s = "a__p41Project__KlientProjektu,a__p41Project__NazevProjektu,a__p41Project__TypProjektu,a__p41Project__p41Code,p41_nevyuctovano__com_nevyuctovano__Hodiny,p41_nevyuctovano__com_nevyuctovano__Honorar,p41_nevyuctovano__com_nevyuctovano__BezDph,p41_nevyuctovano__com_nevyuctovano__Pausaly,p41_nevyuctovano__com_nevyuctovano__Vydaje";

                    switch (master_entity)
                    {
                        case "recpage":
                            s = "a__p41Project__KlientProjektu,a__p41Project__NazevProjektu";
                            break;
                        case "p28Contact":
                            s = "a__p41Project__NazevProjektu,a__p41Project__p41Code,p41_vykazano__com_vykazano__Hodiny,p41_nevyuctovano__com_nevyuctovano__Hodiny,p41_nevyuctovano__com_nevyuctovano__Honorar,p41_nevyuctovano__com_nevyuctovano__BezDph,p41_nevyuctovano__com_nevyuctovano__Pausaly,p41_nevyuctovano__com_nevyuctovano__Vydaje";
                            break;
                        case "p91Invoice":
                            s = "a__p41Project__KlientProjektu,a__p41Project__NazevProjektu,a__p41Project__p41Code,p41_vyuctovano__com_vyuctovano__Hodiny,p41_vyuctovano__com_vyuctovano__Vydaje,p41_vyuctovano__com_vyuctovano__Pausaly,p41_vyuctovano__com_vyuctovano__Hodiny_Odpis,p41_vyuctovano__com_vyuctovano__Hodiny_Pausal";
                            break;

                    }
                    break;
                case "le3":
                    s = "a__le3__NazevProjektu,a__le3__TypProjektu,a__le3__KlientProjektu";
                    if (master_entity == "recpage")
                    {
                        s = "a__le3__NazevProjektu,a__le3__KlientProjektu";
                    }
                    break;
                case "le4":
                    s = "a__le4__NazevProjektu,a__le4__TypProjektu,a__le4__KlientProjektu";
                    if (master_entity == "recpage")
                    {
                        s = "a__le4__NazevProjektu,a__le4__KlientProjektu";
                    }
                    break;
                case "le5":
                    s = "a__le5__KlientProjektu,a__le5__NazevProjektu,a__le5__TypProjektu,a__le5__p41Code,le5_nevyuctovano__com_nevyuctovano__Hodiny,le5_nevyuctovano__com_nevyuctovano__Honorar,le5_nevyuctovano__com_nevyuctovano__BezDph,le5_nevyuctovano__com_nevyuctovano__Pausaly,le5_nevyuctovano__com_nevyuctovano__Vydaje";
                  
                    switch (master_entity)
                    {
                        case "recpage":
                            s = "a__le5__KlientProjektu,a__le5__NazevProjektu";
                            break;
                        case "p28Contact":
                            s = "a__le5__NazevProjektu,a__le5__p41Code,le5_vykazano__com_vykazano__Hodiny,le5_nevyuctovano__com_nevyuctovano__Hodiny,le5_nevyuctovano__com_nevyuctovano__Honorar,le5_nevyuctovano__com_nevyuctovano__BezDph,le5_nevyuctovano__com_nevyuctovano__Pausaly,le5_nevyuctovano__com_nevyuctovano__Vydaje";
                            break;
                        case "p91Invoice":
                            s = "a__le5__KlientProjektu,a__le5__NazevProjektu,a__le5__p41Code";
                            break;

                    }
                    break;
                case "p56":
                    s = "a__p56Task__p56Name,a__p56Task__AktualniStav,p56_p41__p41Project__KlientPlusProjekt,a__p56Task__ToDoList,a__p56Task__p56PlanFrom,a__p56Task__p56PlanUntil,a__p56Task__p56Plan_Hours,p56_vykazano__com_vykazano__Hodiny";
                    switch (master_entity)
                    {
                        case "recpage":
                            s = "a__p56Task__p56Name,p56_p41__p41Project__KlientPlusProjekt";
                            break;
                        case "approve_aio":
                            s = "p56_p41__p41Project__KlientPlusProjekt,a__p56Task__p56Name,a__p56Task__AktualniStav,a__p56Task__p56PlanFrom,a__p56Task__p56PlanUntil,a__p56Task__p56Plan_Hours,p56_vykazano__com_vykazano__Hodiny,p56_nevyuctovano__com_nevyuctovano__Hodiny,p56_nevyuctovano__com_nevyuctovano__BezDph";
                            break;
                        case "le5":
                        case "p41Project":
                            s = "a__p56Task__p56Name,a__p56Task__AktualniStav,a__p56Task__ToDoList,a__p56Task__p56PlanFrom,a__p56Task__p56PlanUntil,a__p56Task__p56Plan_Hours,p56_vykazano__com_vykazano__Hodiny";
                            break;

                    }
                    
                    break;
                case "p90":
                    s = "a__p90Proforma__p90Code,a__p90Proforma__Client,a__p90Proforma__p90Date,a__p90Proforma__p90Amount,a__p90Proforma__p90Amount_WithoutVat,a__p90Proforma__p90Amount_Billed,a__p90Proforma__p90DateMaturity,a__p90Proforma__p91codes,a__p90Proforma__ChybiSparovat,a__p90Proforma__p90Text1,a__p90Proforma__JizSparovano";
                    switch (master_entity)
                    {
                        case "recpage":
                            s = "a__p90Proforma__p90Code,p90_p28__p28Contact__p28Name";
                            break;
                        case "p28Contact":
                            s = "a__p90Proforma__p90Code,a__p90Proforma__p90Date,a__p90Proforma__p90Amount,a__p90Proforma__p90Amount_WithoutVat,a__p90Proforma__p90Amount_Billed,a__p90Proforma__p90DateMaturity,a__p90Proforma__p91codes,a__p90Proforma__ChybiSparovat,a__p90Proforma__p90Text1,a__p90Proforma__JizSparovano";
                            break;
                    }
                  
                    break;

                case "p91":
                    s = "a__p91Invoice__p91Code,a__p91Invoice__p91Client,a__p91Invoice__p91DateSupply,a__p91Invoice__p91Amount_WithoutVat,a__p91Invoice__j27Code,a__p91Invoice__DluhKratKurz,a__p91Invoice__p91DateMaturity,p91_vyuctovano__p91_vyuctovano__Hodiny,a__p91Invoice__VomKdyOdeslano";
                    switch (master_entity)
                    {
                        case "recpage":
                            s = "a__p91Invoice__p91Code,a__p91Invoice__p91Client";
                            break;
                        case "p28Contact":
                            s = "a__p91Invoice__p91Code,a__p91Invoice__p91DateSupply,a__p91Invoice__p91Amount_WithoutVat,a__p91Invoice__j27Code,a__p91Invoice__DluhKratKurz,a__p91Invoice__p91DateMaturity,p91_vyuctovano__p91_vyuctovano__Hodiny,a__p91Invoice__VomKdyOdeslano";
                            break;
                        
                    }
                   
                    break;
                case "p84":
                    s = "a__p84Upominka__CisloFaktury,p84_p91__p91Invoice__p91Client,p84_p91__p91Invoice__p91Client_RegID,a__p84Upominka__p84Name,a__p84Upominka__p84Date,p84_p91__p91Invoice__p91Amount_Debt,p84_p91__p91Invoice__j27Code,p84_p91__p91Invoice__p91DateMaturity,p84_p91__p91Invoice__DnuPoSplatnosti,p84_p91__p91Invoice__p91DateSupply";
                    switch (master_entity)
                    {
                        case "recpage":
                            s = "a__p84Upominka__CisloFaktury,p84_p91__p91Invoice__p91Client";
                            break;
                        

                    }
                    break;
                case "o23":
                    s = "a__o23Doc__o23Name,a__o23Doc__DocType,a__o23Doc__DateInsert_o23Doc,a__o23Doc__UserInsert_o23Doc";
                    if (master_entity == "recpage")
                    {
                        s = "a__o23Doc__o23Name,a__o23Doc__DocType";
                    }
                    break;
                case "o43":
                    s = "a__o43Inbox__o43DateReceived,a__o43Inbox__o43Subject,a__o43Inbox__Sender,a__o43Inbox__o43To,a__o43Inbox__Velikost,a__o43Inbox__o43ImapFolder,a__o43Inbox__Inserted";
                    break;
                case "x31":
                    s = "a__x31Report__x31Name,a__x31Report__Kontext,a__x31Report__x31Code,a__x31Report__RepFormat,a__x31Report__Kategorie,a__x31Report__Soubor";
                    break;
                case "p58":
                    s = "a__p58TaskRecurrence__p58Name,a__p58TaskRecurrence__p58RecurrenceType,p58_p41__p41Project__KlientProjektu,p58_p41__p41Project__p41Name,a__p58TaskRecurrence__p58BaseDateStart,a__p58TaskRecurrence__p58BaseDateEnd,a__p58TaskRecurrence__p58IsPlanUntil,a__p58TaskRecurrence__p58IsPlanFrom";
                    break;
                case "p40":
                    s = "p40_p41__p41Project__KlientProjektu,p40_p41__p41Project__p41Name,a__p40WorkSheet_Recurrence__p40RecurrenceType,a__p40WorkSheet_Recurrence__NejblizsiGenerovani,a__p40WorkSheet_Recurrence__p39Count,a__p40WorkSheet_Recurrence__p40Text,a__p40WorkSheet_Recurrence__p40Value,a__p40WorkSheet_Recurrence__Mena,a__p40WorkSheet_Recurrence__p40FirstSupplyDate,a__p40WorkSheet_Recurrence__p40LastSupplyDate";                    
                    if (master_entity == "p28Contact")
                    {
                        s = "p40_p41__p41Project__p41Name,a__p40WorkSheet_Recurrence__p40RecurrenceType,a__p40WorkSheet_Recurrence__NejblizsiGenerovani,a__p40WorkSheet_Recurrence__p39Count,a__p40WorkSheet_Recurrence__p40Text,a__p40WorkSheet_Recurrence__p40Value,a__p40WorkSheet_Recurrence__Mena,a__p40WorkSheet_Recurrence__p40FirstSupplyDate,a__p40WorkSheet_Recurrence__p40LastSupplyDate";                        
                    }
                    if (master_entity == "p41Project" || master_entity=="le5")
                    {
                        s = "a__p40WorkSheet_Recurrence__p40RecurrenceType,a__p40WorkSheet_Recurrence__NejblizsiGenerovani,a__p40WorkSheet_Recurrence__p39Count,a__p40WorkSheet_Recurrence__p40Text,a__p40WorkSheet_Recurrence__p40Value,a__p40WorkSheet_Recurrence__Mena,a__p40WorkSheet_Recurrence__p40FirstSupplyDate,a__p40WorkSheet_Recurrence__p40LastSupplyDate";
                    }
                    break;
                case "p11":
                    s = "p11_j02__j02User__fullname_desc,a__p11Attendance__p11Date,a__p11Attendance__Den,a__p11Attendance__Prichod,a__p11Attendance__p11TodayEnd,a__p11Attendance__Fond,p11_p31__com_dochazka__Hodiny_V_Praci,p11_p31__com_dochazka__Hodiny_Prestavka,p11_p31__com_dochazka__Hodiny_Nepritomnost,p11_p31__com_dochazka__Hodiny_Dovolena,p11_p31__com_dochazka__Hodiny_Obed,p11_p31__com_dochazka__Hodiny_Nemoc,p11_p31__com_dochazka__Hodiny_Other,p11_p31__com_dochazka__Hodiny";
                    break;
                case "p49":
                    s = "a__p49FinancialPlan__p49Code,a__p49FinancialPlan__p49Date,a__p49FinancialPlan__Rok,p49_p41__p41Project__KlientProjektu,p49_p41__p41Project__p41Name,a__p49FinancialPlan__PlanVydaj,a__p49FinancialPlan__PlanOdmena,a__p49FinancialPlan__PlanZisk,p49_p56__p56Task__p56Name,p49_p32__p32Activity__p32Name,a__p49FinancialPlan__p49Text";
                    if (master_entity == "p56Task" || master_entity == "le5" || master_entity == "p41Project")
                    {
                        s = "a__p49FinancialPlan__p49Date,a__p49FinancialPlan__Rok,a__p49FinancialPlan__PlanVydaj,a__p49FinancialPlan__PlanOdmena,a__p49FinancialPlan__PlanZisk,p49_p56__p56Task__p56Name,p49_p32__p32Activity__p32Name,a__p49FinancialPlan__p49Text";
                    }
                    break;
                case "b05":
                    s = "a__b05Workflow_History__b05DateInsert,a__b05Workflow_History__NazevPlusText,a__b05Workflow_History__DruhVazby,a__b05Workflow_History__Projekt,a__b05Workflow_History__p28Name,a__b05Workflow_History__Posun,a__b05Workflow_History__UserInsert_b05Workflow_History";
                    if (master_entity == "mobile")
                    {
                        s = "a__b05Workflow_History__b05DateInsert,a__b05Workflow_History__NazevPlusText,a__b05Workflow_History__UserInsert_b05Workflow_History";
                    }
                    if (master_entity == "recpage")
                    {
                        s = "a__b05Workflow_History__b05DateInsert,a__b05Workflow_History__UserInsert_b05Workflow_History";
                    }
                    break;
                case "fp1":
                    s = "fp1_p41__p41Project__KlientProjektu,fp1_p41__p41Project__p41Name,fp1_p56__p56Task__p56Name,a__fp1__PlanOdmeny,a__fp1__PlanVydaje,a__fp1__PlanZisk,fp1_p31__fp1_p31__Hodiny,fp1_p31__fp1_p31__Odmeny,fp1_p31__fp1_p31__Vydaje,fp1_p31__fp1_p31__HonorarInterni,fp1_p31__fp1_p31__Zisk1";
                    break;
                case "fp2":
                    s = "fp2_p41__p41Project__KlientProjektu,fp2_p41__p41Project__p41Name,a__fp2__PlanVydaje,a__fp2__PlanOdmeny,a__fp2__PlanZisk,fp2_p31__fp2_p31__Hodiny,fp2_p31__fp2_p31__Odmeny,fp2_p31__fp2_p31__Vydaje,fp2_p31__fp2_p31__HonorarInterni,fp2_p31__fp2_p31__Zisk1";
                    break;

                default:
                    return null;
            }
            if (mq.Prefix == "p41" && master_entity == "approve_aio")
            {
                s = "a__p41Project__KlientProjektu,a__p41Project__NazevProjektu,p41_nevyuctovano__com_nevyuctovano__Hodiny_Rozpr,p41_nevyuctovano__com_nevyuctovano__Hodiny,p41_nevyuctovano__com_nevyuctovano__Vydaje,p41_nevyuctovano__com_nevyuctovano__Pausaly,p41_nevyuctovano__com_nevyuctovano__BezDph,p41_nevyuctovano__com_nevyuctovano__Honorar_EUR,p41_vyuctovano__com_vyuctovano__PosledniFaktura_Kdy";
            }

            if (s == null)
            {
                return s;
            }


            List<string> lis = new List<string>();
            var arr = s.Split(",");
            for (int i = 0; i < arr.Count(); i++)   //pokud v definici sloupce chybí určení entity, pak doplnit:
            {
                if (arr[i].Contains("__"))
                {
                    lis.Add(arr[i]);
                }
                else
                {
                    lis.Add("a__" + entity + "__" + arr[i]);
                }
            }

            if (!string.IsNullOrEmpty(master_entity))
            {
                lis = lis.Where(p => !p.Contains(master_entity)).ToList();  //aby se v podřízeném gridu nezobrazovali duplicitní sloupce z nadřízeného gridu
                if (master_entity == "p41Project")
                {
                    lis = lis.Where(p => !p.Contains("p28Contact")).ToList();   //eliminivat klientské sloupce, pokud je nadřízená entita: Projekt
                }
            }


            s = string.Join(",", lis);

            return s;

        }

        


    }
}