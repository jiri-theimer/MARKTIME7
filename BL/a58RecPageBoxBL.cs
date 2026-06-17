using System.Collections.Generic;

namespace BL
{
    public interface Ia58RecPageBoxBL
    {
        public BO.a58RecPageBox Load(int pid);
        public BO.a58RecPageBox LoadByCode(string strA58Code, int intExcludePID);
        public IEnumerable<BO.a58RecPageBox> GetList(BO.myQuery mq);
        public int Save(BO.a58RecPageBox rec);


    }
    class a58RecPageBoxBL : BaseBL, Ia58RecPageBoxBL
    {
        public a58RecPageBoxBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,b06.b06Name,x31.x31Name,");
            sb(_db.GetSQL1_Ocas("a58"));
            sb(" FROM a58RecPageBox a LEFT OUTER JOIN x31Report x31 ON a.x31ID_Button=x31.x31ID LEFT OUTER JOIN b06WorkflowStep b06 ON a.b06ID_Button=b06.b06ID ");
            sb(strAppend);
            return sbret();
        }
        public BO.a58RecPageBox Load(int pid)
        {
            return _db.Load<BO.a58RecPageBox>(GetSQL1(" WHERE a.a58ID=@pid"), new { pid = pid });
        }

        public BO.a58RecPageBox LoadByCode(string strA58Code, int intExcludePID)
        {
            return _db.Load<BO.a58RecPageBox>(GetSQL1(" WHERE a.a58Code LIKE @code AND a.a58ID<>@excludepid"), new { code = strA58Code, excludepid = intExcludePID });
        }

        public IEnumerable<BO.a58RecPageBox> GetList(BO.myQuery mq)
        {

            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.a58RecPageBox>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.a58RecPageBox rec)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }

            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddString("a58Name", rec.a58Name);
            p.AddString("a58Code", rec.a58Code);
            p.AddString("a58DefaultName", rec.a58DefaultName);
            p.AddInt("a59ID", rec.a59ID, true);           
            p.AddInt("x31ID_Button", rec.x31ID_Button, true);
            p.AddInt("b06ID_Button", rec.b06ID_Button, true);
            p.AddString("a58HtmlText", rec.a58HtmlText);
            
            p.AddString("a58ButtonText", rec.a58ButtonText);
            p.AddInt("x04ID", rec.x04ID, true);
            p.AddEnumInt("a58ContentFlag", rec.a58ContentFlag);
            p.AddString("a58CssClassName", rec.a58CssClassName);
            p.AddString("a58ControlFlag", rec.a58ControlFlag);
            p.AddBool("a58IsHtmlByPlaintext", rec.a58IsHtmlByPlaintext);
                
            int intPID = _db.SaveRecord("a58RecPageBox", p, rec);
            if (intPID > 0 && rec.pid == 0)
            {
                _db.RunSql("UPDATE a58RecPageBox SET a58Guid=RIGHT('0000000000000000'+convert(varchar(10),a58ID),6) WHERE a58ID=@pid", new { pid = intPID });
            }


            return intPID;
        }

        public bool ValidateBeforeSave(BO.a58RecPageBox rec)
        {
            if (rec.a59ID == 0)
            {
                this.AddMessage("Chybí vazba na plochu."); return false;
            }
            if (string.IsNullOrEmpty(rec.a58Code))
            {
                this.AddMessage("Chybí kód prvku."); return false;
            }

            if (LoadByCode(rec.a58Code,rec.pid) != null)
            {
                this.AddMessage("Kód prvku musí být jedinečný v rámci všech funkčních prvků."); return false;
            }

            if ((rec.IsButton() && string.IsNullOrEmpty(rec.a58ButtonText)))
            {
                this.AddMessage("Chybí text tlačítka."); return false;
            }
            if (rec.a58ContentFlag == BO.a58ContentFlagEnum.ButtonOneReport && (rec.x31ID_Button == 0))
            {
                this.AddMessage("Chybí vybrat tiskovou sestavu."); return false;
            }
          
            if (rec.a58ContentFlag == BO.a58ContentFlagEnum.ButtonOneWorkflowStep && (rec.b06ID_Button == 0))
            {
                this.AddMessage("Chybí vybrat workflow krok."); return false;
            }

           
            var adn = new List<string>();            

            switch (rec.a58ContentFlag)
            {
                case BO.a58ContentFlagEnum.ButtonWorkflowDialog:
                    adn.Add("Dialog [Posunout/Doplnit]");
                    break;
                case BO.a58ContentFlagEnum.ButtonReportDialog:
                    adn.Add("Dialog [Tisková sestava]");
                    break;
                case BO.a58ContentFlagEnum.ButtonOneReport:
                    adn.Add("Tisková sestava: " + _mother.x31ReportBL.Load(rec.x31ID_Button).x31Name);
                    break;
              
                case BO.a58ContentFlagEnum.ButtonOneWorkflowStep:
                    //adn.Add("Spustit workflow krok: " + _mother.b06WorkflowStepBL.Load(rec.b06ID_Button).b06Name);
                    break;
                case BO.a58ContentFlagEnum.ButtonUploadO27:
                    adn.Add("Nahrát dokument/přílohu");
                    break;
              
                case BO.a58ContentFlagEnum.o27List:
                    adn.Add("Seznam dokumentů/příloh");
                    break;
                case BO.a58ContentFlagEnum.o32List:
                    adn.Add("Kontaktní média");
                    break;
                case BO.a58ContentFlagEnum.RecHeaderBox:
                    adn.Add("Hlavička záznamu");
                    break;
                case BO.a58ContentFlagEnum.RecTags:
                    adn.Add("Štítky");
                    break;
                default:
                    adn.Add(rec.a58ContentFlag.ToString());
                    break;
            }

            
           

            if (!string.IsNullOrEmpty(rec.a58HtmlText))
            {
                adn.Add("HTML text");
            }

            if (adn.Count==0)
            {
                this.AddMessage("Prvku chybí nastavení."); return false;
                
            }
            rec.a58DefaultName = string.Join("| ",adn);

            return true;
        }

    }
}
