
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

            sb(string.Format("<div id='divDropdownContainer{0}' class='dropdown'>", strControlID));

            

            if (this.SearchTextboxWidth > 0)
            {
                sb($"<input type='text' id='cmdCombo{strControlID}' class='form-control' autocomplete='off' data-bs-toggle='dropdown' aria-expanded='false' style='width:{this.SearchTextboxWidth}px;border: solid 1px #C8C8C8; border-radius: 3px;text-align:left;white-space: nowrap;overflow: hidden;text-overflow: ellipsis;' placeholder='{this.PlaceHolder}'/>");

            }
            else
            {
                sb($"<input type='text' id='cmdCombo{strControlID}' class='form-control' autocomplete='off' data-bs-toggle='dropdown' aria-expanded='false' style='width:100%;border: solid 1px #C8C8C8; border-radius: 3px;text-align:left;white-space: nowrap;overflow: hidden;text-overflow: ellipsis;' placeholder='{this.PlaceHolder}'/>");

            }



            //sb($"<input type='text' id='cmdCombo{strControlID}' class='btn dropdown-toggle form-control' autocomplete='off' data-bs-toggle='dropdown' aria-expanded='false' style='width:100%;border: solid 1px #C8C8C8; border-radius: 3px;text-align:left;white-space: nowrap;overflow: hidden;text-overflow: ellipsis;' placeholder='{this.PlaceHolder}'/>");


            sb(string.Format("<button type='button' id='cmdClear{0}' class='btn btn-light' tabindex='-1' title='Vyčistit' style='position:absolute;top:0;right:30px;border: solid 1px #C8C8C8;border-radius:0px;color:#6495ED; font-weight:bold;'>", strControlID));
            sb("<span aria-hidden='true'>&times;</span>");


            //sb(string.Format("<button type='button' id='cmdPop{0}' class='btn dropdown-toggle' tabindex='-1' data-bs-toggle='dropdown' aria-expanded='false' style='color:#6495ED;width:30px;'></button>", strControlID));

            sb(string.Format("<button type='button' id='cmdPop{0}' class='btn btn-light dropdown-toggle' tabindex='-1' data-bs-toggle='dropdown' aria-expanded='false' style='position:absolute;top:0;right:1px;color:#6495ED;width:30px;border:solid 1px #C8C8C8;border-radius:0px !important;'></button>", strControlID));


           

            sb(string.Format("<div class='dropdown-menu p-0 m-0' aria-labelledby='cmdCombo{0}' style='width:100%;'>", strControlID));


            
            sb("");

            if (this.SearchResultWidth == 0)
            {
                sb($"<div id='divData{strControlID}' style='height:{this.ComboHeight}px;overflow:auto;background-color:#E6F0FF;z-index:500;'>");
            }
            else
            {
                sb($"<div id='divData{strControlID}' style='height:{this.ComboHeight}px;overflow:auto;background-color:#E6F0FF;z-index:500;border:solid 1px silver;width:{this.SearchResultWidth}px;'>");
            }

            sb("</div>");


            sb("</div>");   //dropdown-menu
            sb("</div>");   //dropdown


            sb("");

            string strSelectedText = null;
            if (this.SelectedText.Model != null)    //v textu vybrané combo položky vadí apostrofy!!
            {
                strSelectedText = this.SelectedText.Model.ToString();
                if (strSelectedText.Contains("'"))
                {
                    strSelectedText = strSelectedText.Replace("'", "");
                }
            }



            sb(string.Format("<input type='hidden' value='{0}' data-id='text_{1}' name='{2}'/>", strSelectedText, strControlID, this.elementidprefix + this.SelectedText.Name));

            sb(string.Format("<input type='hidden' value ='{0}' id='{1}' data-id='value_{1}' name='{2}'/>", _SelectedValue.ToString(), strControlID, this.elementidprefix + this.For.Name));   //asp-for pro hostitelské view

            sb($"<input type='hidden' id='explicit_columns{strControlID}'/>");  //sloupce pro searchbox

            sb("");
            sb("");
            sb("<script type='text/javascript'>");
            _sb.Append(string.Format("var c{0}=", strControlID));
            _sb.Append("{");
            _sb.Append(string.Format("controlid: '{0}',posturl: '/TheCombo/GetHtml4TheCombo',entity:'{1}',myqueryinline: '{2}',defvalue: '{3}',deftext: '{4}',on_after_change: '{5}',viewflag: '{6}',filterflag: '{7}',placeholder: '{8}',masterprefix:'{9}',masterpid:{10}", strControlID, this.Entity, this.myqueryinline, _SelectedValue.ToString(), strSelectedText, this.Event_After_ChangeValue, this.ViewFlag, this.FilterFlag, this.PlaceHolder, this.masterprefix, this.masterpid));
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