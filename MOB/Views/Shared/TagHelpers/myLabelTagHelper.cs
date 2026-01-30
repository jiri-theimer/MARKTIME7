using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MOB.Views.Shared.TagHelpers
{
    [HtmlTargetElement("mylabel")]
    public class MyLabelTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("text")]
        public string LabelText { get; set; }

        [HtmlAttributeName("cssclass")]
        public string LabelCssClass { get; set; }

        [HtmlAttributeName("tooltip")]
        public string Tooltip { get; set; }

        [HtmlAttributeName("fixwidth")]
        public int FixedWidth { get; set; }

        [HtmlAttributeName("isright")]
        public bool IsAllignRight { get; set; } //zarovnání doprava

        

        [HtmlAttributeName("for")]
        public string ExplicitFor { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {            
            if (this.LabelCssClass == null)
            {
                this.LabelCssClass = "text-base-content/60";    //text-base-content/60
            }
            if (this.FixedWidth>0)
            {
                this.LabelCssClass = $"{this.LabelCssClass} w-[{this.FixedWidth}px]";
            }
            if (this.IsAllignRight)
            {
                this.LabelCssClass = $"{this.LabelCssClass} sm:text-right";
            }
            

            if (this.For != null)
            {
                if (this.For.Name.Substring(this.For.Name.Length-2,2) =="ID")
                {
                    output.Content.AppendHtml($"<label class='{this.LabelCssClass}' for='combo-input-{this.For.Name.Replace(".", "_")}'>");
                }
                else
                {
                    output.Content.AppendHtml($"<label class='{this.LabelCssClass}' for='{this.For.Name.Replace(".", "_")}'>");
                }
                    
            }
            else
            {
                if (this.ExplicitFor != null)
                {
                    output.Content.AppendHtml($"<label class='{this.LabelCssClass}' for='{this.ExplicitFor}'>");
                }
                else
                {
                    output.Content.AppendHtml($"<label class='{this.LabelCssClass}'>");
                }
            }

            if (!string.IsNullOrEmpty(this.Tooltip))
            {
                
                output.Content.AppendHtml($"<span class='mytooltip static'>{this.LabelText}:");
                
                output.Content.AppendHtml($"<span class='mytooltiptext static bg-blue-100 text-[black]'>{this.Tooltip}</span></span>");
            }
            else
            {
                output.Content.AppendHtml($"{this.LabelText}:");
            }


            output.Content.AppendHtml("</label>");

            output.TagName = null;
        }
    }
}
