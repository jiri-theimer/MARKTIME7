
using Microsoft.AspNetCore.Mvc.ViewFeatures;

using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;


namespace MOB.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("myautocomplete")]
    public class myAutocompleteTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }

        [HtmlAttributeName("suggestions")]                
        public string SuggestionsStrings { get; set; }

        [HtmlAttributeName("isrequired")]
        public bool IsRequired { get; set; }

        [HtmlAttributeName("autocomplete")]
        public string AutoCompleteOnOff { get; set; }

        [HtmlAttributeName("is-attach-to-body")]
        public bool IsAttachToBody { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string strControlID = this.For.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");

            if (this.SuggestionsStrings !=null && this.SuggestionsStrings.Contains(";"))
            {
                this.SuggestionsStrings = this.SuggestionsStrings.Replace(";", ",");
            }
            
            // wrapper div
            output.TagName = "div";
            output.Attributes.SetAttribute("id", $"autocomplete-root-{strControlID}");
            output.Attributes.SetAttribute("class", "relative w-full");

            var sb = new StringBuilder();

            if (this.AutoCompleteOnOff == null)
            {
                this.AutoCompleteOnOff = "off";
            }
            // viditelný input
            sb.AppendLine($@"
                <input id='autocomplete-input-{strControlID}'
                       type='text'
                       class='input input-bordered bg-base-100 text-base-content w-full placeholder:text-base-content/50{(this.IsRequired ? " validator": "")}'
                       placeholder='{this.PlaceHolder}'
                       autocomplete='{this.AutoCompleteOnOff}'                       
                       aria-autocomplete='list'
                       aria-controls='autocomplete-listbox-{strControlID}'
                       aria-expanded='false'
                       role='combobox'
                       aria-activedescendant=''                               
                       aria-haspopup='listbox'
                       aria-owns='autocomplete-listbox-{strControlID}'
                       {(this.IsRequired ? "required='required'" : "")}
                       aria-describedby='autocomplete-status-{strControlID}' />");

            // hidden input
            sb.AppendLine($"<input type='hidden' id='{strControlID}' value='{this.For.Model ?? ""}' name='{this.For.Name}' />");

            if (this.IsRequired)
            {
                output.PostElement.AppendHtml($"<span asp-validation-for='autocomplete-input-{strControlID}' class='text-danger'></span>");
            }
            


            // listbox
            sb.AppendLine($@"
                <ul id='autocomplete-listbox-{strControlID}' 
                    class='absolute z-50 mt-1 hidden max-h-60 w-full divide-y divide-gray-100 overflow-auto rounded-md border border-gray-300 bg-white shadow-lg' 
                    role='listbox' tabindex='-1'></ul>");

            // status div
            sb.AppendLine($@"<div id='autocomplete-status-{strControlID}' class='sr-only' aria-live='polite'></div>");

            sb.AppendLine("<script>");            
            
            sb.AppendLine($"_myautocomplete_init('{strControlID}','{this.SuggestionsStrings}',{(this.IsAttachToBody ? "true" : "false")});");

            sb.AppendLine("</script>");

            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
