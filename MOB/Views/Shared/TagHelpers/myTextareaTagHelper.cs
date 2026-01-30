using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MOB.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mytextarea")]
    public class myTextareaTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }

        [HtmlAttributeName("cssclass")]
        public string CssClass { get; set; }

        [HtmlAttributeName("rows")]
        public int Rows { get; set; }

        [HtmlAttributeName("isdisabled")]
        public bool IsDisabled { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string strControlID = this.For.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");

            output.TagName = "textarea";  // vykreslí <textarea>
            //output.TagMode = TagMode.SelfClosing;

            
            output.Attributes.SetAttribute("id", strControlID);
            output.Attributes.SetAttribute("name", this.For.Name);
            
            if (this.Rows > 0)
            {
                output.Attributes.SetAttribute("rows", this.Rows.ToString());
            }
            if (this.CssClass == null)
            {
                
                output.Attributes.SetAttribute("class", "textarea validator w-full !h14 !min-h-0");
            }
            else
            {
                output.Attributes.SetAttribute("class", $"{this.CssClass} !h14 !min-h-0");
            }
                
            output.Attributes.SetAttribute("placeholder", this.PlaceHolder);

            if (this.For.Model != null)
            {
                output.Content.SetContent(this.For.Model.ToString());
            }
            

        }
    }
}
