using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("myradiolist")]
    public class myRadioListHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";  //jedna hodnota - string

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("valuefield")]
        public string ValueField { get; set; }  //musí být integer
        
        [HtmlAttributeName("textfield")]
        public string TextField { get; set; }

        [HtmlAttributeName("event_after_changevalue")]
        public string Event_After_ChangeValue { get; set; }

        [HtmlAttributeName("repeat-horizontal")]
        public bool RepeatHorizontal { get; set; }  //vodorovně
        
        [HtmlAttributeName("datasource")]
        public ModelExpression DataSource { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IEnumerable lisDatasource = this.DataSource.Model as IEnumerable;
            if (lisDatasource == null)
            {
                return;
            }
            
            string strModelValue = this.For.Model.ToString() as string;
            
            var sb = new System.Text.StringBuilder();
            

            sb.AppendLine("<ul style='list-style:none;padding-left:0px;'>");
            foreach (var item in lisDatasource)
            {
                try
                {
                    string strText = DataSource.Metadata.ElementMetadata.Properties[this.TextField].PropertyGetter(item).ToString();
                    string strValue = Convert.ToInt32(DataSource.Metadata.ElementMetadata.Properties[this.ValueField].PropertyGetter(item)).ToString();
                    string strChecked = "";

                    if (strModelValue == strValue)
                    {
                        strChecked = "checked";
                    }


                    if (this.RepeatHorizontal)
                    {
                        sb.AppendLine("<li style='display: inline-block;'>");
                    }
                    else
                    {
                        sb.AppendLine("<li style='margin-bottom:4px;'>");
                    }

                    sb.Append($"<input type='radio' class='form-check-input' id='chk{this.For.Name}_{strValue}' name='my{this.For.Name}' onclick='myradiolist_checked(\"{this.For.Name}\",\"{strValue}\",\"{this.Event_After_ChangeValue}\")' {strChecked} />");
                    sb.Append($"<label class='form-check-label'  for='chk{this.For.Name}_{strValue}'>{strText}</label>");


                    sb.AppendLine("</li>");
                }
                catch
                {
                    //nic
                }
                
                
                
            }
            sb.AppendLine("</ul>");

            sb.Append($"<input type='hidden' id='{this.For.Name}' name='{this.For.Name}' value='{strModelValue}' />");

            output.Content.AppendHtml(sb.ToString());



        }
    
    }
}
