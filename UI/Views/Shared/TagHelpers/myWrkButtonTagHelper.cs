
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mywrkbutton")]
    public class myWrkButtonTagHelper : TagHelper
    {
        [HtmlAttributeName("prefix")]
        public string prefix { get; set; }

        [HtmlAttributeName("pid")]
        public string pid { get; set; }

        [HtmlAttributeName("b02id")]
        public int b02id { get; set; }

        [HtmlAttributeName("langindex")]
        public int langindex { get; set; }

        [HtmlAttributeName("showcount")]
        public int showcount { get; set; }

        [HtmlAttributeName("showcopyurl")]
        public bool isshowcopyurl { get; set; }

        //kvůli internímu chatu: přístup k BL.Factory aktuálního požadavku
        [HtmlAttributeNotBound]
        [Microsoft.AspNetCore.Mvc.ViewFeatures.ViewContext]
        public Microsoft.AspNetCore.Mvc.Rendering.ViewContext ViewContext { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string s = "Doplnit poznámku";
            if (b02id > 0)
            {
                s = "Workflow/Doplnit poznámku";
            }
            
            string strBadgeB05 = null;
            if (showcount > 0)
            {
                strBadgeB05 = $"<span class='badge-def'>{showcount}</span>";
            }
            string strStyle = "float:right;margin-left:4px;";
           
            strStyle = $"{strStyle};visibility:hidden;";
            string strTitleRP = "Stránka záznamu";
            string strDbPrefix = BO.Code.Entity.GetPrefixDb(prefix);

            //output.Content.AppendHtml("<div style='position:fixed;top:50px;left:150px;'>");

           
            if (strDbPrefix == "p41" || prefix == "p28" || prefix == "p56" || prefix == "j02")
            {
                output.Content.AppendHtml($"<button id='cmdWrkQuickStat' type='button' class='btn btn-light px-1' style='{strStyle};background-color:#E7FFE7;' title='Rychlá statistika' onclick=\"_window_open('/Record/QuickStat?prefix={prefix}&pid={pid}')\"><span class='material-icons-outlined-btn'>auto_graph</span></button>");

            }
            output.Content.AppendHtml($"<button id='cmdWrkTotals' type='button' class='btn btn-light px-1' style='{strStyle};background-color:#E7FFE7;' title='Pivot-Explorer' onclick=\"_window_open('/p31Totals/Index?selected_entity={prefix}&selected_pids={pid}&blank=1')\"><span class='material-icons-outlined-btn'>functions</span></button>");
            output.Content.AppendHtml($"<button id='cmdWrkReport' type='button' class='btn btn-light px-1' style='{strStyle};background-color:#E7FFE7;' title='Pevný REPORT' onclick=\"_window_open('/ReportsClient/ReportContext?prefix={strDbPrefix}&pid={pid}')\"><span class='material-icons-outlined-btn'>print</span></button>");


            if (this.isshowcopyurl)
            {               
                string strTitle = "Kopírovat url stránky záznamu";

                switch (strDbPrefix)
                {
                    case "p41":
                        strTitle = "Kopírovat url stránky projektu"; strTitleRP = "Stránka projektu"; break;
                    case "p28":
                        strTitle = "Kopírovat url stránky kontaktu"; strTitleRP = "Stránka kontaktu"; break;
                    case "p91":
                        strTitle = "Kopírovat url stránky vyúčtování"; strTitleRP = "Stránka vyúčtování"; break;
                    case "p56":
                        strTitle = "Kopírovat url stránky úkolu"; strTitleRP = "Stránka úkolu"; break;
                    case "j02":
                        strTitle = "Kopírovat url stránky uživatele"; strTitleRP = "Stránka uživatele"; break;
                }
                output.Content.AppendHtml($"<button id='cmdCopyRecPageUrl' title='{strTitle}' type='button' class='btn btn-light px-1' style='{strStyle}' onclick=\"_copy_recpage_to_clipboard('{prefix}',{pid})\"><span class='material-icons-outlined-btn'>content_copy</span></button>");

            }

            output.Content.AppendHtml($"<button id='cmdWrkEmail' type='button' class='btn btn-light px-1' style='{strStyle}' title='E-mail + PDF otisk' onclick=\"_window_open('/Mail/SendMail?record_entity={prefix}&record_pid={pid}')\"><span class='material-icons-outlined-btn'>alternate_email</span></button>");

            

            output.Content.AppendHtml($"<a id='cmdWrkRecPage' data-prefix='{prefix}' data-pid='{pid}' target='_top' class='btn btn-light px-1' style='{strStyle}' title='{strTitleRP}'><span class='material-icons-outlined-btn'>maps_home_work</span></a>");

            //if (showcount > 0)
            //{
            //    strStyle = strStyle.Replace("hidden", "visible");
            //}
            output.Content.AppendHtml($"<button id='cmdWrkButton' type='button' class='btn btn-light px-1' style='{strStyle}' title='{s}' onclick=\"_window_open('/workflow_dialog/Index?record_prefix={prefix}&record_pid={pid}')\"><span class='material-icons-outlined-btn'>waves</span>{strBadgeB05}</button>");

            AppendChatButton(output, strDbPrefix);


            //output.Content.AppendHtml("<br>");
            
            
            //output.Content.AppendHtml("</div>");

        }

        /// <summary>
        /// Interní chat: u projektu, úkolu, kontaktu, vyúčtování a dokumentu tlačítko konverzace k záznamu (s počtem zpráv),
        /// u uživatele tlačítko pro přímou zprávu. Zobrazí se jen tam, kde jsou v databázi tabulky chatu.
        /// </summary>
        private void AppendChatButton(TagHelperOutput output, string strDbPrefix)
        {
            try
            {
                if (this.ViewContext == null) return;
                var f = (BL.Factory)this.ViewContext.HttpContext.RequestServices.GetService(typeof(BL.Factory));
                if (f == null || f.CurrentUser == null || !f.ChatBL.IsAvailable()) return;
                int intPID = BO.Code.Bas.InInt(this.pid);
                if (intPID <= 0) return;
                string strStyle = "float:right;margin-left:4px;position:relative;visibility:hidden;";   //jako ostatní tlačítka: zobrazí se až při najetí myší (_handle_mywrk_mouseover)

                if (strDbPrefix == "p41" || strDbPrefix == "p56" || strDbPrefix == "p28" || strDbPrefix == "p91" || strDbPrefix == "o23")
                {
                    int intCount = f.ChatBL.GetRecordMessagesCount(strDbPrefix, intPID);
                    string strBadge = intCount > 0 ? $"<span class='badge-def'>{(intCount > 99 ? "99+" : intCount.ToString())}</span>" : null;
                    string strKeep = null;
                    if (intCount > 0)
                    {
                        //v konverzaci k záznamu už jsou zprávy: tlačítko je vidět trvale, ne až při najetí myší
                        strStyle = strStyle.Replace("visibility:hidden;", "");
                        strKeep = " data-keep='1'";
                    }
                    output.Content.AppendHtml($"<button id='cmdWrkChat'{strKeep} type='button' class='btn btn-light px-1' style='{strStyle}' title='{f.tra("Chat k záznamu")}' onclick=\"_window_open('/Chat/Index?entity={strDbPrefix}&pid={intPID}',3,'Chat')\"><span class='material-icons-outlined-btn'>forum</span>{strBadge}</button>");
                }
                else if (strDbPrefix == "j02" && intPID != f.CurrentUser.pid)
                {
                    output.Content.AppendHtml($"<button id='cmdWrkChat' type='button' class='btn btn-light px-1' style='{strStyle}' title='{f.tra("Napsat zprávu")}' onclick=\"_window_open('/Chat/Index?j02id={intPID}',3,'Chat')\"><span class='material-icons-outlined-btn'>forum</span></button>");
                }
            }
            catch
            {
                //tlačítko chatu je doplněk: jeho chyba nesmí rozbít záhlaví záznamu
            }
        }
    }
}
