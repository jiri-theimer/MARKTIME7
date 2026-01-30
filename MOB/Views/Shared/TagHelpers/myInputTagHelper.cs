using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MOB.Views.Shared.TagHelpers
{
    [HtmlTargetElement("myinput")]
    public class MyInputTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }

        [HtmlAttributeName("type")]
        public string InputType { get; set; } = "text";

        

        [HtmlAttributeName("isdisabled")]
        public bool IsDisabled { get; set; }

        [HtmlAttributeName("autocomplete")]
        public string AutoCompleteOnOff { get; set; }

        [HtmlAttributeName("isrequired")]
        public bool IsRequired { get; set; }

        [HtmlAttributeName("number-min")]
        public int? NumberMin { get; set; }

        [HtmlAttributeName("number-max")]
        public int? NumberMax { get; set; }

        [HtmlAttributeName("number-step")]
        public int NumberStep { get; set; }

        [HtmlAttributeName("cssclass")]
        public string InputCssClass { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string strControlID = this.For.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");

            output.TagName = "input";  // vykreslí <input>
            output.TagMode = TagMode.SelfClosing;

            output.Attributes.SetAttribute("type", this.InputType);
            output.Attributes.SetAttribute("id", strControlID);
            output.Attributes.SetAttribute("name", this.For.Name);
            output.Attributes.SetAttribute("value", this.For.Model ?? "");
            if (this.InputCssClass == null)
            {
                output.Attributes.SetAttribute("class", "input validator w-full");
            }
            else
            {
                output.Attributes.SetAttribute("class", this.InputCssClass);
            }

                output.Attributes.SetAttribute("placeholder", this.PlaceHolder);
            if (!string.IsNullOrEmpty(this.AutoCompleteOnOff))
            {
                output.Attributes.SetAttribute("autocomplete", this.AutoCompleteOnOff);
            }
            if (this.IsDisabled)
            {
                output.Attributes.SetAttribute("disabled", "disabled");
                output.Attributes.SetAttribute("id", null);
                output.Attributes.SetAttribute("name", null);
            }

            if (this.NumberMax != null)
            {
                output.Attributes.SetAttribute("max", this.NumberMax);
            }
            if (this.NumberMin != null)
            {
                output.Attributes.SetAttribute("min", this.NumberMin);
            }
            if (this.NumberStep>0)
            {
                output.Attributes.SetAttribute("step", this.NumberStep);
            }

            if (this.IsRequired)
            {
                output.Attributes.SetAttribute("required", "required");
            }

            // Přidej i validaci (musí být mimo input)
            output.PostElement.AppendHtml($"<span asp-validation-for='{this.For.Name}' class='text-danger'></span>");

            if (this.IsDisabled)
            {
                output.PostElement.AppendHtml($"<input type='hidden' name='{this.For.Name}' id='{strControlID}' value='{this.For.Model ?? ""}' />");

            }
        }
    }
}
