using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class TheSearchController : BaseController
    {
        private System.Text.StringBuilder _sb { get; set; }
        private int _topRecsLimit = 50;
        public string GetHtml4TheSearch(string tableid, string searchstring)
        {
            _sb = new System.Text.StringBuilder();


            if (string.IsNullOrEmpty(searchstring) || searchstring.Trim().Length <= 2)
            {
                return RenderUvodniInfo();
            }

            searchstring = searchstring.Trim();

            string strTrClass = null;
            bool bolFound = false;
            int x = 0;
            int intBitStream = Factory.j02UserBL.Load(Factory.CurrentUser.pid).j02MySearchBitStream;
            if (intBitStream == 0)
            {
                intBitStream = 60;  //výchozí hodnota
            }
            
            bool? bolRecordValid = null;
            if (!BO.Code.Bas.bit_compare_or(intBitStream, 2))
            {
                bolRecordValid = true;
            }
            wl($"<table id='{tableid}' class='table table-thecombo'>");
            wl(string.Format("<tbody id='{0}_tbody'>", tableid));

            if (Factory.CurrentUser.j04IsModule_p28 && BO.Code.Bas.bit_compare_or(intBitStream, 8))
            {
                var mq = new BO.myQueryP28() { TopRecordsOnly = _topRecsLimit, SearchString = searchstring, IsRecordValid = bolRecordValid, MyRecordsDisponible = true };
                var lis = Factory.p28ContactBL.GetList(mq);
                x = 0;
                foreach (var c in lis)
                {
                    if (x == 0)
                    {
                        bolFound = true;
                        VysledkyNadpis(Factory.tra("Kontakty"), lis.Count(), c.pid, "p28");
                    }
                    strTrClass = "txz";
                    if (c.isclosed)
                    {
                        strTrClass = "txz isclosed";
                    }
                    wl($"<tr class='{strTrClass}' data-v='{c.pid}' data-prefix='p28'>");
                    tdMain(c.p28ShortName != null ? c.FullNameDesc : c.p28Name, c.p28ShortName);
                    tdSub(c.p29Name);
                    td(c.GetFullAddress(1));
                    td(c.p28RegID, "mtc-num");
                    wl("</tr>");
                    x++;
                }
            }

            if (Factory.CurrentUser.j04IsModule_p41 && BO.Code.Bas.bit_compare_or(intBitStream,4))
            {
                var mq = new BO.myQueryP41("p41") { TopRecordsOnly = _topRecsLimit, SearchString = searchstring, IsRecordValid = bolRecordValid, MyRecordsDisponible = true };
                var lis = Factory.p41ProjectBL.GetList(mq);
                x = 0;
                foreach (var c in lis)
                {
                    if (x == 0)
                    {
                        bolFound = true;
                        VysledkyNadpis(Factory.tra("Projekty"), lis.Count(), c.pid, "p41");
                    }
                    strTrClass = "txz";
                    if (c.isclosed)
                    {
                        strTrClass = "txz isclosed";
                    }
                    wl($"<tr class='{strTrClass}' data-v='{c.pid}' data-prefix='p41'>");
                    if (c.p41ParentID > 0)
                    {
                        tdMain(c.p41TreePath, null);
                    }
                    else
                    {
                        tdMain(c.p41Name, c.p41NameShort);
                    }
                    tdSub(c.p42Name);
                    td(c.Client);
                    td(c.p41Code, "mtc-num");

                    wl("</tr>");
                    x++;
                }
            }
               


            if (searchstring.Length >= 3 && Factory.CurrentUser.j04IsModule_p91 && BO.Code.Bas.bit_compare_or(intBitStream, 16))
            {
                var mq = new BO.myQueryP91() { TopRecordsOnly = _topRecsLimit, SearchString = searchstring, IsRecordValid = bolRecordValid, MyRecordsDisponible = true };
                var lis = Factory.p91InvoiceBL.GetList(mq);
                x = 0;
                foreach (var c in lis)
                {
                    if (x == 0)
                    {
                        bolFound = true;
                        VysledkyNadpis(Factory.tra("Vyúčtování"), lis.Count(), c.pid, "p91");
                    }
                    strTrClass = "txz";
                    if (c.isclosed)
                    {
                        strTrClass = "txz isclosed";
                    }
                    wl($"<tr class='{strTrClass}' data-v='{c.pid}' data-prefix='p91'>");
                    tdMain(c.p91Code, null);
                    tdSub(c.p92Name);
                    td(c.p91Client);
                    td($"{BO.Code.Bas.Number2String(c.p91Amount_WithoutVat)} {c.j27Code} · {BO.Code.Bas.ObjectDate2String(c.p91DateSupply, "d.M.yyyy")}", "mtc-num");
                    wl("</tr>");
                    x++;
                }
            }

            if (searchstring.Length >= 3 && Factory.CurrentUser.j04IsModule_p90 && BO.Code.Bas.bit_compare_or(intBitStream, 32))
            {
                var mq = new BO.myQueryP90() { TopRecordsOnly = _topRecsLimit, SearchString = searchstring, IsRecordValid = bolRecordValid, MyRecordsDisponible = true };
                var lis = Factory.p90ProformaBL.GetList(mq);
                x = 0;
                foreach (var c in lis)
                {
                    if (x == 0)
                    {
                        bolFound = true;
                        VysledkyNadpis(Factory.tra("Zálohy"), lis.Count(), c.pid, "p90");
                    }
                    strTrClass = "txz";
                    if (c.isclosed)
                    {
                        strTrClass = "txz isclosed";
                    }
                    wl($"<tr class='{strTrClass}' data-v='{c.pid}' data-prefix='p90'>");
                    tdMain(c.p90Code, null);
                    tdSub(c.p89Name);
                    td(c.p28Name);
                    td($"{BO.Code.Bas.Number2String(c.p90Amount)} {c.j27Code}", "mtc-num");
                    wl("</tr>");
                    x++;
                }
            }


            if (searchstring.Length >= 4 && Factory.CurrentUser.j04IsModule_p56 && BO.Code.Bas.bit_compare_or(intBitStream, 64))
            {
                var mq = new BO.myQueryP56() { TopRecordsOnly = _topRecsLimit, SearchString = searchstring, IsRecordValid = bolRecordValid, MyRecordsDisponible = true };
                var lis = Factory.p56TaskBL.GetList(mq);
                x = 0;
                foreach (var c in lis)
                {
                    if (x == 0)
                    {
                        bolFound = true;
                        VysledkyNadpis(Factory.tra("Úkoly"), lis.Count(), c.pid, "p56");
                    }
                    strTrClass = "txz";
                    if (c.isclosed)
                    {
                        strTrClass = "txz isclosed";
                    }
                    wl($"<tr class='{strTrClass}' data-v='{c.pid}' data-prefix='p56'>");
                    tdMain(c.p56Name, null);
                    tdSub(c.p56Code);
                    td(c.p41Name);
                    td(c.ProjectClient);
                    wl("</tr>");
                    x++;
                }
            }
            if (searchstring.Length >= 3 && Factory.CurrentUser.j04IsModule_o23 && BO.Code.Bas.bit_compare_or(intBitStream, 128))
            {
                var mq = new BO.myQueryO23() { TopRecordsOnly = _topRecsLimit, SearchString = searchstring, IsRecordValid = bolRecordValid, MyRecordsDisponible = true };
                var lis = Factory.o23DocBL.GetList(mq);
                x = 0;
                foreach (var c in lis)
                {
                    if (x == 0)
                    {
                        bolFound = true;
                        VysledkyNadpis(Factory.tra("Dokumenty"), lis.Count(), c.pid, "o23");
                    }
                    strTrClass = "txz";
                    if (c.isclosed)
                    {
                        strTrClass = "txz isclosed";
                    }
                    wl($"<tr class='{strTrClass}' data-v='{c.pid}' data-prefix='o23'>");
                    tdMain(c.o23Name, null);
                    tdSub(c.o18Name);
                    td(c.o23Code, "mtc-num");
                    td("");
                    wl("</tr>");
                    x++;
                }
            }

            wl("</tbody>");
            wl("</table>");

            if (!bolFound)
            {
                wl("<div class='ms-info'><span class='material-icons-outlined-nosize'>search_off</span><div>");
                wl($"<b>{Factory.tra("Nic nenalezeno pro")} „{H(searchstring)}“</b>");
                wl($"<small>{Factory.tra("Zkuste jiný tvar slova, část názvu, IČ nebo kód záznamu. Úkoly se hledají od 4 znaků.")}</small>");
                wl("</div></div>");
            }


            return _sb.ToString();


        }

        private void wl(string s)
        {
            _sb.Append(s);
        }
        private void wltd(string s)
        {
            _sb.Append("<td>");
            _sb.Append(s);
            _sb.Append("</td>");
        }

        //nadpis skupiny výsledků (mycombo.js řádky s data-skip přeskakuje), vzhled mtc-group / ms-* v site.css
        private void VysledkyNadpis(string strModule, int intPocet, int firstpid, string prefix)
        {
            if (intPocet > 0)
            {
                string strCount = intPocet >= _topRecsLimit ? $"{_topRecsLimit}+" : intPocet.ToString();
                wl($"<tr class='mtc-group ms-group' data-v='{firstpid}' data-prefix='{prefix}' data-skip='1'><td colspan=4>");
                string strIcon = prefix switch { "p28" => "contacts", "p41" => "work_outline", "p91" => "receipt_long", "p90" => "receipt", "p56" => "task_alt", "o23" => "file_present", _ => "search" };
                wl($"<span class='material-icons-outlined-nosize ms-ico'>{strIcon}</span>{H(strModule)} <span class='ms-n'>{strCount}</span>");
                if (intPocet >= _topRecsLimit)
                {
                    wl($" <span class='ms-more'>{Factory.tra("zobrazeno prvních")} {_topRecsLimit} – {Factory.tra("zpřesněte hledání")}</span>");
                }
                wl("</td></tr>");
            }
        }

        private static string H(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");

        //hlavní text výsledku (tučně) + zkratka
        private void tdMain(string text, string code)
        {
            _sb.Append($"<td class='ms-main'>{H(text)}");
            if (!string.IsNullOrEmpty(code)) _sb.Append($" <span class='ms-code'>{H(code)}</span>");
            _sb.Append("</td>");
        }
        //typ / druh záznamu (šedě)
        private void tdSub(string text)
        {
            _sb.Append($"<td class='ms-sub'>{H(text)}</td>");
        }
        private void td(string text, string css = null)
        {
            _sb.Append(css == null ? "<td>" : $"<td class='{css}'>");
            _sb.Append(H(text));
            _sb.Append("</td>");
        }
        private string RenderUvodniInfo()
        {

            
            wl("<div class='ms-info'><span class='material-icons-outlined-nosize'>search</span><div>");
            wl($"<b>{Factory.tra("Napište alespoň 3 znaky")}</b>");
            wl($"<small>{Factory.tra("Hledá v názvech, zkratkách, kódech a IČ kontaktů, projektů, vyúčtování, záloh, úkolů a dokumentů. Kde hledat, nastavíte níže.")}</small>");
            wl("</div></div>");
            
            wl("<iframe class='ms-setting' frameborder=0 src='/Home/MySearchSetting'></iframe>");

           
            
            return _sb.ToString();
        }
    }
}
