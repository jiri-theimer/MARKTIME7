using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mytooltip")]
    public class myTooltipTagHelper : TagHelper
    {
        [HtmlAttributeName("text")]
        public string Tooltip { get; set; }

        [HtmlAttributeName("position")]
        public string Position { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(this.Position))
            {
                this.Position = "top";
            }
            
            output.Content.AppendHtml("<a class='tooltipx'>?");
            output.Content.AppendHtml($"<span class='tooltiptext'>{this.Tooltip}</span>");
            output.Content.AppendHtml("</a>");
            //output.Content.AppendHtml($"<a data-toggle='tooltip' data-placement='{this.Position}' title='{this.Tooltip}'>");
            //output.Content.AppendHtml("<span class='material-icons-outlined-nosize' style='font-size:115%;color:blue;cursor:help;'>help</span>");
            //output.Content.AppendHtml("</a>");
        }
    }
}
