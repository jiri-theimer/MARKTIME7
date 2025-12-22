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
                    wltd(c.p29Name);
                    wltd(c.p28Name);
                    wltd(c.GetFullAddress(1));
                    wltd(c.p28RegID);
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
                    wltd(c.p42Name);
                    if (c.p41ParentID > 0)
                    {
                        wltd(c.p41TreePath);
                    }
                    else
                    {
                        wltd(c.p41Name);
                    }
                    wltd(c.Client);
                    wltd(c.p41Code);

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
                    wltd($"{c.p91Code}");
                    wltd(c.p91Client);
                    wl($"<td style='text-align:right;'>{BO.Code.Bas.Number2String(c.p91Amount_WithoutVat)} {c.j27Code} <code style='margin-left:20px;'>{BO.Code.Bas.ObjectDate2String(c.p91DateSupply, "dd.MM.yyyy")}</code></td>");
                    wltd(c.p92Name);
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
                    wltd($"{c.p90Code}");
                    wltd(c.p28Name);
                    wl($"<td style='text-align:right;'>{BO.Code.Bas.Number2String(c.p90Amount)} {c.j27Code}</td>");
                    wltd(c.p89Name);
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
                    wltd($"{c.p56Code}");
                    wltd(c.p56Name);
                    wltd($"{c.p41Name}");
                    wltd(c.ProjectClient);
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
                    wltd($"{c.o23Code}");
                    wltd(c.o23Name);
                    wltd($"{c.o18Name}");
                    wltd("");
                    wl("</tr>");
                    x++;
                }
            }

            wl("</tbody>");
            wl("</table>");

            if (!bolFound)
            {
                wl("<p style='padding:10px;'><span class='material-icons-outlined-btn' style='color:red;'>search</span>");
                wl($"<var>{Factory.tra("Ani jeden výsledek")}.</var></p>");
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

        private void VysledkyNadpis(string strModule, int intPocet, int firstpid, string prefix)
        {
            if (intPocet > 0)
            {
                if (prefix == "p41")
                {
                    wl($"<tr style='background-color:lightblue;' data-v='{firstpid}' data-prefix='{prefix}' data-skip='1'><td colspan=4>");
                }
                else
                {
                    wl($"<tr class='bg{prefix}' data-v='{firstpid}' data-prefix='{prefix}' data-skip='1'><td colspan=4>");
                }

                if (intPocet == _topRecsLimit)
                {
                    wl($"<small>{strModule}: více než {_topRecsLimit} výsledků.</small>");
                }
                else
                {
                    wl($"<small>{strModule} <span class='badge-light'>{intPocet}x</span></small>");
                }
                wl("</td></tr>");
            }
        }
        private string RenderUvodniInfo()
        {

            
            wl("<p style='padding:10px;'><span class='material-icons-outlined-btn'>search</span><var>Musíte zadat minimálně 3 znaky.</var></p>");
            
            wl("<iframe frameborder=0 src='/Home/MySearchSetting' style='height:250px;'></iframe>");

           
            
            return _sb.ToString();
        }
    }
}
