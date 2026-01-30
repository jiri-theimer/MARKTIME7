using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace MOB.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mydatepicker")]
    public class myDatePickerTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }

        [HtmlAttributeName("autocomplete")]
        public string AutoCompleteOnOff { get; set; }

        [HtmlAttributeName("isrequired")]
        public bool IsRequired { get; set; }

        [HtmlAttributeName("isdisabled")]
        public bool IsDisabled { get; set; }

        [HtmlAttributeName("isinit-js-auto")] //true: automaticky se spouští js: _mydatepicker_init
        public bool IsInitJsAuto { get; set; } = true;

        [HtmlAttributeName("event-onchange")]
        public string Event_OnChange { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string strControlID = this.For.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");
            if (this.PlaceHolder == null)
            {
                this.PlaceHolder = "dd.mm.yyyy";
            }
            output.TagName = "input";  // vykreslí <input>
            output.TagMode = TagMode.SelfClosing;

            output.Attributes.SetAttribute("type", "text");
            output.Attributes.SetAttribute("id", strControlID);
            output.Attributes.SetAttribute("name", this.For.Name);
            output.Attributes.SetAttribute("value", this.For.Model ?? "");
            output.Attributes.SetAttribute("class", "input input-bordered validator w-[130px]");
            output.Attributes.SetAttribute("placeholder", this.PlaceHolder);
            output.Attributes.SetAttribute("autocomplete", "off");

            if (this.IsRequired)
            {
                output.Attributes.SetAttribute("required", "required");
            }

            if (this.IsDisabled)
            {
                output.Attributes.SetAttribute("disabled", "disabled");
                output.Attributes.SetAttribute("id", null);
                output.Attributes.SetAttribute("name", null);
            }

            // Přidej i validaci (musí být mimo input)
            if (this.IsRequired)
            {
                output.PostElement.AppendHtmlLine($"<span asp-validation-for='{this.For.Name}' class='text-danger'></span>");
            }
            


            if (this.IsDisabled)
            {
                output.PostElement.AppendHtmlLine($"<input type='hidden' name='{this.For.Name}' id='{strControlID}' value='{this.For.Model ?? ""}' />");

            }
            if (this.IsInitJsAuto)
            {
                output.PostElement.AppendHtmlLine("<script type='text/javascript'>");
                if (this.Event_OnChange != null)
                {
                    output.PostElement.AppendHtmlLine($"_mydatepicker_init('{strControlID}',() => {this.Event_OnChange});");
                }
                else
                {
                    output.PostElement.AppendHtmlLine($"_mydatepicker_init('{strControlID}');");
                }
                output.PostElement.AppendHtmlLine("</script>");
            }
            
        }
    }
}
