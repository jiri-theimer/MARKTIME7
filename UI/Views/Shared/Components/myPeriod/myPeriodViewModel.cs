using DocumentFormat.OpenXml.Drawing;

namespace UI.Views.Shared.Components.myPeriod
{
    public class myPeriodViewModel
    {
        private bool _isloaded;
        public myPeriodViewModel()
        {
            _isloaded = false;
        }
        public string prefix { get; set; }
        
        public string UserParamKey { get; set; }
        public string PeriodField { get; set; }
        public DateTime? d1 { get; set; }
        public DateTime? d2 { get; set; }

        public string PeriodFieldAlias { get; set; }
        public string PeriodAlias { get; set; }
        public int PeriodValue { get; set; }

        public bool IsShowButtonRefresh { get; set; }

        public bool IsLoaded()
        {
            return _isloaded;
        }

        public string d1_iso
        {
            get
            {
                if (this.d1 == null) return "1900-01-01";
                return Convert.ToDateTime(this.d1).ToString("o").Substring(0, 10);
            }
        }
        public string d2_iso
        {
            get
            {
                if (this.d2 == null) return "3000-01-01";
                return Convert.ToDateTime(this.d2).ToString("o").Substring(0, 10);
            }
        }

       

        public bool IsDifferentWithStringVersion(string strStringVersion,ref int intStringVersionValue)
        {
            var arr = BO.Code.Bas.ConvertString2List(strStringVersion, "|");
            if (arr[1] == "") arr[1] = "p31Date";
            if (string.IsNullOrEmpty(this.PeriodField)) this.PeriodField = "p31Date";
            intStringVersionValue = Convert.ToInt32(arr[2]);
            if (intStringVersionValue != this.PeriodValue)
            {
                return true;
            }
            if (arr[1] != this.PeriodField)
            {
                return true;
            }
            

            if ((this.PeriodValue==1 && this.d1.HasValue && this.d2.HasValue))
            {
                if (this.d1.Value.ToString("dd.MM.yyyy") != arr[2] || this.d2.Value.ToString("dd.MM.yyyy") != arr[3])
                {
                    return true;
                }
            }

            return false;
            
        }
        public void LoadFromString(BL.Singleton.ThePeriodProvider pp,BL.Factory f, string strPeriod)
        {
            //struktura: prefix|periodfield|periodvalue|d1|d2
            var arr = BO.Code.Bas.ConvertString2List(strPeriod,"|");
            this.prefix = arr[0];
            this.PeriodField = arr[1];
            this.PeriodValue = Convert.ToInt32(arr[2]);
                        
            switch (this.PeriodValue)
            {
                case 0:
                    this.d1 = new DateTime(1900, 1, 1);
                    this.d2 = new DateTime(3000, 1, 1);
                    this.PeriodAlias = null;
                    break;                    
                case 1:
                    if (!string.IsNullOrEmpty(arr[3]) && !string.IsNullOrEmpty(arr[4]))
                    {
                        this.d1 = BO.Code.Bas.String2Date(arr[3]);
                        this.d2 = BO.Code.Bas.String2Date(arr[4]);
                    }                    
                    break;
                case > 60:
                    var rec = f.FBL.LoadX21(this.PeriodValue);
                    if (rec != null)
                    {
                        this.PeriodValue = rec.pid;
                        this.d1 = rec.x21ValidFrom;
                        this.d2 = rec.x21ValidUntil;
                    }
                    else
                    {
                        this.PeriodValue = 0;
                    }
                    break;
                default:
                    var r = pp.ByPid(this.PeriodValue);
                    this.PeriodValue = r.pid;
                    this.d1 = r.d1;
                    this.d2 = r.d2;
                    break;
            }

            RefreshState(pp, f);


        }

        public void SaveUserSetting(BL.Factory f)
        {
            f.CBL.SetUserParam($"{this.UserParamKey}-field", this.PeriodField);
            f.CBL.SetUserParam($"{this.UserParamKey}-value", this.PeriodValue.ToString());
            if (this.PeriodValue == 1 || this.PeriodValue>60)
            {
                if (this.d1.HasValue)
                {
                    f.CBL.SetUserParam($"{this.UserParamKey}-d1", this.d1.Value.ToString("dd.MM.yyyy"));
                }
                if (this.d2.HasValue)
                {
                    f.CBL.SetUserParam($"{this.UserParamKey}-d2", this.d2.Value.ToString("dd.MM.yyyy"));
                }
            }
            
        }
        public void SetIsLoaded()
        {
            _isloaded = true;
        }
        public void LoadUserSetting(BL.Singleton.ThePeriodProvider pp, BL.Factory f,int default_value=0)
        {
            SetIsLoaded();
            int x = f.CBL.LoadUserParamInt($"{this.UserParamKey}-value", default_value);  //podformuláře filtrují období za sebe a nikoliv globálně jako flatview/masterview
            if (x > 0)
            {
                this.PeriodField = f.CBL.LoadUserParam($"{this.UserParamKey}-field");
            }

            if (x > 60)
            {
                //uživatelem definované období
                var rec = f.FBL.LoadX21(x);
                if (rec != null)
                {
                    this.PeriodValue = rec.pid;
                    this.d1 = rec.x21ValidFrom;
                    this.d2 = rec.x21ValidUntil;
                }
                else
                {
                    this.PeriodValue = 0;
                }
                RefreshState(pp, f);
                return;
            }
            
            switch (x)
            {
                case 0: //nefiltrovat období
                    this.PeriodValue = 0;
                    break;
                case 1:     //ručně zadaný interval d1-d2
                    var r1 = pp.ByPid(1);
                    this.PeriodValue = r1.pid;
                    this.d1 = f.CBL.LoadUserParamDate($"{this.UserParamKey}-d1");
                    this.d2 = f.CBL.LoadUserParamDate($"{this.UserParamKey}-d2");
                    break;
                default:    //pojmenované období
                    var r = pp.ByPid(x);
                    this.PeriodValue = r.pid;
                    
                    this.d1 = r.d1;
                    this.d2 = r.d2;
                    break;
            }

            RefreshState(pp, f);
        }

