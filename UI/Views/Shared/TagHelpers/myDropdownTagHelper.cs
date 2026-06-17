using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using System.Collections;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Data.SqlClient;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mydropdown")]
    public class myDropdownTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("valuefield")]
        public string ValueField { get; set; }  //může být i string
        [HtmlAttributeName("textfield")]
        public string TextField { get; set; }

        [HtmlAttributeName("isfirstemptyrow")]
        public bool IsFirstEmptyRow { get; set; }
        [HtmlAttributeName("firstemptyrowtext")]
        public string FirstEmptyRowText { get; set; }
        [HtmlAttributeName("firstemptyrowvalue")]
        public string FirstEmptyRowValue { get; set; }

        [HtmlAttributeName("ismultiple")]
        public bool IsMultiple { get; set; }

        [HtmlAttributeName("event_after_changevalue")]
        public string Event_After_ChangeValue { get; set; }

        [HtmlAttributeName("datavalue")]
        public string DataValue { get; set; }

        [HtmlAttributeName("groupfield")]
        public string GroupField { get; set; }

        [HtmlAttributeName("cssfield")]
        public string CssField { get; set; }

        [HtmlAttributeName("bgcolor")]
        public string BgColor { get; set; }


        [HtmlAttributeName("tabindex")]
        public int TabIndex { get; set; }

        [HtmlAttributeName("datasource")]
        public ModelExpression DataSource { get; set; }

        [HtmlAttributeName("elementid-prefix")]
        public string elementidprefix { get; set; } //použitelné v situaci taghelperu v listu, který je umístěn v partial view komponentě

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IEnumerable lisDatasource = this.DataSource.Model as IEnumerable;
            if (lisDatasource == null)
            {
                return;
            }

            string strSelectedValue = "";
            string strLastGroup = "";
            

            if (this.For.Model != null)
            {
                strSelectedValue=Convert.ToString(this.For.Model);
            }
            if (this.elementidprefix == null)
            {
                this.elementidprefix = "";
            }
            var sb = new System.Text.StringBuilder();

            var strControlID = this.For.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");

            sb.AppendLine($"<select class='form-select' id='{strControlID}' name='{this.elementidprefix+this.For.Name}'");
            if (this.IsMultiple)
            {
                sb.Append(" multiple");
            }
            if (this.TabIndex != 0)
            {
                sb.Append($" tabindex={this.TabIndex}");
            }
            if (this.Event_After_ChangeValue != null)
            {
                sb.Append($" onchange='{this.Event_After_ChangeValue}(this)'");
            }
            if (this.DataValue != null)
            {
                sb.Append($" data-value='{this.DataValue}'");
            }
            if (this.BgColor != null)
            {
                sb.Append($" style='background-color:{this.BgColor}'");
            }
            sb.Append(">");
            if (this.IsFirstEmptyRow)
            {
                if (this.FirstEmptyRowValue == null)
                {
                    sb.AppendLine("<option> " + this.FirstEmptyRowText + "</option>");
                }
                else
                {
                    sb.AppendLine("<option value='" + this.FirstEmptyRowValue + "'> " + this.FirstEmptyRowText + "</option>");
                }                
            }
            foreach (var item in lisDatasource)
            {
                string strGroup = "";
                if (this.GroupField != null)
                {
                    if (DataSource.Metadata.ElementMetadata.Properties[this.GroupField].PropertyGetter(item) == null)
                    {
                        strGroup = "";
                    }
                    else
                    {
                        strGroup = DataSource.Metadata.ElementMetadata.Properties[this.GroupField].PropertyGetter(item).ToString();
                    }

                    if (strGroup != strLastGroup)
                    {
                        sb.AppendLine($"<option disabled style='background-color:silver;'>{strGroup}</option>");

                    }
                }

                string strText = DataSource.Metadata.ElementMetadata.Properties[this.TextField].PropertyGetter(item).ToString();
                string strValue = Convert.ToString(DataSource.Metadata.ElementMetadata.Properties[this.ValueField].PropertyGetter(item));
                
                if (strSelectedValue == strValue)
                {
                    sb.Append($"<option value='{strValue}' selected");
                }
                else
                {
                    sb.Append($"<option value='{strValue}'");
                }
                if (this.CssField != null && DataSource.Metadata.ElementMetadata.Properties[this.CssField].PropertyGetter(item) != null)
                {
                    sb.Append(" class='"+DataSource.Metadata.ElementMetadata.Properties[this.CssField].PropertyGetter(item).ToString()+"'");
                }
                sb.Append(">");
                sb.Append(strText);
                sb.Append("</option>");

                strLastGroup = strGroup;                
            }

            sb.AppendLine("</select>");
            output.Content.AppendHtml(sb.ToString());

        }
    }
}
