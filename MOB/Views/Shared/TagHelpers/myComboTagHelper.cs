
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;


namespace MOB.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mycombo")]
    public class myComboTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        
        [HtmlAttributeName("selected-text")]
        public ModelExpression SelectedText { get; set; }

        [HtmlAttributeName("ajax-prefix")]
        public string AjaxPrefix { get; set; }

        [HtmlAttributeName("placeholder")]
        public string Placeholder { get; set; }

        [HtmlAttributeName("suggestions")]
        public IEnumerable<MOB.Models.Mycontrols.MyComboItem> Suggestions { get; set; }

        [HtmlAttributeName("autocomplete")]
        public string Autocomplete { get; set; }

        [HtmlAttributeName("event-onchange")]
        public string Event_OnChange { get; set; }

        [HtmlAttributeName("isrequired")]
        public bool IsRequired { get; set; }

        [HtmlAttributeName("is-attach-to-body")]
        public bool IsAttachToBody { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string strControlID = this.For.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");

            output.TagName = "div";
            output.Attributes.SetAttribute("class", "relative w-full");

            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine($"<input type='text' id='combo-input-{strControlID}' autocomplete='off' placeholder='{this.Placeholder}' {(this.IsRequired ? "required" : null)} class='input input-bordered w-full validator pr-10' />");

            sb.AppendLine($"<button type='button' tabindex='-1' id='combo-clearbtn-{strControlID}' class='cursor-pointer absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 hidden'>&#10005;</button>");

            if (this.IsRequired)
            {
                sb.AppendLine($"<span asp-validation-for='combo-input-{strControlID}' class='text-danger'></span>");
            }

            sb.AppendLine($"<input type='hidden' id='{strControlID}' value='{this.For.Model ?? ""}' name='{this.For.Name}' />");

            sb.AppendLine($"<ul id='combo-list-{strControlID}' tabindex='-1' class='menu menu-compact bg-base-100 absolute z-20 mt-1 grid hidden max-h-64 w-full gap-1 overflow-y-auto rounded-md border shadow-lg'></ul>");


            if (this.Suggestions == null)
            {
                this.Suggestions = new List<MOB.Models.Mycontrols.MyComboItem>().AsEnumerable();
            }
            var jsonSuggestions = System.Text.Json.JsonSerializer.Serialize(this.Suggestions);


            string strSelectedTextID = null;
            if (this.SelectedText != null)
            {
                strSelectedTextID = this.SelectedText.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");
                sb.AppendLine($"<input type='hidden' id='{strSelectedTextID}' value='{this.SelectedText.Model ?? ""}' name='{this.SelectedText.Name}' />");
            }

            sb.AppendLine("<script>");

            

            sb.AppendLine($"var jsonData={jsonSuggestions};");

            if (this.Event_OnChange != null)
            {
                sb.AppendLine($"_mycombo_init('{strControlID}','{this.AjaxPrefix}','{strSelectedTextID}',jsonData,{(this.IsAttachToBody ? "true" : "false")},() => {this.Event_OnChange});");                
            }
            else
            {
                sb.AppendLine($"_mycombo_init('{strControlID}','{this.AjaxPrefix}','{strSelectedTextID}',jsonData,{(this.IsAttachToBody ? "true" : "false")});");
            }

            

            sb.AppendLine("</script>");


            output.Content.SetHtmlContent(sb.ToString());

        }
    }
}
