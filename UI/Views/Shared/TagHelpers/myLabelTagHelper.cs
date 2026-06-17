using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;


namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mylabel")]
    public class myLabelTagHelper : TagHelper
    {
        

        [HtmlAttributeName("text")]
        public string LabelText { get; set; }

        

        [HtmlAttributeName("tooltip")]
        public string Tooltip { get; set; }

        [HtmlAttributeName("explicit-for")]
        public string ExplicitFor { get; set; }

        [HtmlAttributeName("bez-dvojtecky")]
        public bool IsBezDvojtecky { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var classAttr = output.Attributes["class"];
            if (classAttr != null)
            {
                output.Content.AppendHtml($"<label class='{classAttr.Value}'");

            }
            else
            {
                output.Content.AppendHtml($"<label");
            }


            if (!this.IsBezDvojtecky)
            {
                this.LabelText = $"{this.LabelText}:";
            }

            if (this.ExplicitFor != null)
            {
                output.Content.AppendHtml($" for='{this.ExplicitFor}'");
            }

            var styleAttr = output.Attributes["style"];
            if (styleAttr != null)
            {
                output.Content.AppendHtml($" style='{styleAttr.Value}'");
            }

            output.Content.AppendHtml(">");

            if (!string.IsNullOrEmpty(this.Tooltip))
            {
                output.Content.AppendHtml($"<span class='mylabelhelp'>{this.LabelText}");

                output.Content.AppendHtml($"<span class='mylabelhelptext' style='color:black'>{this.Tooltip}</span></span>");
            }
            else
            {
                output.Content.AppendHtml($"{this.LabelText}");
            }

            

            output.Content.AppendHtml("</label>");

            output.TagName = null;

        }
    }
}
