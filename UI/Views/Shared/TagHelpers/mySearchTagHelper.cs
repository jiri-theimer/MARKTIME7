using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mySearch")]
    public class mySearchTagHelper : TagHelper
    {
        //private const string ForAttributeName = "asp-for";

        //[HtmlAttributeName(ForAttributeName)]
        //public ModelExpression For { get; set; }

        //[HtmlAttributeName("selectedtext")]
        //public ModelExpression SelectedText { get; set; }

        [HtmlAttributeName("elementid-prefix")]
        public string elementidprefix { get; set; } //použitelné v situaci taghelperu v listu, který je umístěn v partial view komponentě


        [HtmlAttributeName("event_after_changevalue")]
        public string Event_After_ChangeValue { get; set; }

        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }

        

        [HtmlAttributeName("search-textbox-width")]
        public int SearchTextboxWidth { get; set; }

        [HtmlAttributeName("search-result-width")]
        public int SearchResultWidth { get; set; }

        [HtmlAttributeName("search-result-marginleft")]
        public int SearchResultMarginLeft { get; set; }

        private int _SelectedValue { get; set; }

        

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

            //if (this.For.Model != null)
            //{
            //    _SelectedValue = Convert.ToInt32(this.For.Model);

            //}
            //else
            //{
            //    _SelectedValue = 0;
            //}
            
            _sb = new System.Text.StringBuilder();

            //var strControlID = this.For.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");

            var strControlID = "mySearch1";



            sb(string.Format("<div id='divDropdownContainer{0}' class='dropdown'>", strControlID));


            //vzhled ms-* v site.css (pole v horní liště + nabídka výsledků)
            string strWidth = this.SearchTextboxWidth > 0 ? $"{this.SearchTextboxWidth}px" : "100%";
            sb($"<input type='search' id='cmdCombo{strControlID}' class='form-control ms-input' autocomplete='off' spellcheck='false' data-bs-toggle='dropdown' data-bs-auto-close='outside' aria-expanded='false' style='width:{strWidth};' placeholder='{System.Net.WebUtility.HtmlEncode(this.PlaceHolder)}'/>");

            if (this.SearchResultMarginLeft == 0)
            {
                sb($"<div class='dropdown-menu p-0 m-0 ms-menu' aria-labelledby='cmdCombo{strControlID}'>");
            }
            else
            {
                sb($"<div class='dropdown-menu p-0 m-0 ms-menu' aria-labelledby='cmdCombo{strControlID}' style='margin-left:{this.SearchResultMarginLeft}px !important;'>");
            }
            string strResWidth = this.SearchResultWidth > 0 ? $"width:{this.SearchResultWidth}px;" : "";
            sb($"<div id='divData{strControlID}' class='ms-data' style='max-height:{this.ComboHeight}px;{strResWidth}'>");
            sb("</div>");


            sb("</div>");   //dropdown-menu
            sb("</div>");   //dropdown


            sb("");

            string strSelectedText = null;
            //if (this.SelectedText.Model != null)    //v textu vybrané combo položky vadí apostrofy!!
            //{
            //    strSelectedText = this.SelectedText.Model.ToString();
            //    if (strSelectedText.Contains("'"))
            //    {
            //        strSelectedText = strSelectedText.Replace("'", "");
            //    }
            //}



            sb(string.Format("<input type='hidden' value='{0}' data-id='text_{1}' name='{2}mySearch1SelectedText'/>", strSelectedText, strControlID, this.elementidprefix));

            sb(string.Format("<input type='hidden' value ='{0}' id='{1}' data-id='value_{1}' name='{2}mySearch1For'/>", _SelectedValue.ToString(), strControlID, this.elementidprefix));   //asp-for pro hostitelské view

            sb($"<input type='hidden' id='explicit_columns{strControlID}'/>");  //sloupce pro searchbox

            sb("");
            sb("");
            sb("<script type='text/javascript'>");
            _sb.Append(string.Format("var c{0}=", strControlID));
            _sb.Append("{");            
            _sb.Append($"controlid: '{strControlID}',posturl: '/TheSearch/GetHtml4TheSearch',defvalue: '{_SelectedValue}',deftext: '',on_after_change: '{System.Web.HttpUtility.JavaScriptStringEncode(this.Event_After_ChangeValue)}',viewflag: '1',placeholder: '{System.Web.HttpUtility.JavaScriptStringEncode(this.PlaceHolder)}'");
            _sb.Append("};");
            sb("");
            sb("");
            sb(string.Format("mycombo_init(c{0})", strControlID));

            sb("</script>");

            output.Content.AppendHtml(_sb.ToString());
        }

        private void sb(string s)
        {
            _sb.AppendLine(s);

        }

    }
}
