
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
                this.dropdown_height = "220px";
            }
            

            string strControlID = this.For.Name;
            string strControlName = this.For.Name;
            if (this.elementidprefix != null)
            {
                strControlID = this.elementidprefix + strControlID;
                strControlName = this.elementidprefix + this.For.Name;
            }
            strControlID = strControlID.Replace(".", "_").Replace("[", "_").Replace("]", "_");



            sb($"<div id='divDropdownContainer{strControlID}' class='dropdown' style='max-width:300px;'>");


            //string strTitle = "";
            //if (this.SelectedText.Model != null)
            //{
            //    strTitle=Convert.ToString(this.SelectedText.Model).Replace(",", "★");
            //}


            
            //sb("<div class='input-group-append'>");
            sb(string.Format("<button type='button' id='cmdCombo{0}' class='btn dropdown-toggle form-control' data-bs-toggle='dropdown' aria-expanded='false' style='border: solid 1px #C8C8C8; border-radius: 3px;width:100%;text-align:left;white-space: nowrap;overflow: hidden;text-overflow: ellipsis;'>", strControlID));
            if (this.PlaceholderIcon != null)
            {
                sb($"<span class='material-icons-outlined-nosize'>{this.PlaceholderIcon}</span>");
            }
            
            if (this.PlaceHolder != null)
            {
                sb(this.PlaceHolder);

            }
            sb("</button>");

            sb(string.Format("<div id='divDropdown{0}' class='dropdown-menu' aria-labelledby='cmdCombo{0}' style='width:100%;min-width:180px;' tabindex='-1'>", strControlID));
            sb(string.Format("<div id='divData{0}' style='height:{1};overflow:auto;width:100%;min-width:180px;'>", strControlID,this.dropdown_height));
            sb("</div>");           
            sb("</div>");
            
            sb("");            
            sb("");

            
            sb("</div>");

            string aliasid = this.SelectedText.Name;
            string aliasname = this.SelectedText.Name;
            if (this.elementidprefix != null)
            {
                aliasid = this.elementidprefix + aliasid;
                aliasname = this.elementidprefix + aliasname;
            }
            aliasid = aliasid.Replace(".", "_").Replace("[", "_").Replace("]", "_");
           
            sb($"<input type='hidden' id='{aliasid}' name='{aliasname}' value='{this.SelectedText.Model}'/>");

            

            sb("<script type='text/javascript'>");
            sb("");
            _sb.Append($"var c{strControlID}=");
            _sb.Append("{");
            
            _sb.Append($"controlid: '{strControlID}',posturl: '/TheCombo/GetHtml4Checkboxlist',entity:'{this.Entity}',masterprefix:'{this.masterprefix}',masterpid:{this.masterpid},selectedvalues:'{strSelectedValues}',myqueryinline:'{this.myqueryinline}',on_after_change:'{this.Event_After_ChangeValue}',placeholder:'{this.PlaceHolder}',aliasid:'{aliasid}'");

            _sb.Append("};");

            sb("");
            sb($"mycombochecklist_init(c{strControlID});");

            
            sb("");
            sb("</script>");


            sb($"<input type='hidden' id='{strControlID}' name='{strControlName}' value=\"{strSelectedValues}\" />");




            output.Content.AppendHtml(_sb.ToString());
        }

        private void sb(string s)
        {
            _sb.AppendLine(s);

        }
    }
}
