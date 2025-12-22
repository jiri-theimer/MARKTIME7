using BO.Code.Cls;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
    public class VysledovkaController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        
        public VysledovkaController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
            
        }
        public IActionResult Index(string master_entity, int master_pid,string caller)
        {
            var v = new VysledovkaViewModel() { prefix = master_entity.Substring(0,3), pid = master_pid };            
            if (string.IsNullOrEmpty(v.prefix) || v.pid == 0) return this.StopPageSubform("prefix or pid missing");
            v.VysledovkaHeader = Factory.CBL.GetObjectAlias(v.prefix, v.pid);

            v.periodinput = new Views.Shared.Components.myPeriod.myPeriodViewModel() { prefix = v.prefix, UserParamKey = "vysledovka-period" };
            v.periodinput.LoadUserSetting(_pp, Factory);    //načtení d1/d2/period_field pro query

            v.ScenarNakladovaSazba = (BO.Model.TimesheetCostRateEnum)Factory.CBL.LoadUserParamInt("vysledovka-nakladovasazba-scenar", 5);
            v.ScenarVysledovkaVydaje = Factory.CBL.LoadUserParamInt("vysledovka-scenar-vydaje", 2);
            v.ScenarVysledovkaHodiny = Factory.CBL.LoadUserParamInt("vysledovka-scenar-hodiny", 1);

            v.IsVystup_Aktivita = Factory.CBL.LoadUserParamBool("vysledovka-IsVystup_Aktivita-"+v.prefix, false);
            v.IsVystup_Osoba = Factory.CBL.LoadUserParamBool("vysledovka-IsVystup_Osoba-"+v.prefix, false);
            v.IsVystup_Aktivita_Osoba = Factory.CBL.LoadUserParamBool("vysledovka-IsVystup_Aktivita_Osoba-"+v.prefix, false);
            v.IsVystup_Aktivita_Osoba = Factory.CBL.LoadUserParamBool("vysledovka-IsVystup_Aktivita_Osoba-"+v.prefix, false);
            v.IsVystup_Analyza = Factory.CBL.LoadUserParamBool("vysledovka-IsVystup_Analyza-"+v.prefix, false);
            v.IsVystup_Fee = Factory.CBL.LoadUserParamBool("vysledovka-IsVystup_Fee-"+v.prefix, false);
            v.IsVystup_Expense = Factory.CBL.LoadUserParamBool("vysledovka-IsVystup_Expense-"+v.prefix, false);
            v.IsVystup_Vysledovka1 = Factory.CBL.LoadUserParamBool("vysledovka-IsVystup_Vysledovka1-" + v.prefix, false);
            
            v.NakladovaSazbaCislo = BO.Code.Bas.InDouble(Factory.CBL.LoadUserParam("vysledovka-NakladovaSazbaCislo", (Factory.Lic.j27ID == 2 ? "500" : "25")));
            v.NakladovaSazbaProcento= Convert.ToDouble(Factory.CBL.LoadUserParam("vysledovka-NakladovaSazbaProcento"));

            

            GenerateVystupy(v);

            if (caller == "grid" && !string.IsNullOrEmpty(master_entity) && master_pid > 0)
            {
                Factory.CBL.SaveLastCallingRecPid(master_entity, master_pid, "grid", true, false, null);   //uložit info o naposledy vybraném záznamu v gridu
            }
            return ViewTup(v, BO.PermValEnum.GR_P31_Vysledovky);
            
        }

        private void GenerateVystupy(VysledovkaViewModel v)
        {            
            v.Vystup_Overview1 = LoadVystup(v,"overview1.ds");
            v.Vystup_Overview2 = LoadVystup(v,"overview2.ds");

            if (v.IsVystup_Osoba) v.Vystup_Osoba = LoadVystup(v, "hours1.ds");
            if (v.IsVystup_Aktivita) v.Vystup_Aktivita = LoadVystup(v, "hours4.ds");
            if (v.IsVystup_Aktivita_Osoba) v.Vystup_Aktivita_Osoba = LoadVystup(v, "hours3.ds");
            if (v.IsVystup_Osoba_Aktivita) v.Vystup_Osoba_Aktivita = LoadVystup(v, "hours2.ds");
            if (v.IsVystup_Vysledovka1) v.Vystup_Vysledovka1 = LoadVystup(v, "vysledovka1.ds");
            if (v.IsVystup_Expense) v.Vystup_Expense = LoadVystup(v, "expenses1.ds");
            if (v.IsVystup_Fee) v.Vystup_Fee = LoadVystup(v, "fees1.ds");
            if (v.IsVystup_Analyza) v.Vystup_Analyza = LoadVystup(v, "effectrates1.ds");

        }
        private System.Data.DataTable GetDT(BL.Code.HtmlTable tab)
        {
            return Factory.gridBL.GetListFromPureSql(tab.Sql);
        }
        private string LoadVystup(VysledovkaViewModel v,string strFileName)
        {
            var def = LoadDefFile(v, $"{BO.Code.Entity.GetPrefixDb(v.prefix)}_{strFileName}");
            if (def == null)
            {
                return null;
            }
            
            def.IsFullWidth = false;
            var vystup = new BL.Code.Datatable2Html(def,Factory).CreateHtmlTable(GetDT(def));
            
            return "<div style='margin-top:30px;'>" + vystup + "</div>";
        }
        private BL.Code.HtmlTable LoadDefFile(VysledovkaViewModel v,string filename)
        {
            if (!System.IO.File.Exists(Factory.App.RootUploadFolder + $"\\_distribution\\vysledovka\\{filename}"))
            {
                return null;
            }
            string strFile = Factory.App.RootUploadFolder + $"\\_distribution\\vysledovka\\{filename}";
            string s = BO.Code.File.GetLinesContent(strFile, 6, 1000);

            if (v.prefix == "le4" || v.prefix == "le3" || v.prefix == "le2" || v.prefix == "le1")
            {
                s = s.Replace(" a.p41ID=@pid",$" a.p41ID IN (select p41ID FROM p41Project WHERE p41ID_P07Level{v.prefix.Substring(2,1)}=@pid)");
            }
            

            string strSQL = BO.Code.Bas.OcistitSQL(s).Replace("@pid", v.pid.ToString());
            strSQL = strSQL.Replace("@d1", BO.Code.Bas.GD(v.periodinput.d1 == null ? new DateTime(2000, 1, 1) : v.periodinput.d1));
            strSQL = strSQL.Replace("@d2", BO.Code.Bas.GD(v.periodinput.d2 == null ? new DateTime(3000, 1, 1) : v.periodinput.d2));

            switch (v.periodinput.PeriodField)
            {
                case "p91DateSupply":
                    strSQL = strSQL.Replace("xa.p31Date BETWEEN", "xd.p91DateSupply BETWEEN");
                    strSQL = strSQL.Replace("a.p31Date BETWEEN", "p91.p91DateSupply BETWEEN");
                    break;
                case "p91Date":
                    strSQL = strSQL.Replace("xa.p31Date BETWEEN", "xd.p91Date BETWEEN");
                    strSQL = strSQL.Replace("a.p31Date BETWEEN", "p91.p91Date BETWEEN");
                    break;
                case "p31DateInsert":
                    strSQL = strSQL.Replace("a.p31Date BETWEEN", "a.p31DateInsert BETWEEN");
                    break;
            }
            if (v.ScenarVysledovkaVydaje == 1)
            {
                strSQL = strSQL.Replace("rst.[Vydaje_Vynos]", "0");  //peněžní výdaje jsou kompletně na odpis
            }
            if (v.ScenarVysledovkaHodiny == 2)
            {
                strSQL = strSQL.Replace("rst.[Honorar]", "0");  //veškeré hodiny odepsat
            }
            switch (v.ScenarNakladovaSazba)
            {
                case BO.Model.TimesheetCostRateEnum.ProcentoFakturacniSazby:
                    strSQL = strSQL.Replace("a.p31Rate_Internal_Orig", "a.p31Rate_Billing_Orig*" + BO.Code.Bas.GN(v.NakladovaSazbaProcento) + "/100");    //nákladová sazba = procento
                    break;
                case BO.Model.TimesheetCostRateEnum.SimulacniSazbaRucne:
                    strSQL = strSQL.Replace("a.p31Rate_Internal_Orig", BO.Code.Bas.GN(v.NakladovaSazbaCislo));  //nákladová sazba = konstanta
                    break;
                case BO.Model.TimesheetCostRateEnum.SimulacniSazbaProjekt:
                    strSQL = strSQL.Replace("a.p31Rate_Internal_Orig", "p41.p41Plan_Internal_Rate");    //simulační sazba projektu
                    break;
                case BO.Model.TimesheetCostRateEnum.RezijniSazba:
                    strSQL = strSQL.Replace("a.p31Rate_Internal_Orig", "a.p31Rate_Overhead");    //režijní sazba
                    break;
                case BO.Model.TimesheetCostRateEnum.SimulacniSazbaUzivatel:
                    strSQL = strSQL.Replace("a.p31Rate_Internal_Orig", "j02.j02Plan_Internal_Rate");    //simulační sazba uživatele
                    break;
            }
            
            var ret = new BL.Code.HtmlTable() { HeaderBgColor = "#add8e6" };
            
            if (filename.Contains("hours"))
            {
                ret.HeaderBgColor = "#ffdab9";
            }
            if (filename.Contains("expense"))
            {
                ret.HeaderBgColor = "#fa8072";
            }
            if (filename.Contains("fee"))
            {
                ret.HeaderBgColor = "#8fbc8f";
            }

            ret.TableAfterCaption= BO.Code.File.GetLinesContent(strFile, 5, 5);
            ret.ColHeaders = BO.Code.File.GetLinesContent(strFile, 1, 1);
            ret.ColTypes = BO.Code.File.GetLinesContent(strFile, 2, 2);
            ret.ColFlexSubtotals= BO.Code.File.GetLinesContent(strFile, 3, 3);
            ret.ColStyles= BO.Code.File.GetLinesContent(strFile, 4, 4);
            ret.Sql = strSQL;
            ret.MailSubject = $"{v.VysledovkaHeader}: {ret.TableAfterCaption}";


            return ret;
            
        }
    }
}
