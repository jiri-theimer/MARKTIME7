using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MOB.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mynumber")]
    public class myNumberTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("placeholder")]
        public string Placeholder { get; set; }

        [HtmlAttributeName("autocomplete")]
        public string Autocomplete { get; set; }

        [HtmlAttributeName("min")]
        public int NumberMin { get; set; } = -999999999;

        [HtmlAttributeName("max")]
        public int NumberMax { get; set; } = 999999999;

        [HtmlAttributeName("decimals")]
        public int Decimals { get; set; } = 2;


        [HtmlAttributeName("isrequired")]
        public bool IsRequired { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string strControlID = this.For.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");

           
            output.TagName = "input";  // vykreslí <input>
            output.TagMode = TagMode.SelfClosing;

            output.Attributes.SetAttribute("type", "text");
            output.Attributes.SetAttribute("id", $"number-{strControlID}");
            //output.Attributes.SetAttribute("name", this.For.Name);
            output.Attributes.SetAttribute("value", this.For.Model ?? "");
            output.Attributes.SetAttribute("class", "input w-full");
            output.Attributes.SetAttribute("placeholder", this.Placeholder);
            if (!string.IsNullOrEmpty(this.Autocomplete))
            {
                output.Attributes.SetAttribute("autocomplete", this.Autocomplete);
            }

            if (this.IsRequired)
            {
                output.Attributes.SetAttribute("required", "required");
            }

            output.PostElement.AppendHtml($@"<input type='hidden'  id='{strControlID}'  name='{this.For.Name}' value='{this.For.Model ?? 0}' />");


            output.PostElement.AppendHtml($"<span id='error-msg-{strControlID}' class='mt-1 min-h-[1.25rem] text-sm text-red-500'></span>");

            output.PostElement.AppendHtml("<script type='text/javascript'>");

            string strDefValJs = (this.For.Model ?? 0).ToString().Replace(",", ".");
            output.PostElement.AppendHtml($"_number_init('{strControlID}', {strDefValJs}, {this.NumberMin}, {this.NumberMax},{this.Decimals});");

            output.PostElement.AppendHtml("</script>");
        }
    }
}
