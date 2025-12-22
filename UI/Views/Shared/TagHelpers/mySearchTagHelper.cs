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


            if (this.SearchTextboxWidth > 0)
            {
                sb($"<input type='text' id='cmdCombo{strControlID}' class='xbtn xdropdown-toggle form-control' autocomplete='off' data-bs-toggle='dropdown' aria-expanded='false' style='width:{this.SearchTextboxWidth}px;border: solid 1px #C8C8C8; border-radius: 3px;text-align:left;white-space: nowrap;overflow: hidden;text-overflow: ellipsis;' placeholder='{this.PlaceHolder}'/>");

            }
            else
            {
                sb($"<input type='text' id='cmdCombo{strControlID}' class='xbtn xdropdown-toggle form-control' autocomplete='off' data-bs-toggle='dropdown' aria-expanded='false' style='width:100%;border: solid 1px #C8C8C8; border-radius: 3px;text-align:left;white-space: nowrap;overflow: hidden;text-overflow: ellipsis;' placeholder='{this.PlaceHolder}'/>");

            }



         

          

            if (this.SearchResultMarginLeft == 0)
            {
                sb(string.Format("<div class='dropdown-menu p-0 m-0' aria-labelledby='cmdCombo{0}' style='width:100%;'>", strControlID));

            }
            else
            {
                sb($"<div class='dropdown-menu p-0 m-0' aria-labelledby='cmdCombo{strControlID}' style='width:100%;margin-left:{this.SearchResultMarginLeft}px !important;'>");

            }




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
            _sb.Append($"controlid: '{strControlID}',posturl: '/TheSearch/GetHtml4TheSearch',defvalue: '{_SelectedValue}',deftext: '{strSelectedText}',on_after_change: '{this.Event_After_ChangeValue}',viewflag: '1',placeholder: '{this.PlaceHolder}'");
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
