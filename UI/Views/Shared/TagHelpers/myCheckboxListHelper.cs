
using System.Collections;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mycheckboxlist")]
    public class myCheckboxListHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }    

        [HtmlAttributeName("valuefield")]
        public string ValueField { get; set; }  //musí být integer
        [HtmlAttributeName("textfield")]
        public string TextField { get; set; }

        [HtmlAttributeName("groupfield")]
        public string GroupField { get; set; }

        [HtmlAttributeName("datasource")]
        public ModelExpression DataSource { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IEnumerable lisDatasource = this.DataSource.Model as IEnumerable;
            if (lisDatasource == null)
            {
                return;
            }
            IEnumerable<int> lisModel = this.For.Model as IEnumerable<int>;
            
            //var strControlID = this.For.Name.Replace(".", "_");
            var sb = new System.Text.StringBuilder();
            string strSeletedValues = "";
            string strLastGroup = "";
            int x = 0;
            if (lisModel != null)
            {
                strSeletedValues = String.Join(",", lisModel);
            }

            sb.AppendLine("<ul style='list-style:none;padding-left:0px;'>");
            foreach (var item in lisDatasource)
            {
                
                string strText = DataSource.Metadata.ElementMetadata.Properties[this.TextField].PropertyGetter(item).ToString();
                string strGroup = "";
                if (this.GroupField !=null)
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
                        sb.AppendLine("<li>");
                        
                        sb.AppendLine("<div style='font-weight:bold;background-color:#ADD8E6;'><span style='padding-left:10px;'>" + strGroup+"</span></div>");
                        sb.AppendLine("</li>");
                    }
                }
                
                int intValue = Convert.ToInt32(DataSource.Metadata.ElementMetadata.Properties[this.ValueField].PropertyGetter(item));
                string strChecked = "";                
                if (lisModel !=null && lisModel.Where(p => p == intValue).Count() > 0)
                {
                    strChecked = "checked";
                }

                
                sb.AppendLine("<li>");
                sb.Append(string.Format("<input type='checkbox' id='chk{0}_{1}' onclick='mycheckboxlist_checked(this,\"{0}_{1}\",{1})' {2} />", this.For.Name, intValue,strChecked));
                sb.Append(string.Format("<label style='min-width:200px;' for='chk{0}_{1}'>{2}</label>", this.For.Name, intValue, strText));
                if (strChecked == "checked")
                {
                    sb.Append(string.Format("<input type='hidden' id='{0}_{1}' name='{0}' value='{1}' />", this.For.Name, intValue));

                }
                else
                {
                    sb.Append(string.Format("<input type='hidden' id='{0}_{1}' name='{0}' value='0' />", this.For.Name, intValue));

                }

                

                sb.AppendLine("</li>");
                strLastGroup = strGroup;
                x += 1;
            }
            sb.AppendLine("</ul>");
            

            output.Content.AppendHtml(sb.ToString());



        }
    }
}
