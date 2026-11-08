
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mycombo")]
    public class myComboTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("selectedtext")]
        public ModelExpression SelectedText { get; set; }

        [HtmlAttributeName("entity")]
        public string Entity { get; set; }

        [HtmlAttributeName("event_after_changevalue")]
        public string Event_After_ChangeValue { get; set; }

        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }

        public string myqueryinline { get; set; }

        [HtmlAttributeName("masterpid")]
        public int masterpid { get; set; }

        [HtmlAttributeName("masterprefix")]
        public string masterprefix { get; set; }

        public int ViewFlag { get; set; }

        [HtmlAttributeName("filter-flag")]
        public string FilterFlag { get; set; }

        [HtmlAttributeName("search-textbox-width")]
        public int SearchTextboxWidth { get; set; }

        [HtmlAttributeName("search-result-width")]
        public int SearchResultWidth { get; set; }

        private int _SelectedValue { get; set; }

        [HtmlAttributeName("elementid-prefix")]
        public string elementidprefix { get; set; } //použitelné v situaci taghelperu v listu, který je umístěn v partial view komponentě

        [HtmlAttributeName("disponibleflag")]
        public string MyDisponibleFlag { get; set; } //pokud true, nabídka záznamů se filtruje na pouze přístupné záznamy


        [HtmlAttributeName("combo-height")]
        public int ComboHeight { get; set; }

        private System.Text.StringBuilder _sb;
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            if (this.ComboHeight <= 0)
            {
                this.ComboHeight = 220;
            }

            if (this.For.Model != null)
            {
                _SelectedValue = Convert.ToInt32(this.For.Model);

            }
            else
            {
                _SelectedValue = 0;
            }
            if (string.IsNullOrEmpty(this.FilterFlag))
            {
                this.FilterFlag = "0";
            }
            _sb = new System.Text.StringBuilder();

            var strControlID = this.For.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");

            if (this.elementidprefix == null)
            {
                this.elementidprefix = "";
            }

            string strSelectedText = this.SelectedText?.Model?.ToString();
            string strWidth = this.SearchTextboxWidth > 0 ? $"{this.SearchTextboxWidth}px" : "100%";
            string attrPlaceholder = Html(this.PlaceHolder);

            //vzhled: třídy mtc-* (site.css); ID prvků zůstávají kvůli kódu stránek (cmdCombo*, cmdClear*, cmdPop*, divData*, data-id value_/text_)
            sb($"<div id='divDropdownContainer{strControlID}' class='dropdown mtc'>");
            sb($"<input type='text' id='cmdCombo{strControlID}' class='form-control mtc-input' autocomplete='off' data-bs-toggle='dropdown' data-bs-auto-close='outside' aria-expanded='false' style='width:{strWidth};' placeholder='{attrPlaceholder}'/>");
            sb($"<button type='button' id='cmdClear{strControlID}' class='mtc-clear' tabindex='-1' title='Vyčistit'><span aria-hidden='true'>&times;</span></button>");
            sb($"<button type='button' id='cmdPop{strControlID}' class='mtc-pop dropdown-toggle' tabindex='-1' data-bs-toggle='dropdown' data-bs-auto-close='outside' aria-expanded='false' title='Zobrazit nabídku'></button>");

            sb($"<div class='dropdown-menu mtc-menu p-0 m-0' aria-labelledby='cmdCombo{strControlID}' style='width:100%;'>");
            //výška nabídky podle obsahu, combo-height = maximum
            string strDataWidth = this.SearchResultWidth > 0 ? $"width:{this.SearchResultWidth}px;" : "";
            sb($"<div id='divData{strControlID}' class='mtc-data' style='max-height:{this.ComboHeight}px;{strDataWidth}'></div>");
            sb("</div>");   //dropdown-menu
            sb("</div>");   //dropdown

            sb($"<input type='hidden' value='{Html(strSelectedText)}' data-id='text_{strControlID}' name='{Html(this.elementidprefix + this.SelectedText.Name)}'/>");
            sb($"<input type='hidden' value='{_SelectedValue}' id='{strControlID}' data-id='value_{strControlID}' name='{Html(this.elementidprefix + this.For.Name)}'/>");   //asp-for pro hostitelské view
            sb($"<input type='hidden' id='explicit_columns{strControlID}'/>");  //sloupce pro searchbox

            //texty do JS bezpečně (apostrofy, uvozovky, </script>)
            sb("<script type='text/javascript'>");
            _sb.Append($"var c{strControlID}={{");
            _sb.Append($"controlid: '{strControlID}',posturl: '/TheCombo/GetHtml4TheCombo',entity:'{Js(this.Entity)}',myqueryinline: '{Js(this.myqueryinline)}',defvalue: '{_SelectedValue}',deftext: '{Js(strSelectedText)}',on_after_change: '{Js(this.Event_After_ChangeValue)}',viewflag: '{this.ViewFlag}',filterflag: '{Js(this.FilterFlag)}',placeholder: '{Js(this.PlaceHolder)}',masterprefix:'{Js(this.masterprefix)}',masterpid:{this.masterpid}");
            _sb.Append("};");
            sb("");
            sb($"mycombo_init(c{strControlID})");
            sb("</script>");

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