        private void RefreshState(BL.Singleton.ThePeriodProvider pp,BL.Factory f)
        {
            this.PeriodAlias = null;
            string s = null;
            if (this.PeriodValue > 0)
            {
                switch (this.PeriodField)
                {
                    case "p31Date":
                        s = f.tra("Datum úkonu"); break;
                    case "p91Date":
                        s = f.tra("Datum vyúčtování"); break;
                    case "p91DateMaturity":
                    case "p90DateMaturity":
                        s = f.tra("Datum splatnosti"); break;
                    case "p90Date":
                        s = f.tra("Datum zálohy"); break;
                    case "p91DateSupply":
                        s = f.tra("DUZP vyúčtování"); break;
                    case "p31DateInsert":
                        s = f.tra("Čas založení úkonu"); break;
                    case "p91DateInsert":
                        s = f.tra("Čas vygenerování vyúčtování"); break;
                    case "p90DateInsert":
                        s = f.tra("Čas založení zálohy"); break;
                    case "p41DateInsert":
                        s = f.tra("Čas založení projektu"); break;
                    case "p28DateInsert":
                        s = f.tra("Čas založení kontaktu"); break;
                    case "j02DateInsert":
                        s = f.tra("Čas založení uživatele"); break;
                    case "o23DateInsert":
                        s = f.tra("Čas založení záznamu agendy"); break;
                    case "b05DateInsert":
                        s = f.tra("Čas záznamu"); break;
                    case "b05Date":
                        s = f.tra("Datum poznámky"); break;
                    case "p41PlanFrom":
                        s = f.tra("Plán zahájení"); break;
                    case "p41PlanUntil":
                        s = f.tra("Plán dokončení"); break;
                    case "p84Date":
                        s = f.tra("Datum upomínky"); break;
                    case "p39Date":
                        s = f.tra("Datum úkonu"); break;
                    case "p39DateCreate":
                        s = f.tra("Datum generování"); break;
                    case "p49Date":
                        s = f.tra("Datum plánu"); break;
                    case "p91DateBilled":
                        s = f.tra("Úhrada vyúčtování"); break;
                    case "p31Approved_When":
                        s = f.tra("Čas schválení"); break;

                    case "o23ReminderDate":
                        s = f.tra("Datum připomenutí záznamu agendy"); break;
                    default:
                        switch (this.prefix)
                        {
                            case "p91":
                                s = f.tra("Datum vyúčtování"); break;
                            case "p90":
                                s = f.tra("Datum zálohy"); break;

                            case "o23":
                                s = f.tra("Čas založení záznamu agendy"); break;
                            case "b05":
                                s = f.tra("Čas záznamu"); break;
                            default:
                                s = f.tra("Datum úkonu"); break;
                        }

                        break;
                }
            }
            else
            {
                s = f.tra("Časové období");
            }
            this.PeriodFieldAlias = s;

            if (this.PeriodValue > 1)
            {
                BO.ThePeriod per = null;
                if (this.PeriodValue > 60)
                {
                    var rec = f.FBL.LoadX21(this.PeriodValue);  //uživatelsky pojmenované období 
                    if (rec != null)
                    {
                        this.d1 = rec.x21ValidFrom;
                        this.d2 = rec.x21ValidUntil;                        
                        this.PeriodAlias = $"{rec.x21Name}: {BO.Code.Bas.ObjectDate2String(rec.x21ValidFrom)} - {BO.Code.Bas.ObjectDate2String(rec.x21ValidUntil)}";
                    }
                }
                else
                {
                    per = pp.ByPid(this.PeriodValue);

                    if (per.d1.Value.Year == per.d2.Value.Year && per.d1.Value.Year==DateTime.Now.Year)
                    {

                        this.PeriodAlias = $"{f.tra(per.PeriodName)}: {per.d1.Value.ToString("d.M.")} - {per.d2.Value.ToString("d.M.")}";
                    }
                    else
                    {
                        this.PeriodAlias = $"{f.tra(per.PeriodName)}: {per.d1.Value.ToString("d.M.yyyy")} - {per.d2.Value.ToString("d.M.yyyy")}";
                    }
                    
                    

                }


            }
            if (this.PeriodValue == 1)
            {
                this.PeriodAlias = $"{BO.Code.Bas.ObjectDate2String(this.d1)} - {BO.Code.Bas.ObjectDate2String(this.d2)}";

            }
        }
    }
}
