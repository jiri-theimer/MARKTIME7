using BO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BL
{
    public interface Ix55WidgetBL
    {
        public BO.x55Widget Load(int pid);
        public BO.x55Widget LoadByCode(string code, int pid_exclude);
        public IEnumerable<BO.x55Widget> GetList(BO.myQuery mq);
        public int Save(BO.x55Widget rec);

        public BO.x56WidgetBinding LoadState(int j02id,string skin,int x54id);
        public int SaveState(BO.x56WidgetBinding rec);
        public int Clear2FactoryState(BO.x56WidgetBinding rec);
        public void SetAsDefaultGlobalBoxes(string boxes);
        public void UpdateBoxes2AllUsers(string boxes,string skin, int x54id);

    }
    class x55WidgetBL : BaseBL, Ix55WidgetBL
    {
        public x55WidgetBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null,bool istestcloud=false)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("x55"));
            sb(" FROM x55Widget a");
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
        public BO.x55Widget Load(int pid)
        {
            return _db.Load<BO.x55Widget>(GetSQL1(" WHERE a.x55ID=@pid"), new { pid = pid });
        }
        public BO.x55Widget LoadByCode(string code, int pid_exclude)
        {
            return _db.Load<BO.x55Widget>(GetSQL1(" WHERE a.x55Code LIKE @code AND a.x55ID<>@pid_exclude AND a.x01ID=@x01id",_mother.CurrentUser.IsHostingModeTotalCloud), new { code = code, pid_exclude = pid_exclude,x01id=_mother.CurrentUser.x01ID });
        }

        public IEnumerable<BO.x55Widget> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x55Widget>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.x55Widget rec)
        {
            
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.x55ID);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("x55Name", rec.x55Name);
            p.AddString("x55Code", rec.x55Code);
            p.AddString("x55TableSql", rec.x55TableSql);
            p.AddString("x55TableColHeaders", rec.x55TableColHeaders);
            p.AddString("x55TableColTypes", rec.x55TableColTypes);
            p.AddString("x55Content", rec.x55Content);
            p.AddInt("x55Ordinal", rec.x55Ordinal);                        
            p.AddString("x55Image", rec.x55Image);
            p.AddString("x55Description", rec.x55Description);
            p.AddNonBlackColorString("x55BoxBackColor", rec.x55BoxBackColor);
            p.AddNonBlackColorString("x55HeaderBackColor", rec.x55HeaderBackColor);
            p.AddNonBlackColorString("x55HeaderForeColor", rec.x55HeaderForeColor);
            p.AddInt("x55DataTablesLimit", rec.x55DataTablesLimit);
            p.AddEnumInt("x55DataTablesButtons", rec.x55DataTablesButtons);
            p.AddString("x55Help", rec.x55Help);
            
            p.AddString("x55ChartSql", rec.x55ChartSql);
            p.AddString("x55ChartHeaders", rec.x55ChartHeaders);
            p.AddString("x55ChartType", rec.x55ChartType);
            p.AddInt("x58ID_Par1", rec.x58ID_Par1, true);
            p.AddInt("x58ID_Par2", rec.x58ID_Par2, true);
            p.AddInt("x04ID", rec.x04ID, true);

            p.AddInt("x55BoxMaxHeight", rec.x55BoxMaxHeight);
            p.AddInt("x55ChartHeight", rec.x55ChartHeight);            
            p.AddString("x55ChartColors", rec.x55ChartColors);
            p.AddString("x55Category", rec.x55Category);
            p.AddString("x55TableColTotals", rec.x55TableColTotals);
            p.AddString("x55ReportCodes", rec.x55ReportCodes);

            int intPID = _db.SaveRecord("x55Widget", p, rec);            
            if (intPID > 0)
            {
                
            }


            return intPID;
        }

        public bool ValidateBeforeSave(BO.x55Widget rec)
        {
            if (string.IsNullOrEmpty(rec.x55Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.x55Code))
            {
                this.AddMessage("Chybí vyplnit [Kód]."); return false;
            }
            rec.x55Code = Regex.Replace(rec.x55Code, "[^a-zA-Z0-9]", "_"); //kód raději pouze pro alfanumerické znaky

            if (LoadByCode(rec.x55Code, rec.pid) != null)
            {
                this.AddMessageTranslated(string.Format(_mother.tra("V systému již existuje jiný widget s kódem: {0}."), rec.x55Code)); return false;
            }
            

            if (!string.IsNullOrEmpty(rec.x55ChartSql) && rec.x55ChartType==null)
            {
                this.AddMessage("Chybí vyplnit [Typ grafu]."); return false;
            }
            if (!string.IsNullOrEmpty(rec.x55ChartSql) && rec.x55ChartHeaders == null)
            {
                this.AddMessage("Chybí vyplnit [Názvy veličin grafu]."); return false;
            }

            return true;
        }

        public int Clear2FactoryState(BO.x56WidgetBinding rec)
        {
            if (rec == null) return 0;

            _mother.CBL.SetUserParam($"Widgets-ColumnsPerPage-{rec.x56Skin}-{rec.x54ID}", "2");    //výchozí jsou 2 sloupce


            var codes = _mother.x54WidgetGroupBL.GetList_x57(rec.x54ID).Where(p => p.x57IsDefault).Select(p => p.x55Code);
            if (codes.Count() > 0)
            {
                rec.x56Boxes = string.Join(",", codes);
            }

            //výchozí paletů widgetů natáhnout z aplikační role            
            //BO.GetString ss = _db.Load<GetString>("SELECT o58DefaultValue as Value FROM o58GlobalParam WHERE o58Entity='x55' AND o58Key='boxes'");
            //if (ss != null)
            //{
            //    rec.x56Boxes = ss.Value;    //globální nastavení
            //}

            rec.x56DockState = null;
            return SaveState(rec);
           

        }

        public void SetAsDefaultGlobalBoxes(string boxes)
        {
            _db.RunSql("DELETE FROM o58GlobalParam WHERE o58Entity='x55' AND o58Key='boxes'");

            if (string.IsNullOrEmpty(boxes))
            {
                return;
            }

            _db.RunSql("INSERT INTO o58GlobalParam(o58ID,o58Name,o58Key,o58Entity,o58UserInsert,o58UserUpdate,o58DefaultValue,o58DateUpdate) VALUES(666,'boxes','boxes','x55',@login,@login,@boxes,GETDATE())", new { login = _mother.CurrentUser.j02Login, boxes = boxes });

        }

        public void UpdateBoxes2AllUsers(string boxes,string skin,int x54id)
        {
            if (string.IsNullOrEmpty(boxes))
            {
                return;
            }

            _db.RunSql("UPDATE x56WidgetBinding SET x56Boxes=@boxes,x56DockState=null WHERE x56Skin=@skin AND x54ID=@x54id", new { boxes = boxes,skin=skin,x54id=x54id });

        }

        public BO.x56WidgetBinding LoadState(int j02id,string skin,int x54id)
        {
            if (string.IsNullOrEmpty(skin)){
                skin = "index";
            }
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("x56"));
            sb(" FROM x56WidgetBinding a WHERE a.j02ID=@j02id AND a.x56Skin=@skin AND a.x54ID=@x54id");
            
            var rec= _db.Load<BO.x56WidgetBinding>(sbret(), new { j02id = j02id,skin=skin,x54id=x54id });

            
            if (rec==null || (rec.DateInsert==rec.DateUpdate))
            {                
                if (rec == null)
                {
                    rec = new BO.x56WidgetBinding() { j02ID = j02id, x56Skin = skin,x54ID=x54id };  //zatím nebyl vytvořený state widgetů                    

                }                
                                                                                    //
                if (Clear2FactoryState(rec) > 0)
                {
                    return LoadState(j02id, skin,x54id);
                }
                
            }
                        

            return rec;
        }

        public int SaveState(BO.x56WidgetBinding rec)
        {            
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("j02ID", rec.j02ID,true);
            p.AddInt("x54ID", rec.x54ID, true);
            p.AddString("x56Skin", rec.x56Skin);
            p.AddString("x56Boxes", rec.x56Boxes);
            p.AddString("x56DockState", rec.x56DockState);
           
            return _db.SaveRecord("x56WidgetBinding", p, rec);

        }
    }
}
