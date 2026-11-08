
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mycombochecklist")]
    public class myComboCheckListTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("entity")]
        public string Entity { get; set; }

        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }

        [HtmlAttributeName("placeholder_icon")]
        public string PlaceholderIcon { get; set; }

        [HtmlAttributeName("masterpid")]
        public int masterpid { get; set; }

        [HtmlAttributeName("masterprefix")]
        public string masterprefix { get; set; }

        [HtmlAttributeName("myqueryinline")]
        public string myqueryinline { get; set; }

        [HtmlAttributeName("selectedtext")]
        public ModelExpression SelectedText { get; set; }

        [HtmlAttributeName("dropdown-height")]
        public string dropdown_height { get; set; }

        [HtmlAttributeName("event_after_changevalue")]
        public string Event_After_ChangeValue { get; set; }

        [HtmlAttributeName("elementid-prefix")]
        public string elementidprefix { get; set; } //použitelné v situaci taghelperu v listu, který je umístěn v partial view komponentě

        

        private System.Text.StringBuilder _sb;
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;


            _sb = new System.Text.StringBuilder();
            string strSelectedValues = this.For.Model as string;
            if (this.dropdown_height == null)
            {
                this.dropdown_height = "320px";
            }
            

            string strControlID = this.For.Name;
            string strControlName = this.For.Name;
            if (this.elementidprefix != null)
            {
                strControlID = this.elementidprefix + strControlID;
                strControlName = this.elementidprefix + this.For.Name;
            }
            strControlID = strControlID.Replace(".", "_").Replace("[", "_").Replace("]", "_");



            string aliasid = this.SelectedText.Name;
            string aliasname = this.SelectedText.Name;
            if (this.elementidprefix != null)
            {
                aliasid = this.elementidprefix + aliasid;
                aliasname = this.elementidprefix + aliasname;
            }
            aliasid = aliasid.Replace(".", "_").Replace("[", "_").Replace("]", "_");

            //vzhled: třídy mcl-* (site.css); ID prvků zůstávají (cmdCombo*, divDropdown*, divData*, skryté hodnoty)
            sb($"<div id='divDropdownContainer{strControlID}' class='dropdown mcl' style='max-width:300px;'>");
            sb($"<button type='button' id='cmdCombo{strControlID}' class='form-control mcl-button dropdown-toggle' data-bs-toggle='dropdown' data-bs-auto-close='outside' aria-expanded='false'>");
            if (this.PlaceholderIcon != null)
            {
                sb($"<span class='material-icons-outlined-nosize'>{Html(this.PlaceholderIcon)}</span>");
            }
            sb($"<span class='mcl-text'>{Html(this.PlaceHolder)}</span><span class='mcl-badge' style='display:none;'></span>");
            sb("</button>");

            sb($"<div id='divDropdown{strControlID}' class='dropdown-menu mcl-menu p-0' aria-labelledby='cmdCombo{strControlID}' tabindex='-1'>");
            sb("<div class='mcl-tools'>");
            sb("<input type='search' class='form-control form-control-sm mcl-search' placeholder='Hledat…' autocomplete='off' />");
            sb("<div class='mcl-tools-row'>");
            sb("<button type='button' class='btn btn-sm btn-light mcl-all' title='Zaškrtnout vše zobrazené'>Vše</button>");
            sb("<button type='button' class='btn btn-sm btn-light mcl-none' title='Odškrtnout vše zobrazené'>Nic</button>");
            sb("<label class='mcl-onlysel'><input type='checkbox' class='mcl-onlysel-chk' /> jen vybrané</label>");
            sb("<span class='mcl-count'></span>");
            sb("</div></div>");
            sb($"<div id='divData{strControlID}' class='mcl-data' style='max-height:{Html(this.dropdown_height)};'></div>");
            sb("</div>");   //dropdown-menu
            sb("</div>");   //dropdown

            sb($"<input type='hidden' id='{aliasid}' name='{Html(aliasname)}' value='{Html(this.SelectedText.Model?.ToString())}'/>");

            sb("<script type='text/javascript'>");
            _sb.Append($"var c{strControlID}={{");
            _sb.Append($"controlid: '{strControlID}',posturl: '/TheCombo/GetHtml4Checkboxlist',entity:'{Js(this.Entity)}',masterprefix:'{Js(this.masterprefix)}',masterpid:{this.masterpid},selectedvalues:'{Js(strSelectedValues)}',myqueryinline:'{Js(this.myqueryinline)}',on_after_change:'{Js(this.Event_After_ChangeValue)}',placeholder:'{Js(this.PlaceHolder)}',aliasid:'{aliasid}'");
            _sb.Append("};");
            sb("");
            sb($"mycombochecklist_init(c{strControlID});");
            sb("</script>");

            sb($"<input type='hidden' id='{strControlID}' name='{Html(strControlName)}' value=\"{Html(strSelectedValues)}\" />");

            output.Content.AppendHtml(_sb.ToString());
        }

        private static string Html(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");
        private static string Js(string s) => System.Web.HttpUtility.JavaScriptStringEncode(s ?? "");

        private void sb(string s)
        {
            _sb.AppendLine(s);

        }
    }
}
